using UnityEngine;
[CreateAssetMenu(menuName = "Abilities/Pogo")]
public class Pogo : Ability
{
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;
    private Transform orintation;
    private Transform cameraTransform;

    public LayerMask pogoLayer;
    public float pogoJumpForce;
    public float sphereRadius;
    public float pogoDistance;



    private Ray ray;


    public override void OnInstantiate()
    {
        canAbility = true;
        isPreview = true;
    }

    public override void AbilityPreview(PlayerContext ctx)
    {
        
    }

    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        rb = ctx.rb;
        abilityManager = ctx.abilityManager;
        cameraTransform = ctx.cameraTransform;
        movementScript = ctx.movement;

        this.abilityIndex = abilityIndex;

        if (canAbility)
        {
            RaycastHit hit;
            if (Physics.SphereCast(cameraTransform.position, sphereRadius, cameraTransform.forward, out hit, pogoDistance, pogoLayer))
            {
                movementScript.PogoJump(pogoJumpForce);
                usingAbility = true;
                canAbility = false;
                abilityManager.StartAbilityDuration(abilityIndex, duration);
            }
            else if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, pogoDistance, pogoLayer))
            {
                movementScript.PogoJump(pogoJumpForce);
                usingAbility = true;
                canAbility = false;
                abilityManager.StartAbilityDuration(abilityIndex, duration);
            }
        }
    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        // RaycastHit hit;
        // if (Physics.SphereCast(cameraTransform.position, sphereRadius, cameraTransform.forward, out hit, pogoDistance, pogoLayer))
        // {
        //     Debug.Log("Hit");
        // }
        // else if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, pogoDistance, pogoLayer))
        // {
        //     Debug.Log("Hit");
        // }
    }
    
    public override void DeActivate(PlayerContext ctx)
    {
        
    }

    public override void AbilityEnd()
    {
        usingAbility = false;
        abilityManager.StartAbilityCooldown(abilityIndex, duration);
    }

    public override void CooldownEnd()
    {
        canAbility = true;
    }
}
