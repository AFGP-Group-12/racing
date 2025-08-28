using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SquiggleBorderAnimator : MonoBehaviour
{
    [Header("UI Toolkit")]
    public UIDocument uiDocument;

    [Header("Selection")]
    [Tooltip("USS class to animate.")]
    public string buttonClass = "mainbutton";

    [Header("Frames")]
    [Tooltip("Resources folder containing squiggle frames. E.g. Resources/UI/Textures/SquiggleFrames")]
    public string resourcesFolder = "UI/Textures/SquiggleFrames";
    [Tooltip("Playback speed (frames per second).")]
    public float fps = 8f;
    [Tooltip("Animate only while hovered (saves perf). If false, always animates.")]
    public bool animateOnHoverOnly = true;

    private List<Sprite> _frames;
    private StyleBackground[] _styleFrames;

    private class Target
    {
        public VisualElement ve;
        public IVisualElementScheduledItem ticker;
        public int frame;
    }

    private readonly List<Target> _targets = new();

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        // Load frames
        _frames = Resources.LoadAll<Sprite>(resourcesFolder)
                  .OrderBy(s => s.name)
                  .ToList();

        if (_frames.Count == 0)
        {
            Debug.LogWarning($"SquiggleBorderAnimator: No sprites found at Resources/{resourcesFolder}");
            return;
        }

        _styleFrames = _frames.Select(f => new StyleBackground(f)).ToArray();

        var root = uiDocument.rootVisualElement;

        // Find all target buttons by class
        var buttons = root.Query<Button>(className: buttonClass).ToList();

        foreach (var b in buttons)
        {
            // Use USS for repeat/size — just give it the class
            b.AddToClassList("squiggle-bg");

            // Start on first frame
            b.style.backgroundImage = _styleFrames[0];

            var t = new Target { ve = b, frame = 0 };

            long intervalMs = Mathf.Max(1, Mathf.RoundToInt(1000f / Mathf.Max(1f, fps)));
            t.ticker = b.schedule.Execute(() =>
            {
                if (_styleFrames.Length == 0) return;
                t.frame = (t.frame + 1) % _styleFrames.Length;
                b.style.backgroundImage = _styleFrames[t.frame];
            }).Every(intervalMs);

            if (animateOnHoverOnly)
            {
                t.ticker.Pause();
                b.RegisterCallback<MouseEnterEvent>(_ => t.ticker.Resume());
                b.RegisterCallback<MouseLeaveEvent>(_ => t.ticker.Pause());
                b.RegisterCallback<DetachFromPanelEvent>(_ => t.ticker.Pause());
            }

            _targets.Add(t);
        }
    }

    void OnDisable()
    {
        foreach (var t in _targets)
            t.ticker?.Pause();
        _targets.Clear();
    }
}
