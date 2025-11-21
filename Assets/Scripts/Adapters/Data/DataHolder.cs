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

    public DataObject(DataObject dataObject)
    {
        Name = dataObject.Name;
        id = dataObject.id;
    }
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
    public const string EDITOR_PATH = "Assets/Game Data/UI/QuestData.asset";

    public List<string> CategoryChoices()
        => Categories.Select(q => q.Name).ToList();
    public List<string> ObjectChoices(bool addNone = false)
    {
        List<string> objectNames = new();
        if (addNone)
            objectNames.Add("None");
        objectNames.AddRange(Categories.SelectMany(q => q.Objects).Select(q => q.Name).ToList());
        return objectNames;
    }
    public int UniqueID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(1, int.MaxValue);
        } while (Categories.SelectMany(q => q.Objects).Count(q => q.id == i) > 0);
        return i;
    }
    public int UniqueCategID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(1, int.MaxValue);
        } while (Categories.Select(q => q.id).Count(q => q == i) > 0);
        return i;
    }
#endif
    #endregion

    [SerializeField] public List<CATEG_T> Categories = new();

    public WRAPPER_T GetObjectBySaveIndex(DataAssign dataAssign)
    {
        return GetCategByID(dataAssign.categoryId)?.Objects.FirstOrDefault(q => q.id == dataAssign.objectId);
    }

    public DataAssign GetSaveIndexByName(string _name)
    {
        WRAPPER_T wrapper;
        for (int i = 0; i < Categories.Count; i++)
        {
            wrapper = Categories[i].Objects.FirstOrDefault(q => q.Name == _name);
            if (wrapper != null)
                return new(Categories[i].id, wrapper.id);
        }
        return new(-1, -1);
    }

    public CATEG_T GetCategByID(int id)
    {
        return Categories.FirstOrDefault(q => q.id == id);
    }

    public int GetCategIndexById(int categoryId, bool addSelectionOffset = false)
    {
        return Categories.FindIndex(q => q.id == categoryId) + (addSelectionOffset ? 1 : 0);
    }


    public int GetCategIdFromName(string newValue)
    {
        CATEG_T cat = Categories.FirstOrDefault(q => q.Name == newValue);
        return cat != null ? cat.id : -1;
    }
}