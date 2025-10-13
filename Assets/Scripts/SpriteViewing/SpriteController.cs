using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    [Serializable]
    public class SpriteSetEntry
    {
        public MovementState state;
        public SpriteSet spriteSet;
    }

    [Header("Refs")]
    [SerializeField]
    public List<SpriteSetEntry> spriteSetsList = new();
    private Dictionary<MovementState, SpriteSet> spriteSets = new();
    public SpriteRenderer targetRenderer;
    public Transform viewer;
    public PlayerStateHandler playerStateHandler;

    [Header("Current State")]
    public MovementState currentMovementState = MovementState.idle;

    [Header("Behavior")]
    [Tooltip("Degrees above the horizontal plane to switch to Top; below to switch to Bottom.")]
    public float topBottomPitchThresholdDeg = 45f;

    [Tooltip("Yaw sector size for 8-way directions.")]
    public float yawSectorSizeDeg = 45f;

    [Tooltip("Optional FPS override. <= 0 uses SpriteSet.defaultFps.")]
    public float fpsOverride = 0f;

    [Tooltip("Optional small hysteresis to reduce flicker at boundaries.")]
    public float hysteresisDeg = 2f;

    [Header("Billboard (yaw-only)")]
    public Transform billboardPivot;
    public bool enableYawBillboard = true;

    private int _frameIdx = 0;
    private float _timer = 0f;
    private SpriteSet.Direction _currentDir = SpriteSet.Direction.S;
    private SpriteSet.Direction _lastStableDir = SpriteSet.Direction.S;

    void Reset()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
        if (targetRenderer != null) billboardPivot = targetRenderer.transform;
    }

    void Awake()
    {
        // Convert list to dictionary for fast lookups
        spriteSets.Clear();
        foreach (var entry in spriteSetsList)
        {
            if (entry.spriteSet != null && !spriteSets.ContainsKey(entry.state))
            {
                spriteSets[entry.state] = entry.spriteSet;
            }
        }
    }

    // void OnEnable()
    // {
    //     if (playerStateHandler != null)
    //         playerStateHandler.OnStateChanged += HandleStateChanged;
    // }

    // void OnDisable()
    // {
    //     if (playerStateHandler != null)
    //         playerStateHandler.OnStateChanged -= HandleStateChanged;
    // }

    private void HandleStateChanged(MovementState newState)
    {
        currentMovementState = newState;
        // Reset animation when state changes
        _frameIdx = 0;
        _timer = 0f;
    }

    void Update()
    {
        if (!IsValidState())
            return;

        SpriteSet spriteSet = spriteSets[currentMovementState];

        Vector3 toViewerLocal = GetViewerDirectionLocal();
        if (toViewerLocal == Vector3.zero) return;

        UpdateSpriteDirection(toViewerLocal);
        AdvanceAnimation(spriteSet);
        ApplySprite(spriteSet);
    }

    private bool IsValidState()
    {
        return spriteSets.ContainsKey(currentMovementState) 
            && spriteSets[currentMovementState] != null 
            && targetRenderer != null 
            && viewer != null;
    }

    private Vector3 GetViewerDirectionLocal()
    {
        Vector3 toViewerWorld = viewer.position - transform.position;
        if (toViewerWorld.sqrMagnitude < 1e-6f) return Vector3.zero;
        return transform.InverseTransformDirection(toViewerWorld);
    }

    private void UpdateSpriteDirection(Vector3 toViewerLocal)
    {
        float pitchDeg = CalculatePitch(toViewerLocal);
        SpriteSet.Direction chosen = DetermineDirection(pitchDeg, toViewerLocal);

        if (chosen != _currentDir)
        {
            _currentDir = chosen;
            _lastStableDir = chosen;
            _frameIdx = 0;
            _timer = 0f;
        }
    }

    private float CalculatePitch(Vector3 toViewerLocal)
    {
        float horizMag = new Vector2(toViewerLocal.x, toViewerLocal.z).magnitude;
        return Mathf.Atan2(toViewerLocal.y, horizMag) * Mathf.Rad2Deg;
    }

    private SpriteSet.Direction DetermineDirection(float pitchDeg, Vector3 toViewerLocal)
    {
        float tHigh = topBottomPitchThresholdDeg;
        float tLow = topBottomPitchThresholdDeg - hysteresisDeg;
        float bLow = -topBottomPitchThresholdDeg;
        float bHigh = -topBottomPitchThresholdDeg + hysteresisDeg;

        if (_lastStableDir == SpriteSet.Direction.T && pitchDeg >= tLow)
            return SpriteSet.Direction.T;
        
        if (_lastStableDir == SpriteSet.Direction.B && pitchDeg <= bHigh)
            return SpriteSet.Direction.B;

        if (pitchDeg >= tHigh)
            return SpriteSet.Direction.T;
        
        if (pitchDeg <= bLow)
            return SpriteSet.Direction.B;

        return ComputeYawDirection4Way(toViewerLocal);
    }

    private void AdvanceAnimation(SpriteSet spriteSet)
    {
        float fps = (fpsOverride > 0f) ? fpsOverride : Mathf.Max(1f, spriteSet.defaultFps);
        _timer += Time.deltaTime;
        if (_timer >= (1f / fps))
        {
            _timer -= (1f / fps);
            _frameIdx++;
        }
    }

    private void ApplySprite(SpriteSet spriteSet)
    {
        Sprite s = spriteSet.GetFrame(_currentDir, _frameIdx);
        if (s != null)
            targetRenderer.sprite = s;
    }

    void LateUpdate()
    {
        if (!enableYawBillboard || billboardPivot == null || viewer == null) return;

        Vector3 toViewer = viewer.position - billboardPivot.position;
        toViewer.y = 0f;

        if (toViewer.sqrMagnitude > 1e-6f)
        {
            Quaternion lookYaw = Quaternion.LookRotation(toViewer.normalized, Vector3.up);
            billboardPivot.rotation = lookYaw;
        }
    }

    private SpriteSet.Direction ComputeYawDirection(Vector3 toViewerLocal)
    {
        // Guard: if horizontal magnitude is tiny, fallback to front
        Vector2 xz = new(toViewerLocal.x, toViewerLocal.z);
        if (xz.sqrMagnitude < 1e-6f)
            return SpriteSet.Direction.N;

        // Yaw angle around Y in degrees, where:
        // 0 = forward (N), +90 = right (W), 180 = back (S), -90/270 = left (E)
        float yawDeg = Mathf.Atan2(toViewerLocal.x, toViewerLocal.z) * Mathf.Rad2Deg;

        // Map to 0..360
        if (yawDeg < 0f) yawDeg += 360f;

        // Quantize into 8 slices centered every 45 deg with half-width 22.5 deg:
        // 0=N, 45=NW, 90=W, 135=SW, 180=S, 225=SE, 270=E, 315=NE
        int sector = Mathf.RoundToInt(yawDeg / yawSectorSizeDeg) % 8;

        switch (sector)
        {
            case 0: return SpriteSet.Direction.N;
            case 1: return SpriteSet.Direction.NW;
            case 2: return SpriteSet.Direction.W;
            case 3: return SpriteSet.Direction.SW;
            case 4: return SpriteSet.Direction.S;
            case 5: return SpriteSet.Direction.SE;
            case 6: return SpriteSet.Direction.E;
            case 7: return SpriteSet.Direction.NE;
            default: return SpriteSet.Direction.N;
        }
    }

    private SpriteSet.Direction ComputeYawDirection4Way(Vector3 toViewerLocal)
    {
        // Guard: if horizontal magnitude is tiny, fallback to front
        Vector2 xz = new(toViewerLocal.x, toViewerLocal.z);
        if (xz.sqrMagnitude < 1e-6f)
            return SpriteSet.Direction.N;

        // Yaw angle around Y in degrees, where:
        // 0 = forward (N), +90 = right (W), 180 = back (S), -90/270 = left (E)
        float yawDeg = Mathf.Atan2(toViewerLocal.x, toViewerLocal.z) * Mathf.Rad2Deg;

        // Map to 0..360
        if (yawDeg < 0f) yawDeg += 360f;

        // Quantize into 4 slices centered every 90 deg with half-width 45 deg:
        // 0=N (315-45), 90=W (45-135), 180=S (135-225), 270=E (225-315)
        int sector = Mathf.RoundToInt(yawDeg / 90f) % 4;

        switch (sector)
        {
            case 0: return SpriteSet.Direction.N;  // Front
            case 1: return SpriteSet.Direction.W;  // Right
            case 2: return SpriteSet.Direction.S;  // Back
            case 3: return SpriteSet.Direction.E;  // Left
            default: return SpriteSet.Direction.N;
        }
    }
}
