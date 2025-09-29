using Newtonsoft.Json;
using System;
using UnityEngine;

// cannot use generics, unity doesnt support number restraint
[Serializable]
public class ModifiableInteger : IModifiable
{
#if UNITY_EDITOR
    [JsonIgnore] public int BaseValue { get => baseValue; set => baseValue = value; }
#endif
    [SerializeField, JsonProperty] protected int baseValue;

    public int currentValue;
    [JsonIgnore] public ModValue Modifier { get => mod; set => mod = value; }
    [SerializeField, JsonProperty] ModValue mod;

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

    public void ChangeBaseVal(int v)
    {
        baseValue = v;
        RecalculateMod();
    }
}
