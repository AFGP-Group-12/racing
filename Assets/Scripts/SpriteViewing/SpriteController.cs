using UnityEngine;

public class SpriteController : MonoBehaviour
{
    [Header("Refs")]
    public SpriteSet spriteSet;
    public SpriteRenderer targetRenderer;
    public Transform viewer;

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

    void Update()
    {
        if (spriteSet == null || targetRenderer == null || viewer == null)
            return;

        // Compute vector to viewer in object local space
        Vector3 toViewerWorld = viewer.position - transform.position;
        if (toViewerWorld.sqrMagnitude < 1e-6f) return;

        Vector3 toViewerLocal = transform.InverseTransformDirection(toViewerWorld);

        // Compute pitch (elevation angle) in degrees: atan2(y, horizontalMagnitude)
        float horizMag = new Vector2(toViewerLocal.x, toViewerLocal.z).magnitude;
        float pitchDeg = Mathf.Atan2(toViewerLocal.y, horizMag) * Mathf.Rad2Deg;

        // Apply Top/Bottom with a little hysteresis to reduce flicker
        float tHigh = topBottomPitchThresholdDeg;
        float tLow = topBottomPitchThresholdDeg - hysteresisDeg;
        float bLow = -topBottomPitchThresholdDeg;
        float bHigh = -topBottomPitchThresholdDeg + hysteresisDeg;

        SpriteSet.Direction chosen;

        if (_lastStableDir == SpriteSet.Direction.T)
            chosen = (pitchDeg >= tLow) ? SpriteSet.Direction.T : ComputeYawDirection(toViewerLocal);
        else if (_lastStableDir == SpriteSet.Direction.B)
            chosen = (pitchDeg <= bHigh) ? SpriteSet.Direction.B : ComputeYawDirection(toViewerLocal);
        else
        {
            if (pitchDeg >= tHigh)
                chosen = SpriteSet.Direction.T;
            else if (pitchDeg <= bLow)
                chosen = SpriteSet.Direction.B;
            else
                chosen = ComputeYawDirection(toViewerLocal);
        }

        if (chosen != _currentDir)
        {
            _currentDir = chosen;
            _lastStableDir = chosen;
            _frameIdx = 0;
            _timer = 0f;
        }

        // Advance animation
        float fps = (fpsOverride > 0f) ? fpsOverride : Mathf.Max(1f, spriteSet.defaultFps);
        _timer += Time.deltaTime;
        if (_timer >= (1f / fps))
        {
            _timer -= (1f / fps);
            _frameIdx++;
        }

        // Set sprite
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
}
