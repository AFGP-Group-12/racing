using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/GrappleStationary")]
public class GrappleStationary : Ability
{
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;
    private Transform orintation;
    private Transform cameraTransform;

    [Header("Grapple Info")]
    public float grappleSpeed = 5f;
    public float maxGrappleDistance = 10f;
    public LayerMask grappleSurface;

    public Vector3 grappleLocation;
    private Ray ray;

    public override void OnInstantiate()
    {
        canAbility = true;
    }
    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        rb = ctx.rb;
        abilityManager = ctx.abilityManager;
        cameraTransform = ctx.cameraTransform;

        if (canAbility)
        {
            canAbility = false;

            ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance))
            {
                grappleLocation = hit.point;
                usingAbility = true;
            }

            abilityManager.StartAbilityDuration(abilityIndex, duration);
        }

    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        
    }

    public override void AbilityEnd()
    {
        abilityManager.StartAbilityCooldown(abilityIndex, cooldown);
    }

    public override void DeActivate(PlayerContext ctx)
    {
        abilityManager.StartAbilityCooldown(abilityIndex, cooldown);
    }

    public override void CooldownEnd()
    {
        canAbility = true;
    }


}
