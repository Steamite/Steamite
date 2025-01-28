using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Adds ability to assign <see cref="Human"/>s.</summary>
public class AssignBuilding : Building
{
    #region Variables
    /// <summary>List of assigned <see cref="Human"/>s.</summary>
    [Header("Humans")] protected List<Human> assigned = new();
    public int limit = 5;
    #endregion

    #region Properties
    [CreateProperty] public List<Human> Assigned => assigned;
    public bool ContainsWorker(Human human) => assigned.Contains(human);
    #endregion

    #region Window
    /// <summary>
    /// <inheritdoc/> <br/>
    /// Adds Assign list to <paramref name="toEnable"/>.
    /// </summary>
    /// <inheritdoc/>
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Assign");
        base.OpenWindowWithToggle(info, toEnable);
        ((IUIElement)info.constructedElement.Q<VisualElement>("Assign")).Fill(this);
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new AssignBSave();
        (clickable as AssignBSave).assigned = assigned.Select(q => q.id).ToList();
        (clickable as AssignBSave).limit = limit;
        return base.Save(clickable);
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        limit = (save as AssignBSave).limit;
        base.Load(save);
    }
    #endregion

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings.Insert(0, $"Can assign up to: {limit} workers");
        return strings;
    }

    #region Assign
    public virtual void ManageAssigned(Human human, bool add) => throw new NotImplementedException();

    /// <summary>
    /// Returns humans that are not assigned in the buildings.
    /// </summary>
    /// <returns><see cref="NotImplementedException"/> </returns>
    public virtual List<Human> GetUnassigned() => throw new NotImplementedException();
    #endregion
}
