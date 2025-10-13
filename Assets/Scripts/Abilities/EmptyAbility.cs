using UnityEngine;

[CreateAssetMenu(fileName = "Abilities/EmptyAbility")]
public class EmptyAbility : Ability
{
    public override void OnInstantiate()
    {
        // Do nothing
    }

    public override void AbilityPreview(PlayerContext ctx)
    {
        // Does nothing here
    }

    public override void Activate(PlayerContext ctx, int abilityIndex)
    {
        // Do nothing
    }

    public override void DeActivate(PlayerContext ctx)
    {
        // Do nothing
    }

    public override void AbilityEnd()
    {
        // Do nothing
    }

    public override void CooldownEnd()
    {
        // Do nothing
    }

    public override void AbilityInUse(PlayerContext ctx)
    {
        // Do nothing
    }
}
