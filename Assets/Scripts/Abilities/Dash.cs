using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class Dash : Ability
{
    private Rigidbody rb;
    private PlayerStateHandler stateHandler;
    private PlayerAbilityManager abilityManager;
    private PlayerMovement movementScript;
    private Transform orintation;
    private Transform cameraTransform;
    private Vector3 horizontalForce;
    private Vector3 verticalForce;

    [Header("Dash Info")]
    public float dashForce = 5f;
    public float dashForceY = 6f;

    [Tooltip("If the user is trying to do an input with basically no verticallity this will help the user do so. So if the value is 0.2 and the user's y input is 0.1 the dash will take it in as 0")]
    [Range(0f, 0.5f)]
    public float dashInnerDeadzone = 0.2f; 

    [Tooltip("This value acts as the maximum y input that can be taken in by the dash. So if its 0.7 then when the user looks straight up the value will be 0.7 rather than 1")]
    [Range(0.5f, 1f)]
    public float dashOuterDeadzone = 0.7f;


    public override void OnInstantiate()
    {
        canAbility = true;
        usingAbility = false;
    }
    public override void AbilityPreview(PlayerContext ctx)
    {
        // Does nothing here
    }

    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        rb = ctx.rb;
        stateHandler = ctx.stateHandler;
        abilityManager = ctx.abilityManager;
        orintation = ctx.orintation;
        cameraTransform = ctx.cameraTransform;

        this.abilityIndex = abilityIndex;

        if (canAbility)
        {
            stateHandler.isDashing = true;
            rb.useGravity = false;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            abilityManager.StartAbilityDuration(abilityIndex, duration);
            usingAbility = true;
            canAbility = false;
            horizontalForce = orintation.forward * dashForce;

            if (cameraTransform.forward.y > dashOuterDeadzone)
            {
                verticalForce = new Vector3(0, dashOuterDeadzone, 0);
            }
            else if (math.abs(cameraTransform.forward.y) < dashInnerDeadzone)
            {
                verticalForce = new Vector3(0, 0, 0);
            }
            else if (cameraTransform.forward.y < -dashOuterDeadzone)
            {
                verticalForce = new Vector3(0, -dashOuterDeadzone, 0);
            }
            else
            {
                verticalForce = new Vector3(0, cameraTransform.forward.y, 0);
            }

            //verticalForce = new Vector3(0, cameraTransform.forward.y, 0);


            verticalForce *= dashForce;
            rb.AddForce(verticalForce * dashForceY, ForceMode.Impulse);


        }
    }
    public override void AbilityInUse(PlayerContext ctx)
    {
        // cameraTransform = ctx.cameraTransform;

        // Add vertical force here when you figure it out
        rb.AddForce(horizontalForce, ForceMode.Impulse);
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
