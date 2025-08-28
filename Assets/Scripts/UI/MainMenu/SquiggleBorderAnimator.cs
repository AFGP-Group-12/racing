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


    [Header("Text wobble")]
    public bool animateText = true;
    public float textJitterPixels = 0.8f;
    public float textRotateDegrees = 0.6f;
    public float textFps = 30f;

    private class BorderAnim { public Button btn; public IVisualElementScheduledItem ticker; public int frame; }
    private class TextAnim { public Label lbl; public IVisualElementScheduledItem ticker; public float tAccum; }

    private readonly List<BorderAnim> _borderAnims = new();
    private readonly List<TextAnim> _textAnims = new();

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

        var buttons = root.Query<Button>(className: buttonClass).ToList();

        foreach (var b in buttons)
        {
            b.AddToClassList("squiggle-bg");          // USS handles size/repeat
            b.style.backgroundImage = _styleFrames[0];

            var anim = new BorderAnim { btn = b, frame = 0 };
            long ms = Mathf.Max(1, Mathf.RoundToInt(1000f / Mathf.Max(1f, fps)));

            anim.ticker = b.schedule.Execute(() =>
            {
                anim.frame = (anim.frame + 1) % _styleFrames.Length;
                b.style.backgroundImage = _styleFrames[anim.frame];
            }).Every(ms);

            if (animateOnHoverOnly)
            {
                anim.ticker.Pause();
                b.RegisterCallback<MouseEnterEvent>(_ => anim.ticker.Resume());
                b.RegisterCallback<MouseLeaveEvent>(_ => anim.ticker.Pause());
                b.RegisterCallback<DetachFromPanelEvent>(_ => anim.ticker.Pause());
            }

            _borderAnims.Add(anim);
        }

        if (animateText)
        {
            _textAnims.Clear();

            foreach (var b in buttons)
            {
                // Prefer a label with class "wobble-text"; otherwise first Label child.
                var lbl = b.Q<Label>(className: "wobble-text") ?? b.Q<Label>();
                if (lbl == null) continue;

                var tAnim = new TextAnim { lbl = lbl, tAccum = 0f };
                long textMs = Mathf.Max(1, Mathf.RoundToInt(1000f / Mathf.Max(1f, textFps)));
                float seed = UnityEngine.Random.value * 1000f;   // per-label phase

                tAnim.ticker = lbl.schedule.Execute(() =>
                {
                    tAnim.tAccum += textMs / 1000f;
                    float dx = Mathf.Sin(tAnim.tAccum * 6.2f + seed) * textJitterPixels;
                    float dy = Mathf.Cos(tAnim.tAccum * 5.1f + seed * 1.3f) * textJitterPixels;
                    float ang = Mathf.Sin(tAnim.tAccum * 3.7f + seed * 2.0f) * textRotateDegrees;

                    lbl.style.translate = new Translate(dx, dy, 0);
                    lbl.style.rotate = new Rotate(Angle.Degrees(ang));
                }).Every(textMs);

                if (animateOnHoverOnly)
                {
                    tAnim.ticker.Pause();
                    b.RegisterCallback<MouseEnterEvent>(_ => tAnim.ticker.Resume());
                    b.RegisterCallback<MouseLeaveEvent>(_ => tAnim.ticker.Pause());
                    b.RegisterCallback<DetachFromPanelEvent>(_ => tAnim.ticker.Pause());
                }

                _textAnims.Add(tAnim);
            }
        }
    }
}
