using System.Collections.Generic;
using UnityEngine;

/// <summary>Building that doesn't produce resources but creates research.</summary>
public class Research_Production : Building, IProduction, IAssign
{
    public List<Human> Assigned { get; set; }
    public int AssignLimit { get; set; }
    public float ProdTime { get; set; }
    public float CurrentTime { get; set; }
    public int Modifier { get; set; }
    public bool Stoped { get; set; }

    #region Window
    /// <summary>
    /// Adds "Research" to <paramref name="toEnable"/>. <br/>
    /// <inheritdoc cref="Building.OpenWindowWithToggle(InfoWindow, List{string})"/>
    /// </summary>
    /// <inheritdoc/>
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Research");
        base.OpenWindowWithToggle(info, toEnable);
    }
    #endregion


    #region Production
    /// <summary>
    /// Triggers research event.
    /// </summary>
    /// <param name="speed"></param>
    public void ProgressProduction(float speed)
    {
        SceneRefs.researchAdapter.DoProduction(speed);
    }

    public void ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            human.workplace = this;
            Assigned.Add(human);
        }
    }

    public List<Human> GetUnassigned()
    {
        return SceneRefs.humans.GetPartTime();
    }

    public void Product()
    {
        // Stop animation
    }

    #endregion
}
