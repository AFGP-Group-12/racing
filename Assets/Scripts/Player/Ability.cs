using System;
using JetBrains.Annotations;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Scriptable Objects/Ability")]
public class Ability : ScriptableObject
{
    public String abilityName;
    public Sprite image;
    public float rarity;

    [Tooltip("If a function name is needed make sure to copy it exactly.")]
    public String functionName;

}
