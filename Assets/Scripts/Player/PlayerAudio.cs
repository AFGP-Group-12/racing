using Unity.VisualScripting;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Integration")]
    [Tooltip("If true, the script reads horizontal speed from a Rigidbody on the same GameObject.")]
    public bool autoReadSpeedFromRigidbody = true;

    [Header("Mix")]
    [Range(0f, 1f)] public float volume = 0.9f;
    [Range(0f, 1f)] public float spatialBlend3D = 1f;
    [Range(0f, 0.5f)] public float pitchJitter = 0.07f;
    public float minFootstepSpeed = 0.6f;

    [Header("Footsteps")]
    public AudioClip[] footstepWalk;
    public AudioClip[] footstepSprint;
    public AudioClip[] footstepWallrun;
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.33f;
    public float wallrunStepInterval = 0.42f;

    [Header("Loops")]
    public AudioClip slideLoop;
    public AudioClip airLoop;
    public AudioClip wallrunLoop;

    [Header("One-shots")]
    public AudioClip dashStart;
    public AudioClip grappleFire;
    public AudioClip jumpStart;
    public AudioClip landImpact;

    MovementState _state = MovementState.idle;
    float _speed;
    float _stepTimer;

    AudioSource _oneshot;
    AudioSource _looper;
    Rigidbody _rb;
    System.Random _rng = new System.Random();

    void Awake()
    {
        _oneshot = gameObject.AddComponent<AudioSource>();
        _looper = gameObject.AddComponent<AudioSource>();
        foreach (var src in new[] { _oneshot, _looper })
        {
            src.playOnAwake = false;
            src.spatialBlend = spatialBlend3D;
            src.volume = volume;
        }
        _looper.loop = true;

        GetComponent<PlayerStateHandler>().OnStateChanged += OnStateChanged;

        if (autoReadSpeedFromRigidbody)
            _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (autoReadSpeedFromRigidbody && _rb)
        {
            var v = _rb.linearVelocity;
            _speed = new Vector3(v.x, 0f, v.z).magnitude;
        }
        TickFootsteps();
    }

    void OnStateChanged(MovementState newState)
    {
        var prev = _state;
        if (newState == prev) return;

        _state = newState;
        _stepTimer = 0f;

        Debug.Log($"PlayerAudio: State changed from {prev} to {_state}");

        switch (_state)
        {
            case MovementState.sliding:
                StartLoop(slideLoop);
                break;
            case MovementState.air:
                StartLoop(airLoop);
                break;
            case MovementState.wallrunningleft:
            case MovementState.wallrunningright:
                StartLoop(wallrunLoop);
                break;
            case MovementState.dashing:
                PlayOneShot(dashStart);
                StopLoopIfAny();
                break;
            case MovementState.grappling:
                PlayOneShot(grappleFire);
                StopLoopIfAny();
                break;
            default:
                StopLoopIfAny();
                break;
        }

        if (prev == MovementState.air &&
            (_state == MovementState.walking || _state == MovementState.sprinting || _state == MovementState.idle))
        {
            PlayOneShot(landImpact);
        }

        if ((prev == MovementState.walking || prev == MovementState.sprinting || prev == MovementState.idle) &&
            _state == MovementState.air)
        {
            Debug.Log($"play jump because state changed from {prev} to {_state}");
            PlayOneShot(jumpStart);
        }
    }

    void TickFootsteps()
    {
        AudioClip[] bank = null;
        float interval = 0f;

        switch (_state)
        {
            case MovementState.walking:
                bank = footstepWalk; interval = walkStepInterval; break;
            case MovementState.sprinting:
                bank = footstepSprint; interval = sprintStepInterval; break;
            case MovementState.wallrunningleft:
            case MovementState.wallrunningright:
                bank = footstepWallrun; interval = wallrunStepInterval; break;
        }

        if (bank == null || bank.Length == 0 || interval <= 0f) return;
        // if (_speed < minFootstepSpeed) return;


        _stepTimer -= Time.deltaTime;
        float speedScale = Mathf.Clamp(_speed / 6f, 0.6f, 1.5f);
        float scaledInterval = interval / speedScale;

        if (_stepTimer <= 0f)
        {
            PlayRandom(bank);
            _stepTimer = scaledInterval;
        }
    }

    void PlayRandom(AudioClip[] bank)
    {
        int i = _rng.Next(bank.Length);
        PlayOneShot(bank[i]);
    }

    void PlayOneShot(AudioClip clip)
    {
        if (!clip) return;
        _oneshot.pitch = 1f + ((UnityEngine.Random.value * 2f - 1f) * pitchJitter);
        _oneshot.PlayOneShot(clip, volume);
    }

    void StartLoop(AudioClip loopClip)
    {
        if (_looper.clip == loopClip) return;
        if (!loopClip) { StopLoopIfAny(); return; }

        _looper.clip = loopClip;
        _looper.pitch = 1f + ((UnityEngine.Random.value * 2f - 1f) * pitchJitter);
        _looper.volume = volume;
        _looper.Play();
    }

    void StopLoopIfAny()
    {
        if (_looper.isPlaying) _looper.Stop();
        _looper.clip = null;
    }
}
