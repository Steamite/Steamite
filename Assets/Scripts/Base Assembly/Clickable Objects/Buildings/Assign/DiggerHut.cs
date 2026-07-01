using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class DiggerHut : Building, IDiggerHut
{
    List<Human> assigned = new();
    [SerializeField] ModifiableInteger assignLimit = new(3);


    [CreateProperty] public List<Human> Assigned { get => assigned; set => assigned = value; }
    [CreateProperty] public ModifiableInteger AssignLimit { get => assignLimit; set => assignLimit = value; }

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    public override void OrderDeconstruct()
    {
        ((IAssign)this).ClearHumans();
        base.OrderDeconstruct();
    }
}