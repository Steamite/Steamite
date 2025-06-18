using System.Collections.Generic;
using UnityEngine;

/// <summary>Holds prefabs for Factory instantialization.</summary>
[CreateAssetMenu(fileName = "Resource holder", menuName = "ScriptableObjects/Resource Holder", order = 1)]
public class ResourceHolder : ScriptableObject
{
    /// <summary>Registered prefabs</summary>
    [SerializeField] List<ClickableObject> prefabs = new();

    /// <summary>
    /// Finds a prefab by name.
    /// </summary>
    /// <param name="prefName">Name of the prefab.</param>
    /// <returns>Matching prefab.</returns>
    public T GetPrefab<T>(string prefName) where T: ClickableObject
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i].objectName.ToUpper() == prefName.ToUpper())
                return prefabs[i] as T;
        }
        Debug.LogError("Could not find Prefab! " + prefName);
        return null;
    }

    /// <summary>
    /// Gets prefab at <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of the prefab.</param>
    /// <returns>Prefab at input index.</returns>
    public ClickableObject GetPrefab(int index)
    {
        if (index < prefabs.Count)
            return prefabs[index];
        return null;
    }
}