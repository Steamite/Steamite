using System.Collections.Generic;
using UnityEngine;

/// <summary>Building that doesn't produce resources but creates research.</summary>
public class Research_Production : ProductionBuilding
{
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
    public override void ProgressProduction(float speed)
    {
        SceneRefs.researchAdapter.DoProduction(speed);
    }

    /// <summary>Does nothing.</summary>
    protected override void AfterProduction(){}
    /// <summary>Does nothing.</summary>
    protected override void Product(){}
    /// <summary>Does nothing.</summary>
    public override void RefreshStatus(){}
    #endregion
}
