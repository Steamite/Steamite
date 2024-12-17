using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class AssignBuilding : Building
{

    [Header("Humans")]
    protected List<Human> assigned = new();

    [CreateProperty]
    public List<Human> Assigned => assigned;
    public bool ContainsWorker(Human human) => assigned.Contains(human);

    public int limit = 5;


    #region Window

    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Assign");
        base.OpenWindowWithToggle(info, toEnable);
        ((IUIElement)info.constructedElement.Q<VisualElement>("Assign")).Fill(this);
    }
    #endregion

    #region Saving
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
    #endregion

    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings.Insert(0, $"Can assign up to: {limit} workers");
        return strings;
    }

    #region Assign
    public virtual void ManageAssigned(Human human, bool add)
    {
        throw new NotImplementedException();
    }

    public virtual List<Human> GetUnassigned()
    {
        throw new NotImplementedException();
    }
    #endregion
}
