using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

/// <summary>Building that doesn't produce resources but creates research.</summary>
public class ResearchProduction : Building, IProduction, IAssign
{
    [SerializeField] AssignData assignData;
    [CreateProperty] public AssignData AssignData { get => assignData; set => assignData = value; }

    public float ProdTime { get; set; }
    public float CurrentTime { get; set; }
    [SerializeField] ModifiableFloat modifier;
    [CreateProperty] public ModifiableFloat ProdSpeed { get => modifier; set => modifier = value; }
    public bool Stoped { get; set; }

    #region Window
    /// <summary>
    /// Adds "Research" to <paramref name="toEnable"/>. <br/>
    /// <inheritdoc cref="Building.ToggleInfoComponents(InfoWindow, List{string})"/>
    /// </summary>
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Research Info", "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    #endregion


    #region Production
    /// <summary>
    /// Triggers research event.
    /// </summary>
    /// <param name="speed"></param>
    public void ProgressProduction(float speed)
    {
        SceneRefs.ResearchAdapter.DoProduction(speed);
    }

    public void Product()
    {
        // Stop animation
    }

    #endregion
}
