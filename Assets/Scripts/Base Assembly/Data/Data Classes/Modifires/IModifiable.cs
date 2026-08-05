

using BuildingStats;
using System;
using UnityEngine;

[Serializable]
public class ModValue
{
    public float percentMod { get; private set; }
    public int absoluteMod { get; private set; }

  /*  ModValue(int percent, int absolute)
    {
        percentMod = percent;
        absoluteMod = absolute;
    }*/

    public ModValue()
    {
        percentMod = 1;
        absoluteMod = 0;
    }
    public void Init()
    {
        percentMod = 1;
        absoluteMod = 0;
    }

    public void ModifyPercent(int ammount)
        => percentMod += ammount;

    public void ModifyAbsolute(int ammount)
        => absoluteMod += ammount;
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

    public void AddMod(StatValue stat)
    {
        if (stat.percent)
            Modifier.ModifyPercent(stat.modAmmount);
        else
            Modifier.ModifyAbsolute(Mathf.RoundToInt(stat.modAmmount));
        RecalculateMod();
    }
}