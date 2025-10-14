using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ResourceType", menuName = "Resources/ResourceType")]
public class ResourceType : ScriptableObject
{
    public string Name;
    public Color color;
    public Texture2D image;
    public ResourceCategory category;
}


public static class ResFluidTypes
{
    const string RES_PATH = "Assets/Game Data/Resource Types/Resource/Resources.asset";
    const string FLUID_PATH = "Assets/Game Data/Resource Types/Fluid/Fluids.asset";
    const string MONEY_PATH = "Assets/Game Data/Resource Types/Money.asset";

    static ResourceTypeHolder resources;
    static ResourceTypeHolder fluids;

    static Dictionary<ResourceCategory, List<ResourceType>> resByCategory;

    public static ResourceType Money { get; private set; }

    public static ResourceType None => resources.types[0];

#if UNITY_EDITOR
    [MenuItem("Custom Editors/Refresh Resource _&r", priority = 0)]
#endif
    public static async Task Init()
    {
        resources = await Addressables.LoadAssetAsync<ResourceTypeHolder>(RES_PATH).Task;
        fluids = await Addressables.LoadAssetAsync<ResourceTypeHolder>(FLUID_PATH).Task;
        Money = await Addressables.LoadAssetAsync<ResourceType>(MONEY_PATH).Task;
        var test = await Addressables.LoadAssetsAsync<ResourceType>("resources").Task;
        resByCategory = new();
        foreach (var res in resources.types.Union(fluids.types))
        {
            if (res.category == null)
                return;
            if (!resByCategory.ContainsKey(res.category))
                resByCategory.Add(res.category, new() { res });

            resByCategory[res.category].Add(res);
        }
        Debug.Log("Resource reloaded");
    }

    public static ResourceType GetResByName(string name)
        => resources.types.First(q => q.Name == name);
    public static ResourceType GetResByIndex(int i)
        => resources.types[i];

    public static ResourceType GetFluidByIndex(int index)
        => fluids.types[index];


    public static List<string> GetResNamesList()
        => resources.types.Select(q => q.Name).ToList();
    public static List<ResourceType> GetResList()
        => resources.types.Skip(1).ToList();

    public static int GetResourceIndex(ResourceType resourceType)
        => resources.types.IndexOf(resourceType);

    public static List<string> GetFluidNames()
        => fluids.types.Select(q => q.Name).ToList();
}