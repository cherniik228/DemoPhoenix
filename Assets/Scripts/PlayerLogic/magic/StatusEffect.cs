using UnityEngine;
using System;

[Serializable]
public class StatusEffect
{
    public string effectName;
    public float duration;
    [HideInInspector] public float timer;
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
}