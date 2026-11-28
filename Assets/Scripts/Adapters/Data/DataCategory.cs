using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public abstract class DataCategory<T> where T : DataObject
{
    [SerializeField] public int id;
    [SerializeField] public string Name;
    [SerializeReference] public List<T> Objects;
    [SerializeField] public VectorImage Icon;


#if UNITY_EDITOR
    public T GetObjectByID(int objectId)
        => Objects.FirstOrDefault(q => q.id == objectId);
    public string GetObjectChoiceByID(int objectId)
    {
        if (objectId == -1)
            return "None";

        T obj = Objects.FirstOrDefault(q => q.id == objectId);
        if (obj == null)
            return "Missing!";

        return obj.Name;
    }

    [NonSerialized] public List<T> availableObjects;

    public Dictionary<int, string> GetObjectCatalog(bool onlyAvailable)
    {
        Dictionary<int, string> valuePairs = new();
        ICollection<T> collection = onlyAvailable ? availableObjects : Objects;
        foreach (var item in collection)
        {
            valuePairs.Add(item.id, item.Name);
        }
        return valuePairs;
    }

    public List<string> ObjectChoices(bool onlyAvailable = false)
    {
        List<string> objectNames = new();
        ICollection<T> collection = onlyAvailable ? availableObjects : Objects;
        
        objectNames.Add("None");
        objectNames.AddRange(collection.Select(q => q.Name).ToList());

        return objectNames;
    }

    public int GetObjectIDByName(string newValue)
        => Objects.FirstOrDefault(q => q.Name == newValue).id;
#endif
}