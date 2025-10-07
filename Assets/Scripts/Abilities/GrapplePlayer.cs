using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/GrapplePlayer")]
public class GrapplePlayer : Ability
{
    private GameObject playerObject;
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;
    private Transform orintation;
    private Transform cameraTransform;

    [Header("Grapple Info")]

    public GameObject previewPrefab;

    public float grappleSpeed = 5f;
    public float maxGrappleDistance = 10f;
    public float verticalStrength = 10f;
    public float horizontalStrength = 10f;
    public LayerMask playerLayer;

    private Vector3 verticalForce;
    private Vector3 horizontalForce;

    private Vector3 grappleLocation;
    private Ray ray;
    private GameObject previewObject;

    public override void OnInstantiate()
    {
        canAbility = true;
        isPreview = true;
    }
    public override void AbilityPreview(PlayerContext ctx)
    {
        cameraTransform = ctx.cameraTransform;

        ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (!usingAbility && Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, playerLayer))
        {
            if (previewObject == null)
            {
                previewObject = Instantiate(previewPrefab, hit.transform.position, cameraTransform.rotation);
            }
            else
            {
                previewObject.transform.position = hit.transform.position;

                previewObject.transform.rotation = cameraTransform.rotation;
            }
        }
        else if (!usingAbility)
        {
            Destroy(previewObject);
            previewObject = null;
        }
        
    }
    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        playerObject = ctx.gameObject;
        rb = ctx.rb;
        abilityManager = ctx.abilityManager;
        cameraTransform = ctx.cameraTransform;
        stateHandler = ctx.stateHandler;
        orintation = ctx.orintation;
        this.abilityIndex = abilityIndex;

        if (canAbility)
        {
            canAbility = false;

            ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, playerLayer))
            {
                stateHandler.isGrappling = true;
                grappleLocation = hit.point;
                usingAbility = true;
                horizontalForce = orintation.forward * horizontalStrength;
                
            }

            abilityManager.StartAbilityDuration(abilityIndex, duration);
        }

    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        verticalForce = new Vector3(0, verticalStrength, 0);
        rb.AddForce(horizontalForce + verticalForce, ForceMode.Impulse);
    }

    public override void AbilityEnd()
    {
        GrappleEnd();
    }

    public override void DeActivate(PlayerContext ctx)
    {
        abilityManager.EndAbilityEarly(abilityIndex);
        GrappleEnd();
    }

    private void GrappleEnd()
    {
        stateHandler.isGrappling = false;
        usingAbility = false;
        abilityManager.StartAbilityCooldown(abilityIndex, cooldown);
    }

    public override void CooldownEnd()
    {
        canAbility = true;
    }


}
