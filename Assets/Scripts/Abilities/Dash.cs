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
            Debug.Log("Activating Dash");
            stateHandler.isDashing = true;
            rb.useGravity = false;
            rb.angularVelocity= Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            Debug.Log(ctx.cameraTransform.forward * dashForce);

            rb.AddForce(cameraTransform.forward.x * dashForce,cameraTransform.forward.y * dashForce / yDamping,cameraTransform.forward.z * dashForce,ForceMode.Impulse);
            abilityManager.StartAbilityDuration(abilityIndex, duration);
            canAbility = false;
        }
    }
    public override void AbilityEnd()
    {

        rb.useGravity = true;
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
