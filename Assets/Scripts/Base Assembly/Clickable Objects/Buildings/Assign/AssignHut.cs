using System.Collections.Generic;
using Unity.Properties;

public class AssignHut : Building, IAssign
{
    List<Human> assigned = new();
    ModifiableInteger assignedLimit = new(3);

    [CreateProperty] public List<Human> Assigned { get => assigned; set => assigned = value; }
    [CreateProperty] public ModifiableInteger AssignLimit { get => assignedLimit; set => assignedLimit = value; }

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
}