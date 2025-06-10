using System;
using UnityEngine;

[Serializable] public class ModifiableInteger : IModifiable
{
#if UNITY_EDITOR
    public int BaseValue { get => baseValue; set => baseValue = value; }
#endif
    [SerializeField] int baseValue;
    public int currentValue;
    public ModValue Modifier { get; set; }

    public override string ToString()
    {
        return currentValue.ToString();
    }
    
    public void RecalculateMod()
    {
        currentValue = Mathf.RoundToInt(baseValue * Modifier.percentMod) + Modifier.absoluteMod;
    }
}
