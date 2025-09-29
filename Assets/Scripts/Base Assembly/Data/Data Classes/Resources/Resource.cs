using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable, JsonObject]
public class Resource : ResAmmount<ResourceType>
{
    #region Constructors
    public Resource() : base() { }
    public Resource(ResAmmount<ResourceType> resAmmount) : base(resAmmount) { }
    public Resource(List<ResourceType> types, List<int> ammounts) : base(types, ammounts) { }
    #endregion

    /// <summary>Removes empty resources.</summary>
    public void RemoveEmpty()
    {
        for (int i = types.Count - 1; i > -1; i--)
        {
            if (ammounts[i] == 0)
            {
                ammounts.RemoveAt(i);
                types.RemoveAt(i);
            }
        }
    }


    public Resource Diff(Resource resourceCost)
    {
        Resource resource = new Resource();
        base.Diff(resource, resourceCost);
        return resource;
    }

    public Resource Diff(Resource bonusInventory, Resource cost)
    {
        Resource resource = new(this);
        resource.Manage(bonusInventory, true);
        Resource a = resource.Diff(cost);
        return a;
    }

    public override bool Equals(object _resource)
    {
        if (_resource is not Resource)
            return false;
        return base.Equals(_resource);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), types, ammounts);
    }
}