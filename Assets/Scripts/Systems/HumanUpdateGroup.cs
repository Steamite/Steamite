using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup)), UpdateBefore(typeof(TransformSystemGroup))]
public partial class HumanUpdateGroup : ComponentSystemGroup
{
}
