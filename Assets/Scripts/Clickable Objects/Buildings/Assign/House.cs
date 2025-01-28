using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>Provides a place to sleep for <see cref="Human"/>s.</summary>
public class House : AssignBuilding
{
    /// <summary>
    /// <inheritdoc/> <br/>
    /// Also sends assigned Humans to the elevator.
    /// </summary>
    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (constructed)
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

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can house up to {limit} workers";
        return strings;
    }

    #region Assign
    /// <summary>
    /// Changes the <see cref="Human.home"/>.
    /// </summary>
    /// <param name="human">To modify.</param>
    /// <param name="add">Add or remove.</param>
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
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns><returns>Returns homeless <see cref="Human"/>s</returns></returns>
    public override List<Human> GetUnassigned()
    {
        List<Human> unassigned = SceneRefs.humans.GetHumen();
        unassigned.RemoveAll(q => Assigned.Contains(q));
        return unassigned;
    }
    #endregion
}
