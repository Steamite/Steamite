using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource holder", menuName = "ScriptableObjects/Resource Holder", order = 1)]
public class ResourceHolder : ScriptableObject
{
    [SerializeField] List<ClickableObject> prefabs = new();

    public List<ClickableObject> GetPrefabs()
    {
        return prefabs;
    }
    public ClickableObject GetPrefab(string prefName)
    {
        for(int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i].name.ToUpper() == prefName.ToUpper())
                return prefabs[i];
        }
        Debug.LogError("Could not find Prefab!" + prefName);
        return null;
    }
    public ClickableObject GetPrefab(int index)
    {
        if (index < prefabs.Count)
            return prefabs[index];
        return null;
    }
}
