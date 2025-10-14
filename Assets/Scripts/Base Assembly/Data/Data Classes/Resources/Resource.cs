using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>Game resources.</summary>
[Serializable, JsonObject]
public class Resource
{
    #region Variables
    /// <summary>Resource types here, coresponds to the <see cref="ammounts"/>.</summary>
    [JsonProperty, JsonRequired] public List<ResourceType> types;
    /// <summary>Resource ammounts here, coresponds to the <see cref="types"/>.</summary>
    [JsonProperty, JsonRequired] public List<int> ammounts;
    #endregion

    #region Constructors
    public Resource(List<ResourceType> _type, List<int> _ammount)
    {
        types = _type;
        ammounts = _ammount;
    }

    public Resource(Resource ammount)
    {
        types = ammount.types.ToList();
        ammounts = ammount.ammounts.ToList();
    }
    public Resource(ResourceSave save)
    {
        types = save.types.Select(q => ResFluidTypes.GetResByIndex(q)).ToList();
        ammounts = save.ammounts.ToList();
    }
    public Resource()
    {
        types = new();
        ammounts = new();
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
        if (cmpRes.types.Count != types.Count)
            return false;
        for (int i = 0; i < types.Count; i++)
        {
            int x = cmpRes.types.IndexOf(types[i]);
            if (x == -1)
                return false;
            if (ammounts[i] != cmpRes.ammounts[x])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the types and ammounts in two resources are the same, using insed of Equals because of types.
    /// </summary>
    /// <param name="resAmmount">Resource to Compare.</param>
    /// <returns>If the types and ammounts are identical.</returns>
    public bool Same(Resource resAmmount)
    {
        if(types.Count == resAmmount.types.Count)
        {
            for (int i = 0; i < types.Count; i++)
            {
                int x = resAmmount.types.IndexOf(types[i]);
                if (x == -1)
                    return false;
                if (ammounts[i] != resAmmount.ammounts[x])
                    return false;
            }
            return true;
        }
        return false;
    }

    public override int GetHashCode() { return base.GetHashCode(); }
    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < types.Count && i < ammounts.Count; i++)
        {
            if (ammounts[i] > 0)
                s += $"{types[i].Name}: {ammounts[i]}\n";
        }
        if (s.Length > 0)
            return s.Remove(s.Length - 1);
        else
            return "Nothing";
    }
    public int this[ResourceType _type]
    {
        get
        {
            int i = types.IndexOf(_type);
            if (i > -1)
                return ammounts[i];
            else
                return -1;
        }
        set
        {
            int i = types.IndexOf(_type);
            if (i > -1)
                ammounts[i] = value;
            else
                Debug.LogError("NO TYPE PRESENT, add type?");
        }
    }
    #endregion

    protected virtual void Add(ResourceType type, int ammount)
    {
        int i = types.IndexOf(type);
        if (i == -1)
        {
            types.Add(type);
            ammounts.Add(ammount);
        }
        else
            ammounts[i] += ammount;
    }

    protected virtual int Remove(ResourceType type, int change)
    {
        int i = types.IndexOf(type);
        if (i == -1)
        {
            Debug.LogWarning($"{type} not present in this res: {this}");
            return 0;
        }
        else
            ammounts[i] -= change;
        return change;
    }

    public void Manage(Resource resAmmount, bool add, float mod = 1, bool removeEmpty = false)
    {
        for (int i = resAmmount.types.Count - 1; i > -1; i--)
            ManageSimple(resAmmount.types[i], resAmmount.ammounts[i], add, mod, removeEmpty);
    }

    public void ManageSimple(ResourceType type, float ammount, bool add, float mod = 1, bool removeEmpty = false)
    {
        if (add)
            Add(type, Mathf.RoundToInt(ammount * mod));
        else
        {
            Remove(type, Mathf.RoundToInt(ammount * mod));
            if (removeEmpty)
            {
                int i = types.IndexOf(type);
                if (ammounts[i] <= 0)
                {
                    if (ammount < 0)
                        Debug.LogError("removed too much");
                    types.RemoveAt(i);
                    ammounts.RemoveAt(i);
                }
            }
        }
    }

    public void TakeResource(Resource toTransfer)
    {
        Resource ret = new();
        Diff(ret, toTransfer);
        ret = toTransfer - ret;
        Manage(ret, false);
        toTransfer.Manage(ret, false, removeEmpty: true);
    }

    public Resource GetResOfAmmount(Resource ret, int _ammount, bool remove)
    {
        for (int i = types.Count -1; i > -1; i--)
        {
            int x = (ammounts[i] < _ammount) 
                ? ammounts[i] 
                : _ammount;
            ret.ManageSimple(types[i], x, true);
            if(remove)
                ManageSimple(types[i], x, false, removeEmpty: true);
            _ammount -= x;
            if(_ammount <= 0)
                break;
        }
        return ret;
    }

    protected void BaseDiff(Resource ret, Resource cost)
    {
        for (int i = 0; i < cost.types.Count; i++)
        {
            int j = types.IndexOf(cost.types[i]);
            if (j > -1)
            {
                int x = cost.ammounts[i] - ammounts[j];
                if (x > 0)
                {
                    ret.types.Add(cost.types[i]);
                    ret.ammounts.Add(x);
                }
            }
            else
            {
                ret.types.Add(cost.types[i]);
                ret.ammounts.Add(cost.ammounts[i]);
            }
        }
    }

    public Resource Diff(Resource cost) 
    {
        Resource diff = new();
        BaseDiff(diff, cost);
        return diff;
    }

    public Resource Diff(Resource bonusInventory, Resource cost)
    {
        Resource resource = new(this);
        resource.Manage(bonusInventory, true);
        Resource diff = resource.Diff(cost);
        return diff;
    }

    public void RemoveEmpty()
    {
        for (int i = types.Count - 1; i > -1; i--)
        {
            if (ammounts[i] == 0)
            {
                types.RemoveAt(i);
                ammounts.RemoveAt(i);
            }
        }
    }


    public int Sum() => ammounts.Sum();

    public static Resource operator -(Resource a, Resource b)
    {
        Resource res = new(a);
        res.Manage(b, false, removeEmpty: true);
        return res;
    }
}