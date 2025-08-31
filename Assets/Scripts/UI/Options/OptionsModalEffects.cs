using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsModalEffects : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    public enum ScribblePreset { MarkerThick, TornEdge }

    [Header("Border")]
    public ScribblePreset scribblePreset = ScribblePreset.MarkerThick;
    [Range(1f, 12f)] public float stroke = 3f;
    public float jitterAmplitude = 4f;
    public Color inkColor = new Color(0.12f, 0.12f, 0.12f, 1f);

    private VisualElement scribbleLayer;

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        scribbleLayer = root.Q<VisualElement>("optionsScribble");
        if (scribbleLayer != null)
            scribbleLayer.generateVisualContent += PaintScribbleBorder;
    }

    // --- Painter ---

    private void PaintScribbleBorder(MeshGenerationContext ctx)
    {
        var r = ctx.visualElement.contentRect;
        if (r.width <= 0 || r.height <= 0) return;

        var pts = BuildJitteredRectPath(r, 48);
        var p = ctx.painter2D;
        p.fillColor = Color.clear;

        switch (scribblePreset)
        {

            case ScribblePreset.MarkerThick:
                p.strokeColor = inkColor;
                p.lineWidth = stroke * 2.4f;
                p.BeginPath(); p.MoveTo(pts[0]);
                for (int i = 1; i < pts.Count; i++) p.LineTo(pts[i]);
                p.ClosePath(); p.Stroke();

                p.lineWidth = stroke * 1.2f;
                p.BeginPath(); p.MoveTo(pts[0] + new Vector2(0.8f, 0.5f));
                for (int i = 1; i < pts.Count; i++) p.LineTo(pts[i] + new Vector2(0.8f, 0.5f));
                p.ClosePath(); p.Stroke();
                break;

            case ScribblePreset.TornEdge:
                p.strokeColor = inkColor; p.lineWidth = stroke * 0.9f;
                DrawRaggedPolyline(p, pts);
                break;
        }
    }

    // --- Helpers used by the border ---

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
            Vector2 dir = (ept - s);

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
        pts.Add(pts[0]);
        return pts;
    }

    private void DrawRaggedPolyline(Painter2D p, List<Vector2> pts)
    {
        if (pts == null || pts.Count < 2) return;

        var rag = new List<Vector2>(pts.Count * 2);
        for (int i = 0; i < pts.Count - 1; i++)
        {
            Vector2 a = pts[i], b = pts[i + 1];
            Vector2 dir = (b - a).normalized;
            Vector2 n = new Vector2(-dir.y, dir.x);
            rag.Add(a);
            if (i % 5 == 0)
                rag.Add(a + n * Random.Range(jitterAmplitude * 0.5f, jitterAmplitude * 1.2f));
        }
        rag.Add(pts[^1]);

        p.BeginPath(); p.MoveTo(rag[0]);
        for (int i = 1; i < rag.Count; i++) p.LineTo(rag[i]);
        p.ClosePath(); p.Stroke();
    }
}
