using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class House : AssignBuilding
{
    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    #region Window

    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        OpenWindowWithToggle(info, toEnable);
    }

    #endregion
    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (build.constructed)
        {
            foreach (Human h in assigned)
            {
                h.home = null;
                if (h.nightTime)
                {
                    h.Night();
                }
            }
        }
    }

    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can house up to {limit} workers";
        return strings;
    }

    #region Assign
    public override void ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            human.home = this;
            assigned.Add(human);
        }
        else
        {
            human.home = null;
            assigned.Remove(human);
        }
        UIUpdate(nameof(Assigned));
    }
    public override List<Human> GetUnassigned()
    {
        List<Human> unassigned = SceneRefs.humans.GetHumen();
        unassigned.RemoveAll(q => Assigned.Contains(q));
        return unassigned;
    }
    #endregion
}
