using UnityEngine;
using UnityEngine.UIElements;
using System;

public class SketchifyButtons : MonoBehaviour
{
    [SerializeField] private UIDocument doc;

    void Awake()
    {
        var root = doc.rootVisualElement;

        // pick your button by name or class
        var btn = root.Q<Button>(className: "sketch");
        AddScribbleOutline(btn, ink: new Color(0, 0, 0, 0.9f), lineWidth: 2f, jitter: 1.6f);
    }

    void AddScribbleOutline(VisualElement target, Color ink, float lineWidth, float jitter)
    {
        if (target == null)
        {
            Debug.Log("SketchifyButtons: target is null");
            return;
        }
        var fx = new VisualElement { pickingMode = PickingMode.Ignore };
        fx.style.position = Position.Absolute;
        fx.style.left = 0; fx.style.top = 0; fx.style.right = 0; fx.style.bottom = 0;

        // Redraw when size changes
        target.RegisterCallback<GeometryChangedEvent>(_ => fx.MarkDirtyRepaint());

        fx.generateVisualContent += ctx =>
        {
            var r = target.contentRect;
            var p = ctx.painter2D;
            p.lineWidth = lineWidth;
            p.strokeColor = ink;
            p.fillColor = Color.clear;

            // Draw 2–3 passes with tiny randomness = “hand drawn”
            for (int pass = 0; pass < 3; pass++)
            {
                float j = jitter * (1f + pass * 0.25f);
                var rnd = new System.Random(target.GetHashCode() + pass * 97);

                Vector2 J(Vector2 v) => new Vector2(v.x + Rand(rnd, j), v.y + Rand(rnd, j));

                p.BeginPath();
                var a = J(new Vector2(r.xMin + 6, r.yMin + 6));
                var b = J(new Vector2(r.xMax - 6, r.yMin + 6));
                var c = J(new Vector2(r.xMax - 6, r.yMax - 6));
                var d = J(new Vector2(r.xMin + 6, r.yMax - 6));

                p.MoveTo(a); p.LineTo(b); p.LineTo(c); p.LineTo(d); p.ClosePath();
                p.Stroke();
            }
        };

        target.Add(fx);
    }

    static float Rand(System.Random rnd, float range)
        => (float)(rnd.NextDouble() * 2 - 1) * range;
}