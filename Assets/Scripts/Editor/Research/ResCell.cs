using System;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ResCell : ResourceCell<Resource, ResourceType>
{
    public ResCell() : base() { }

}
[UxmlElement]
public partial class FluidCell : ResourceCell<Fluid, FluidType>
{
    public FluidCell() : base() { }
}