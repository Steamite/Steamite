
using System.Collections.Generic;
using System;

[Serializable]
public class Resource
{
    // (https://steamite.atlassian.net/wiki/x/AYCl)
    public int capacity = -1; // -1 = no limit
    public List<ResourceType> type = new(); // stores all resource types
    public List<int> ammount = new();
    public Resource()
    {

    }
    /// <returns>a new identical resource</returns>
    public Resource Clone()
    {
        Resource clone = new();
        for (int i = 0; i < type.Count; i++)
        {
            clone.type.Add(type[i]);
            if (ammount.Count <= i)
                clone.ammount.Add(0);
            else
                clone.ammount.Add(ammount[i]);
        }
        return clone;
    }
    // override object.Equals
    public override bool Equals(object resource)
    {
        if (resource == null || GetType() != resource.GetType())
        {
            return false;
        }

        Resource cmpRes = (Resource)resource;
        if (cmpRes.type.Count != type.Count)
            return false;
        for (int i = 0; i < type.Count; i++)
        {
            int x = cmpRes.type.IndexOf(type[i]);
            if (x == -1)
                return false;
            if (ammount[i] != cmpRes.ammount[x])
                return false;
        }
        return true;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}