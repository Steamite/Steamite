using BuildingStats;
using System;
using UnityEngine;

[Serializable]
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

    public void AddMod(StatValue stat)
    {
        if (stat.percent)
            Modifier.percentMod += stat.modAmmount * 0.01f;
        else
            Modifier.absoluteMod += Mathf.RoundToInt(stat.modAmmount);
        RecalculateMod();
    }
}