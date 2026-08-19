using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StabilityBuilding : Building, IStabilitySupport
{
    [SerializeField] int supportValue;
    public int SupportValue 
    { 
        get => supportValue; 
        set => supportValue = value; 
    }

    protected override void FinishBuild()
    {
        base.FinishBuild();
        ApplySupport();
    }

    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);

        if (IsWorking)
            ApplySupport();
    }

    void ApplySupport()
    {
        GridPos pos = GetPos();
        MyGrid.GetGroundLevelData(pos.y).ChangeStability(
            (int)pos.x,
            (int)pos.z,
            supportValue,
            true);
    }
}
