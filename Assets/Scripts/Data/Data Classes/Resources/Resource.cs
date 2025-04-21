using System;
using System.Collections.Generic;

/// <summary>Game resources.</summary>
[Serializable]
public class Resource
{
    #region Variables
    /// <summary>Capacity in this resource (-1 means there's no limit)</summary>
    public int capacity = -1;
    /// <summary>Resource types here, coresponds to the <see cref="ammount"/>.</summary>
    public List<ResourceType> type = new();
    /// <summary>Resource ammounts here, coresponds to the <see cref="type"/>.</summary>
    public List<int> ammount = new();
    #endregion

    #region Constructors
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
    #endregion

    #region Overrides
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

    public int this[ResourceType _type]
    {
        get => ammount[type.IndexOf(_type)];
        set => ammount[type.IndexOf(_type)] = value;
    }
    #endregion

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

    /// <summary>Removes empty resources.</summary>
    public void RemoveEmpty()
    {
        for (int i = type.Count - 1; i > -1; i--)
        {
            if (ammount[i] == 0)
            {
                ammount.RemoveAt(i);
                type.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// for generic resources
    /// </summary>
    /// <param name="r"></param>
    /// <returns>data to display in the info window</returns>
    public string GetDisplayText()
    {
        string s = "";
        for (int i = 0; i < type.Count && i < ammount.Count; i++)
        {
            if (ammount[i] > 0)
                s += $"{Enum.GetName(typeof(ResourceType), type[i])}: {ammount[i]}\n";
        }
        if (s.Length > 0)
            return s.Remove(s.Length - 1);
        else
            return "Nothing";
    }

    public void Add(ResourceType _type, int _ammount)
    {
        type.Add(_type);
        ammount.Add(_ammount);
    }
}