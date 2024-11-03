
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

[Serializable]
public class Resource
{
    public int capacity = -1; // -1 = no limit
    public List<ResourceType> type = new(); // stores all resource types
    public List<int> ammount = new();

    public Resource(List<ResourceType> _type, List<int> _ammount)
    {
        type = _type;
        ammount = _ammount;
    }
    public Resource(int _capacity)
    {
        capacity = _capacity;
    }
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

    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < type.Count; i++)
        {
            if (ammount[i] > 0)
                s += $"{type[i]}: {ammount[i]}\n";
        }
        return s;
    }
    public string ToStringComplete()
    {
        string s = "";
        for (int i = 0; i < type.Count; i++)
        {
            s += $"{type[i]}: {ammount[i]}\n";
        }
        return s;
    }

    /// <summary>
    /// Creates a string ready to be displayed.
    /// </summary>
    /// <param name="str">String to add to.</param>
    /// <param name="available">Storage resources.</param>
    /// <returns>True if you cannot afford it.</returns>
    public bool ToStringTMP(ref string str, Resource available)
    {
        bool canAfford = true;
        Resource diff = MyRes.DiffRes(this, available);
        for (int i = 0; i < type.Count; i++)
        {
            if (diff.type.Contains(type[i]))
            {
                str += $"<color=red>{type[i]}: {available.ammount[available.type.IndexOf(type[i])]}/{ammount[i]}</color>\n";
                canAfford = false;
            }
            else
                str += $"{type[i]}: {available.ammount[available.type.IndexOf(type[i])]}/{ammount[i]}\n";
        }
        return !canAfford;
    }
}