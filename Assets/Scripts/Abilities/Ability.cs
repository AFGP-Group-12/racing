using System;
using JetBrains.Annotations;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;


public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public float cooldown = 1f;
    public float duration = 1f; // Set to zero if it has an infinite duration
    public int abilityIndex = -1;
    public bool canAbility = true;


    public abstract void Activate(PlayerContext ctx, int abilityIndex);

    public abstract void DeActivate(PlayerContext ctx);

    public abstract void AbilityEnd();

    public abstract void CooldownEnd();
}
