using System;
using UnityEngine;

// cannot use generics, unity doesnt support number restraint
[Serializable]
public class ModifiableInteger : IModifiable
{
#if UNITY_EDITOR
    public int BaseValue { get => baseValue; set => baseValue = value; }
#endif
    [SerializeField] protected int baseValue;

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
    public ModifiableInteger() { }
    public ModifiableInteger(int defValue)
    {
        baseValue = defValue;
        ((IModifiable)this).Init();
    }

    public static int operator -(ModifiableInteger i)
        => -i.currentValue;
    public static int operator +(ModifiableInteger i)
        => i.currentValue;

    public static int operator -(ModifiableInteger i, int a)
        => i.currentValue - a;
    public static int operator +(ModifiableInteger i, int a)
        => i.currentValue + a;

    public static bool operator <(ModifiableInteger i, int a)
        => i.currentValue < a;
    public static bool operator >(ModifiableInteger i, int a)
        => i.currentValue > a;

    public static bool operator ==(ModifiableInteger i, int a)
        => i.currentValue == a;
    public static bool operator !=(ModifiableInteger i, int a)
        => i.currentValue != a;
    public static bool operator <=(ModifiableInteger i, int a)
        => i.currentValue <= a;
    public static bool operator >=(ModifiableInteger i, int a)
        => i.currentValue >= a;

    public override bool Equals(object obj)
    {
        if (obj == null || obj is not ModifiableInteger)
            return false;
        return baseValue == (obj as ModifiableInteger).baseValue;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
