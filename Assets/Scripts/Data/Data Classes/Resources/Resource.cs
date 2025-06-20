using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Game resources.</summary>
[Serializable]
public class Resource
{
    #region Variables
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

    public Resource()
    {

    }
    #endregion

    #region Overrides
    public override bool Equals(object _resource)
    {
        if (_resource == null || _resource is not Resource)
        {
            return false;
        }

        Resource cmpRes = _resource as Resource;
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
        get
        {
            int i = type.IndexOf(_type);
            if (i > -1)
                return ammount[i];
            else
                return -1;
        }
        set
        {
            int i = type.IndexOf(_type);
            if (i > -1)
                ammount[i] = value;
            else
                Debug.LogError("NO TYPE PRESENT, add type?");
        }
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

    public int Sum() => ammount.Sum();
}