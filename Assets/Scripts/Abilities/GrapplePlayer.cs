using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/GrapplePlayer")]
public class GrapplePlayer : Ability
{
    private GameObject playerObject;
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;
    private PlayerGrappleLine grappleLineScript;
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
        grappleLineScript = ctx.grappleLine;
        orintation = ctx.orintation;
        this.abilityIndex = abilityIndex;

        if (canAbility)
        {

            ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, playerLayer))
            {
                stateHandler.isGrappling = true;
                usingAbility = true;

                grappleLocation = hit.point;
                horizontalForce = orintation.forward * horizontalStrength;
                grappleLineScript.SetEndPoint(hit.point);

                rb.linearVelocity = new Vector3(rb.linearVelocity.x,0f,rb.linearVelocity.z);

                // if (GameplayClient.instance != null) GameplayClient.instance.SendAbilityDataGrapplePlayer();
                abilityManager.StartAbilityDuration(abilityIndex, duration);
                canAbility = false;
            }
        }

    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        verticalForce = new Vector3(0, verticalStrength, 0);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x,verticalForce.y,rb.linearVelocity.z);
        rb.AddForce(horizontalForce, ForceMode.Impulse);


    }

    public override void AbilityEnd()
    {
        rb.AddForce(horizontalForce, ForceMode.Impulse);
        GrappleEnd();
    }

    public override void DeActivate(PlayerContext ctx)
    {

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
