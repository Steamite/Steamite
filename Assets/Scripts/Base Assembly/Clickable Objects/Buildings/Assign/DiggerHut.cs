

using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class DiggerHut : Building, IDiggerHut
{
    [CreateProperty] public AssignData AssignData { get => assignData; set => assignData = value; }
    [SerializeField] AssignData assignData;

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }

    protected override void StartDeconstruction()
    {
        ((IAssign)this).ClearHumans();
        base.StartDeconstruction();
    }
}