using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Platform")]
public class Platform : Ability
{
    private Rigidbody rb;
    private PlayerAbilityManager abilityManager;
    private Transform cameraTransform;


    [Header("Platform Info")]
    public GameObject platformPrefab;
    public GameObject previewPrefab;
    public float minScale;
    public float maxScale;
    public float increaseIncrement;
    public float decreaseIncrement;
    public float placementDistance;
    public LayerMask hitLayers;


    private Vector3 spawnCoordinates;
    private GameObject previewObject;
    private PreviewPlatform previewScript;
    private Ray ray;

    public override void OnInstantiate()
    {
        canAbility = true;
    }
    public override void AbilityPreview(PlayerContext ctx)
    {
        // Does nothing here
    }
    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        rb = ctx.rb;
        abilityManager = ctx.abilityManager;
        cameraTransform = ctx.cameraTransform;
        this.abilityIndex = abilityIndex;

        if (canAbility)
        {
            usingAbility = true;
            canAbility = false;

            ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, placementDistance, hitLayers))
            {
                spawnCoordinates = hit.point;
            }
            else
            {
                spawnCoordinates = ray.origin + ray.direction * placementDistance;
            }
            previewObject = Instantiate(previewPrefab, spawnCoordinates, Quaternion.identity);
            previewScript = previewObject.GetComponent<PreviewPlatform>();

            abilityManager.StartAbilityDuration(abilityIndex, duration);
        }
    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        ray = new Ray(cameraTransform.position, cameraTransform.forward);
        //Debug.DrawRay(cameraTransform.position, cameraTransform.forward * placementDistance, Color.blue);

        if (Physics.Raycast(ray, out RaycastHit hit, placementDistance))
        {
            spawnCoordinates = hit.point;
        }
        else
        {
            spawnCoordinates = ray.origin + ray.direction * placementDistance;
        }

        previewScript.MoveTransform(spawnCoordinates);
        
    }
    public override void AbilityEnd()
    {
        PlacePrefab();
    }

    public override void DeActivate(PlayerContext ctx)
    {
        abilityManager.EndAbilityEarly(abilityIndex);
        PlacePrefab();
    }

    public override void CooldownEnd()
    {
        canAbility = true;
    }

    public void PlacePrefab()
    {
        if (usingAbility)
        {
            previewScript.DestroyObject();
            previewScript = null;
            previewObject = null;

            SpawnPlatform(spawnCoordinates);
            if (GameplayClient.instance != null) GameplayClient.instance.SendAbilityDataPlatform(spawnCoordinates);

            abilityManager.StartAbilityCooldown(abilityIndex, cooldown);

            usingAbility = false;
        }
    }

    public void SpawnPlatform(Vector3 pos)
    {
        GameObject tmp = Instantiate(platformPrefab, pos, Quaternion.identity);
        tmp.GetComponent<SpawnedPlatform>().SetVariables(minScale, maxScale, increaseIncrement, decreaseIncrement);
    }
}
