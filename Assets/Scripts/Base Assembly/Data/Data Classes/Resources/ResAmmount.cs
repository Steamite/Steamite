using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>Game resources.</summary>
[Serializable]
public class ResAmmount<T> : ResAmmountBase where T : System.Enum
{
    #region Variables
    /// <summary>Resource types here, coresponds to the <see cref="ammounts"/>.</summary>
    public List<T> types;
    /// <summary>Resource ammounts here, coresponds to the <see cref="types"/>.</summary>
    public List<int> ammounts;
    #endregion

    #region Constructors
    public ResAmmount(List<T> _type, List<int> _ammount)
    {
        types = _type;
        ammounts = _ammount;
    }

    public ResAmmount(ResAmmount<T> ammount)
    {
        types = ammount.types.ToList();
        ammounts = ammount.ammounts.ToList();
    }

    public ResAmmount()
    {
        types = new();
        ammounts = new();
    }
    #endregion

    #region Overrides
    public override bool Equals(object _resource)
    {
        if (_resource == null || _resource is not ResAmmount<T>)
        {
            return false;
        }

        ResAmmount<T> cmpRes = _resource as ResAmmount<T>;
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
    public override int GetHashCode() { return base.GetHashCode(); }
    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < types.Count && i < ammounts.Count; i++)
        {
            if (ammounts[i] > 0)
                s += $"{Enum.GetName(typeof(T), types[i])}: {ammounts[i]}\n";
        }
        if (s.Length > 0)
            return s.Remove(s.Length - 1);
        else
            return "Nothing";
    }
    public int this[T _type]
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

    protected virtual void Add(T type, int ammount)
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

    protected virtual int Remove(T type, int change)
    {
        int i = types.IndexOf(type);
        if (i == -1)
        {
            Debug.LogError($"{type} not present in this fluid: {this}, adding");
        }
        else
            ammounts[i] -= change;
        return change;
    }

    public void Manage(ResAmmount<T> resAmmount, bool add, int mod = 1, bool removeEmpty = false)
    {
        for (int i = resAmmount.types.Count - 1; i > -1; i--)
            ManageSimple(resAmmount.types[i], resAmmount.ammounts[i], add, mod, removeEmpty);
    }

    public void ManageSimple(T type, int ammount, bool add, int mod = 1, bool removeEmpty = false)
    {
        if (add)
            Add(type, ammount * mod);
        else
        {
            Remove(type, ammount * mod);
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

    public void TakeResource(ResAmmount<T> toTransfer)
    {
        ResAmmount<T> ret = new();
        Diff(ret, toTransfer);
        ret = toTransfer - ret;
        Manage(ret, false);
        toTransfer.Manage(ret, false, removeEmpty: true);
    }

    public ResAmmount<T> GetResOfAmmount(ResAmmount<T> ret, int _ammount, bool remove)
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

    protected void Diff(ResAmmount<T> ret, ResAmmount<T> cost)
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

    public int Sum() => ammounts.Sum();

    public static ResAmmount<T> operator -(ResAmmount<T> a, ResAmmount<T> b)
    {
        ResAmmount<T> res = new(a);
        res.Manage(b, false, removeEmpty: true);
        return res;
    }
}

[Serializable]
public abstract class ResAmmountBase 
{
}