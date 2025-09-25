using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class Dash : Ability
{
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;
    private Transform cameraTransform;
    public float dashForce = 10f;
    public float yDamping = 3f;
    

    public override void OnInstantiate()
    {
        canAbility = true;
    }

    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        rb = ctx.rb;
        stateHandler = ctx.stateHandler;
        abilityManager = ctx.abilityManager;
        movementScript = ctx.movement;
        cameraTransform = ctx.cameraTransform;

        this.abilityIndex = abilityIndex;

        if (canAbility)
        {
            stateHandler.isDashing = true;
            rb.useGravity = false;

            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            abilityManager.StartAbilityDuration(abilityIndex, duration);
            usingAbility = true;
            canAbility = false;
            
        }
    }
    public override void AbilityInUse(PlayerContext ctx)
    {
        // cameraTransform = ctx.cameraTransform;

        Debug.Log("Activating Dash");
        Debug.Log(cameraTransform.forward * dashForce);
        Vector3 forceDirection = new Vector3(cameraTransform.forward.x * dashForce, 0, cameraTransform.forward.z * dashForce);

        rb.AddForce(forceDirection, ForceMode.Impulse);
        abilityManager.StartAbilityDuration(abilityIndex, duration);
    }
    public override void AbilityEnd()
    {
        rb.useGravity = true;
        usingAbility = false;
        stateHandler.isDashing = false;

        abilityManager.StartAbilityCooldown(abilityIndex, cooldown);
    }
    public override void CooldownEnd()
    {
        canAbility = true;
    }

    // Does nothing here
    public override void DeActivate(PlayerContext ctx)
    {

    }

}
