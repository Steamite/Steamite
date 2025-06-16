using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
/// <summary>Provides a place to sleep for <see cref="Human"/>s.</summary>
public class House : Building, IAssign
{
    [CreateProperty] public List<Human> Assigned { get; set; } = new();
    public ModifiableInteger AssignLimit { get; set; }

    #region Deconstruction
    /// <summary>
    /// <inheritdoc/> <br/>
    /// Also sends assigned Humans to the elevator.
    /// </summary>
    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (constructed)
        {
            foreach (Human h in Assigned)
            {
                h.home = null;
                if (h.nightTime)
                {
                    h.Night();
                }
            }
        }
    }
    #endregion

    #region UI
    /// <summary>
    /// <inheritdoc/> <br/>
    /// Adds Assign list to <paramref name="toEnable"/>.
    /// </summary>
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Assign");
        base.ToggleInfoComponents(info, toEnable);
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can house up to {AssignLimit.currentValue} workers";
        return strings;
    }
    #endregion

    #region Assign
    /// <summary>
    /// <inheritdoc/> <br/>
    /// And it's <see cref="Human.home"/>.
    /// </summary>
    /// <param name="human"><inheritdoc/></param>
    /// <param name="add"><inheritdoc/></param>
    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (Assigned.Count == AssignLimit.currentValue)
                return false;
            Assigned.Add(human);
            human.home = this;
        }
        else
        {
            Assigned.Remove(human);
            human.home = null;
        }
        UIUpdate(nameof(Assigned));
        return true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><returns>Returns homeless <see cref="Human"/>s</returns></returns>
    public List<Human> GetUnassigned()
    {
        List<Human> unassigned = SceneRefs.humans.GetHumen();
        unassigned.RemoveAll(q => Assigned.Contains(q) || q.home != null);
        return unassigned;
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new AssignBSave();
        (clickable as AssignBSave).assigned = Assigned.Select(q => q.id).ToList();
        return base.Save(clickable);
    }

    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
    }
    #endregion;
}
