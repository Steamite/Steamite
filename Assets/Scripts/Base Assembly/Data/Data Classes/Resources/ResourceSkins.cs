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
    [SerializeField] public List<ResourceSkin> resSkin = new();
    [SerializeField] public List<ResourceSkin> fluidSkin = new();

    /// <summary>
    /// Ensures each resource type has it's own <see cref="ResourceSkin"/>.
    /// </summary>
    private void OnValidate()
    {
        List<string> resNames = Enum.GetNames(typeof(ResourceType)).ToList();
        while (resSkin.Count > resNames.Count)
        {
            resSkin.RemoveAt(resSkin.Count - 1);
        }
        while (resSkin.Count < resNames.Count)
        {
            resSkin.Add(new());
        }
        for (int i = 0; i < resSkin.Count; i++)
        {
            resSkin[i].name = resNames[i];
        }

        resNames = Enum.GetNames(typeof(FluidType)).ToList();
        while (fluidSkin.Count > resNames.Count)
        {
            fluidSkin.RemoveAt(fluidSkin.Count - 1);
        }
        while (fluidSkin.Count < resNames.Count)
        {
            fluidSkin.Add(new());
        }
        for (int i = 0; i < fluidSkin.Count; i++)
        {
            fluidSkin[i].name = resNames[i];
        }
    }

    /// <summary>
    /// Gets Resource color by resourcetype.
    /// </summary>
    /// <param name="resourceType">Type of resource.</param>
    /// <returns>The skin for this resource.</returns>
    public Color GetResourceColor(Enum resourceType)
    {
        if(resourceType is ResourceType)
            return resSkin[(int)(object)resourceType].color;
        else
            return fluidSkin[(int)(object)resourceType].color;
    }
}
