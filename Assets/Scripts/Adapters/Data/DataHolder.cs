using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DataObject
{
    [SerializeField] public string Name;
    [SerializeField] public string description;
    [SerializeField] public int id;

    public DataObject(DataObject dataObject)
    {
        Name = dataObject.Name;
        id = dataObject.id;
        description = dataObject.description;
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
    [SerializeField] public List<CATEG_T> Categories = new();



    public WRAPPER_T GetObjectBySaveIndex(DataAssign dataAssign)
    {
        WRAPPER_T result = GetCategByID(dataAssign.categoryId)?.Objects.FirstOrDefault(q => q.id == dataAssign.objectId);
        if (result == null)
            Debug.LogError($"cat: {dataAssign.categoryId}, id: {dataAssign.objectId}");
        return result;
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




    #region Editor

#if UNITY_EDITOR
    IEnumerable<WRAPPER_T> AllObjects => Categories.SelectMany(q => q.Objects);


    public const string EDITOR_PATH = "Assets/Game Data/UI/QuestData.asset";

    public List<string> CategoryChoices(bool addNone = false)
    {
        List<string> objectNames = new();
        if (addNone)
            objectNames.Add("None");
        objectNames.AddRange(Categories.Select(q => q.Name).ToList());
        return objectNames;
    }

    public string GetCategoryChoiceByID(int categoryId)
    {
        if (categoryId == -1)
            return "None";

        CATEG_T categ = Categories.FirstOrDefault(q => q.id == categoryId);
        if (categ == null)
            return "Missing!";

        return categ.Name;
    }


    /*
        public Dictionary<int, string> GetCategoryCatalog()
        {
            Dictionary<int, string> valuePairs = new();
            foreach (var item in Categories)
            {
                valuePairs.Add(item.id, item.Name);
            }
            return valuePairs;
        }

        public Dictionary<int, string> GetObjectCatalog(int categoryId, bool onlyAvailable = false)
            => Categories.FirstOrDefault(q => q.id == categoryId).GetObjectCatalog(onlyAvailable);*/



    public int UniqueID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(1, 0xFFFF);
        } while (AllObjects.Count(q => q.id == i) > 0);
        return i;
    }

    public int UniqueCategID()
    {
        int i;
        do
        {
            i = UnityEngine.Random.Range(1, 0xFFFF);
        } while (Categories.Select(q => q.id).Count(q => q == i) > 0);
        return i;
    }

    public int GetCategIdFromName(string newValue)
    {
        CATEG_T cat = Categories.FirstOrDefault(q => q.Name == newValue);
        return cat != null ? cat.id : 0;
    }

#endif
    #endregion


}