using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Stores data about the resource visual.</summary>
[Serializable]
public class ResourceSkin
{
    public string name;
    public Color color;
    public Sprite icon;
}

/// <summary>Contains all <see cref="ResourceSkin"/>s.</summary>
[CreateAssetMenu(fileName = "Resource Skin", menuName = "ScriptableObjects/Resource Skin")]
public class ResourceSkins : ScriptableObject
{
    [SerializeField] public ResourceSkin moneySkin;
    /// <summary>List of all skins.</summary>
    [SerializeField] public List<ResourceSkin> skins = new();

    /// <summary>
    /// Ensures each resource type has it's own <see cref="ResourceSkin"/>.
    /// </summary>
    private void OnValidate()
    {
        List<string> resNames = Enum.GetNames(typeof(ResourceType)).Union(Enum.GetNames(typeof(FluidType))).ToList();

        while (skins.Count > resNames.Count)
        {
            skins.RemoveAt(skins.Count - 1);
        }
        while (skins.Count < resNames.Count)
        {
            skins.Add(new());
        }


        for (int i = 0; i < skins.Count; i++)
        {
            skins[i].name = resNames[i];
        }
    }

    /// <summary>
    /// Gets Resource color by resourcetype.
    /// </summary>
    /// <param name="resourceType">Type of resource.</param>
    /// <returns>The skin for this resource.</returns>
    public Color GetResourceColor(Enum resourceType)
    {
        return skins[(int)(ResourceType)resourceType].color;
    }
}
