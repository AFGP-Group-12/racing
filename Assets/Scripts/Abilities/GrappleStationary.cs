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

    public override void OnInstantiate()
    {

    }
    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        
    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        
    }

    public override void AbilityEnd()
    {
        
    }

    public override void DeActivate(PlayerContext ctx)
    {
        
    }

    public override void CooldownEnd()
    {
        
    }


}
