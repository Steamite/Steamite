using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DataObject
{
    [SerializeField] public string Name;
    [SerializeField] public int id;

    public virtual string GetName() => Name;

    public DataObject(int _id)
    {
        id = _id;
        Name = "";
    }

    public DataObject() { }
}

public abstract class DataHolder<CATEG_T, WRAPPER_T> : ScriptableObject where CATEG_T : DataCategory<WRAPPER_T> where WRAPPER_T : DataObject
{
    public const string PATH = "";

    #region Editor

#if UNITY_EDITOR
    public List<string> CategoryChoices()
        => Categories.Select(q => q.Name).ToList();
    public List<string> ObjectChoices(bool addNone = false)
    {
        List<string> objectNames = new();
        if (addNone)
            objectNames.Add("None");
        objectNames.AddRange(Categories.SelectMany(q => q.Objects).Select(q=> q.Name).ToList());
        return objectNames;
    }
    public int UniqueID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(0, int.MaxValue);
        } while (Categories.SelectMany(q => q.Objects).Count(q => q.id == i) > 0);
        return i;
    }
#endif
    #endregion

    [SerializeField] public List<CATEG_T> Categories = new();

    public WRAPPER_T GetObjectBySaveIndex(DataAssign dataAssign)
    {
        if (dataAssign.categoryIndex == -1)
            return null;
        return Categories[dataAssign.categoryIndex].Objects.FirstOrDefault(q => q.id == dataAssign.objectId);
    }

    public DataAssign GetSaveIndexByName(string _name)
    {
        WRAPPER_T wrapper;
        for (int i = 0; i < Categories.Count; i++)
        {
            wrapper = Categories[i].Objects.FirstOrDefault(q => q.Name == _name);
            if (wrapper != null)
                return new(i, wrapper.id);
        }
        return new(-1, -1);
    }
}