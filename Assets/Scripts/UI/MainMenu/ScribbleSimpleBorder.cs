using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScribbleBorderSimple : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [Header("Target")]
    public string targetElementName = "profileScribble";

    [Header("Marker Thick Border")]
    [Range(1f, 12f)] public float stroke = 6f;
    public Color inkColor = new Color(0.12f, 0.12f, 0.12f, 1f);
    public float jitterAmplitude = 4f;

    private VisualElement target;

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        target = root.Q<VisualElement>(targetElementName);
        if (target != null) target.generateVisualContent += PaintMarkerThick;
    }

    private void PaintMarkerThick(MeshGenerationContext ctx)
    {
        var r = ctx.visualElement.contentRect;
        if (r.width <= 0 || r.height <= 0) return;

        var pts = BuildJitteredRectPath(r, 48);
        var p = ctx.painter2D;
        p.fillColor = Color.clear;

        // Fat pass
        p.strokeColor = inkColor;
        p.lineWidth = stroke * 2.4f;
        p.BeginPath(); p.MoveTo(pts[0]);
        for (int i = 1; i < pts.Count; i++) p.LineTo(pts[i]);
        p.ClosePath(); p.Stroke();

        // Offset pass for marker feel
        p.lineWidth = stroke * 1.2f;
        p.BeginPath(); p.MoveTo(pts[0] + new Vector2(0.8f, 0.5f));
        for (int i = 1; i < pts.Count; i++) p.LineTo(pts[i] + new Vector2(0.8f, 0.5f));
        p.ClosePath(); p.Stroke();
    }

    // Static jittered rectangle (no animation)
    private List<Vector2> BuildJitteredRectPath(Rect r, int segPerEdge)
    {
        var pts = new List<Vector2>(segPerEdge * 4 + 1);
        Vector2[] corners = {
            new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
            new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)
        };

        for (int e = 0; e < 4; e++)
        {
            Vector2 s = corners[e];
            Vector2 ept = corners[(e + 1) % 4];
            Vector2 dir = ept - s;

            for (int i = 0; i <= segPerEdge; i++)
            {
                float u = i / (float)segPerEdge;
                Vector2 p = s + dir * u;

                Vector2 n = new Vector2(-dir.y, dir.x).normalized;
                float wob = Mathf.Sin((u + e) * 6.283f * 1.6f) * 0.5f
                          + Mathf.Sin((u + e * 0.37f) * 6.283f * 3.7f) * 0.35f;

                p += n * wob * jitterAmplitude;
                p += dir.normalized * Mathf.Sin(u * 6.283f * 2.0f) * 0.4f;
                pts.Add(p);
            }
        }
        pts.Add(pts[0]); // close
        return pts;
    }
}
