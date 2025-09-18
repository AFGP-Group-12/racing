using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class Dash : Ability
{
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;

    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        rb = ctx.rb;
        stateHandler = ctx.stateHandler;
        abilityManager = ctx.abilityManager;
        movementScript = ctx.movement;


        this.abilityIndex = abilityIndex;

        if (canAbility)
        {
            stateHandler.isDashing = true;
            rb.AddForce(new Vector3(10, 10, 10), ForceMode.Impulse);
            abilityManager.StartAbilityDuration(abilityIndex, duration);
            canAbility = false;
        }
    }
    public override void AbilityEnd()
    {
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
