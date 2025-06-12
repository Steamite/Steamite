using UnityEngine;

public class ModifiableFloat : IModifiable
{

#if UNITY_EDITOR
    public float BaseValue { get => baseValue; set => baseValue = value; }
#endif
    [SerializeField] protected float baseValue;

    public float currentValue;
    public ModValue Modifier { get; set; }

    public override string ToString()
    {
        return currentValue.ToString();
    }

    public virtual void RecalculateMod()
    {
        currentValue = (baseValue * Modifier.percentMod) + Modifier.absoluteMod;
    }
    public ModifiableFloat() { }
    public ModifiableFloat(float defValue)
    {
        baseValue = defValue;
        ((IModifiable)this).Init();
    }

    public static float operator -(ModifiableFloat f)
        => f.currentValue;
    public static float operator +(ModifiableFloat f)
        => f.currentValue;

    public static float operator -(ModifiableFloat f, int a)
        => f.currentValue - a;
    public static float operator +(ModifiableFloat f, int a)
        => f.currentValue + a;
    public static float operator *(ModifiableFloat f, float a)
        => f.currentValue * a;

    public static bool operator <(ModifiableFloat f, int a)
        => f.currentValue < a;
    public static bool operator >(ModifiableFloat f, int a)
        => f.currentValue > a;

    public static bool operator ==(ModifiableFloat f, int a)
        => f.currentValue == a;
    public static bool operator !=(ModifiableFloat f, int a)
        => f.currentValue != a;
    public static bool operator <=(ModifiableFloat f, int a)
        => f.currentValue <= a;
    public static bool operator >=(ModifiableFloat f, int a)
        => f.currentValue >= a;

    public override bool Equals(object obj)
    {
        if (obj == null || obj is not ModifiableFloat)
            return false;
        return baseValue == (obj as ModifiableFloat).baseValue;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}