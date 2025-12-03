using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TextureTiling : MonoBehaviour
{
    public float tilingPerUnit = 1f;

    Renderer rendererComponent;
    MaterialPropertyBlock mpb;

    void OnEnable()
    {
        if (rendererComponent == null)
            rendererComponent = GetComponent<Renderer>();

        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        UpdateTiling();
    }

    void OnValidate()
    {
        if (rendererComponent == null)
            rendererComponent = GetComponent<Renderer>();

        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        UpdateTiling();
    }

    void UpdateTiling()
    {
        if (rendererComponent == null)
            return;

        Vector3 s = transform.localScale;

        // how many times the texture repeats based on scale
        Vector2 scale = new Vector2(s.x * tilingPerUnit, s.z * tilingPerUnit);

        // _BaseMap_ST is the tiling/offset for URP Lit base texture
        rendererComponent.GetPropertyBlock(mpb);
        mpb.SetVector("_BaseMap_ST", new Vector4(scale.x, scale.y, 0f, 0f));
        rendererComponent.SetPropertyBlock(mpb);
    }
}