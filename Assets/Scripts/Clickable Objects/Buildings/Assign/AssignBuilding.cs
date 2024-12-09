using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class AssignBuilding : Building
{

    [Header("Humans")]
    public List<Human> assigned = new();
    public int limit = 5;

    #region Window
    protected override void SetupWindow(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Assign");

        base.SetupWindow(info, toEnable);
        info.constructed.Q<WorkerAssign>("Worker-Assign").FillStart(this);
    }
    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        info.constructed.Q<Label>("Assigned-Count").text = $"Assigned: {assigned.Count}/{limit}";
    }
    #endregion
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
}
