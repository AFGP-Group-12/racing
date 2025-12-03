using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScribbleBorderSimple : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [Header("Target")]
    public List<string> targetElementName = new List<string> { };

    [Header("Marker Thick Border")]
    [Range(1f, 12f)] public float stroke = 6f;
    public Color inkColor = new Color(0.12f, 0.12f, 0.12f, 1f);
    public float jitterAmplitude = 4f;

    private readonly List<VisualElement> subscribedElements = new();

    void Awake()
    {
        if (!uiDocument)
            uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        RegisterPanelCallbacks();
        TryBindTargets();
    }

    void OnDisable()
    {
        UnregisterPanelCallbacks();
        UnbindTargets();
    }

    private void RegisterPanelCallbacks()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;
        root.RegisterCallback<AttachToPanelEvent>(OnPanelAttached);
        root.RegisterCallback<DetachFromPanelEvent>(OnPanelDetached);
    }

    private void UnregisterPanelCallbacks()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;
        root.UnregisterCallback<AttachToPanelEvent>(OnPanelAttached);
        root.UnregisterCallback<DetachFromPanelEvent>(OnPanelDetached);
    }

    private void OnPanelAttached(AttachToPanelEvent evt)
    {
        TryBindTargets();
    }

    private void OnPanelDetached(DetachFromPanelEvent evt)
    {
        UnbindTargets();
    }

    private void TryBindTargets()
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null || root.panel == null) return;


        UnbindTargets();

        foreach (var name in targetElementName)
        {
            var t = root.Q<VisualElement>(name);
            if (t == null) continue;

            t.generateVisualContent += PaintMarkerThick;
            subscribedElements.Add(t);
        }
    }

    private void UnbindTargets()
    {
        foreach (var element in subscribedElements)
            if (element != null)
                element.generateVisualContent -= PaintMarkerThick;

        subscribedElements.Clear();
    }

    private void PaintMarkerThick(MeshGenerationContext ctx)
    {
        var r = ctx.visualElement.contentRect;
        if (r.width <= 0 || r.height <= 0) return;

        var pts = BuildJitteredRectPath(r, 48);

        var p = ctx.painter2D;
        p.fillColor = Color.clear;

        // Safer joins to avoid spikes (available in recent Unity versions)
        // If your version does not expose these, you can remove these three lines.
        p.lineJoin = LineJoin.Round;
        p.lineCap  = LineCap.Round;
        p.miterLimit = 1.5f;

        // Fat pass
        p.strokeColor = inkColor;
        p.lineWidth = stroke * 2.4f;
        p.BeginPath(); p.MoveTo(pts[0]);
        for (int i = 1; i < pts.Count; i++) p.LineTo(pts[i]);
        p.ClosePath(); p.Stroke();

        // Offset pass for marker feel
        p.lineWidth = stroke * 1.2f;
        Vector2 off = new Vector2(0.8f, 0.5f);
        p.BeginPath(); p.MoveTo(pts[0] + off);
        for (int i = 1; i < pts.Count; i++) p.LineTo(pts[i] + off);
        p.ClosePath(); p.Stroke();
    }

    // Jittered rectangle with no duplicated corner points
    private List<Vector2> BuildJitteredRectPath(Rect r, int segPerEdge)
    {
        var pts = new List<Vector2>(segPerEdge * 4);
        Vector2[] corners =
        {
            new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
            new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)
        };

        for (int e = 0; e < 4; e++)
        {
            Vector2 s = corners[e];
            Vector2 ept = corners[(e + 1) % 4];
            Vector2 dir = ept - s;
            Vector2 n = new Vector2(-dir.y, dir.x).normalized;

            // Note: i < segPerEdge (do not add the corner twice)
            for (int i = 0; i < segPerEdge; i++)
            {
                float u = i / (float)segPerEdge;

                // Exact corner at the start of each edge to keep corners consistent
                if (i == 0) { pts.Add(s); continue; }

                Vector2 p = s + dir * u;

                // Fade jitter near corners so joins remain stable
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.05f, 0.95f, u));

                float wob =
                    (Mathf.Sin((u + e) * 6.283f * 1.6f) * 0.5f +
                     Mathf.Sin((u + e * 0.37f) * 6.283f * 3.7f) * 0.35f) * fade;

                p += n * wob * jitterAmplitude;
                p += dir.normalized * Mathf.Sin(u * 6.283f * 2.0f) * 0.4f * fade;

                pts.Add(p);
            }
        }

        // Do not append pts[0]; ClosePath will close the path.
        return pts;
    }
}
