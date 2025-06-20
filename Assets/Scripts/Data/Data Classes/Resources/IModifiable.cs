using BuildingStats;
using UnityEngine;

public class ModValue
{
    public float percentMod;
    public int absoluteMod;

    public ModValue()
    {
        percentMod = 1;
        absoluteMod = 0;
    }
}

public interface IModifiable
{
    ModValue Modifier { get; set; }


    public void Init()
    {
        Modifier = new();
        RecalculateMod();
    }
    public void RecalculateMod();

    public void AddMod(StatPair pair)
    {
        if (pair.percent)
            Modifier.percentMod += pair.modAmmount * 0.01f;
        else
            Modifier.absoluteMod += Mathf.RoundToInt(pair.modAmmount);
        RecalculateMod();
    }
}