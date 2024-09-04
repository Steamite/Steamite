using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AssignBuilding : Building
{
    [Header("Humans")]
    public List<Human> assigned = new();
    public int limit = 5;
    ///////////////////////////////////////////////////
    //---------------Saving & Loading----------------//
    ///////////////////////////////////////////////////
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new AssignBSave();
        (clickable as AssignBSave).assigned = assigned.Select(q => q.id).ToList();
        (clickable as AssignBSave).limit = limit;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        limit = (save as AssignBSave).limit;
        base.Load(save);
    }

    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings.Insert(0, $"Can assign up to: {limit} workers");
        return strings;
    }
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info = null;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            // if to be setup
            if (setUp)
            {
                info.cTransform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "FILL"; // worker table
                info.cTransform.GetChild(0).GetComponent<WorkerAssign>().FillStart(this);
                info.SetAssignButton(true, info.cTransform.GetChild(0).GetChild(2));
            }
            // update
        }
        return info;
    }
}
