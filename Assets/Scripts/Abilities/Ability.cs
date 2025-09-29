using System;
using JetBrains.Annotations;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;


public abstract class Ability : ScriptableObject
{
    [Header("General Ability Info")]
    public string abilityName; // name of the ability
    public Sprite icon; // icon for UI
    public float cooldown = 1f; // Set to zero if it has no cooldown
    public float duration = 1f; // Set to zero if it has an infinite duration
    public int abilityIndex = -1; // leave as -1 until assigned to player
    public bool canAbility = true; // whether the ability can be used or not (on cooldown or not)
    public bool usingAbility = false; // whether the ability is currently being used (for duration abilities)

    public abstract void OnInstantiate();

    public abstract void Activate(PlayerContext ctx, int abilityIndex); 

    public abstract void DeActivate(PlayerContext ctx);

    public abstract void AbilityInUse(PlayerContext ctx); 

    public abstract void AbilityEnd();

    public abstract void CooldownEnd();
}
