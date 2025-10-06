using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/GrappleStationary")]
public class GrappleStationary : Ability
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
    public LayerMask grappleSurface;

    [Range(0.5f, 1f)]
    public float maxGrappleExtend;

    [Range(0f, 0.5f)]
    public float minGrappleExtend;

    public float grappleSpringForce;

    public float grappleDamper;

    public float grappleMassScale;

    private GameObject previewObject;

    private Vector3 grappleLocation;
    private Ray ray;

    private SpringJoint joint;

    public override void OnInstantiate()
    {
        canAbility = true;
        isPreview = true;
    }
    public override void AbilityPreview(PlayerContext ctx)
    {
        cameraTransform = ctx.cameraTransform;

        ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (!usingAbility && Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, grappleSurface))
        {
            if (previewObject == null)
            {
                previewObject = Instantiate(previewPrefab, hit.point, Quaternion.identity);
            }
            else
            {
                previewObject.transform.position = hit.point;

                Vector3 awayDirection = -hit.normal;
                Quaternion rotationAway = Quaternion.LookRotation(awayDirection);

                previewObject.transform.rotation = rotationAway;
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

        if (canAbility)
        {
            canAbility = false;
            stateHandler.isGrappling = true;

            ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance,grappleSurface))
            {
                grappleLocation = hit.point;
                usingAbility = true;

                joint = playerObject.AddComponent<SpringJoint>();

                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grappleLocation;

                float distanceFromPoint = Vector3.Distance(playerObject.transform.position, grappleLocation);

                joint.maxDistance = distanceFromPoint * maxGrappleExtend;
                joint.minDistance = distanceFromPoint * minGrappleExtend;

                joint.spring = grappleSpringForce;
                joint.damper = grappleDamper;
                joint.massScale = grappleMassScale;
            }

            abilityManager.StartAbilityDuration(abilityIndex, duration);
        }

    }

    public override void AbilityInUse(PlayerContext ctx)
    {

    }

    public override void AbilityEnd()
    {
        GrappleEnd();
    }

    public override void DeActivate(PlayerContext ctx)
    {
        GrappleEnd();
    }

    private void GrappleEnd()
    {
        stateHandler.isGrappling = false;
        usingAbility = false;
        Destroy(joint);
        abilityManager.StartAbilityCooldown(abilityIndex, cooldown);
    }

    public override void CooldownEnd()
    {
        canAbility = true;
    }


}
