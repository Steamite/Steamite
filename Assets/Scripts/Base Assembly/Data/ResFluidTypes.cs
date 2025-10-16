using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.Analytics.IAnalytic;

public static class ResFluidTypes
{
    const string RESOURCE_PATH = "Assets/Game Data/ResourceData.asset";

    public static ResourceType Money;

    public static ResourceType None;

    static List<ResourceType> resources;
    static List<ResourceType> Resources
    {
        get
        {
            if(resources == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(RESOURCE_PATH));
            return resources;
        }
    }

    static List<ResourceType> fluids;
    static List<ResourceType> Fluids
    {
        get
        {
            if (fluids == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(RESOURCE_PATH));
            return fluids;
        }
    }

    static List<ResourceTypeCategory> fullRes;
    static List<ResourceTypeCategory> FullRes
    {
        get
        {
            if (fullRes == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(RESOURCE_PATH));
            return fullRes;
        }
    }
#if UNITY_EDITOR
    [MenuItem("Custom Editors/Refresh Resource _&r", priority = 0)]
    [InitializeOnLoadMethod]
#endif
    public static async Task Init()
    {
        ResourceData data = await Addressables.LoadAssetAsync<ResourceData>(RESOURCE_PATH).Task;
        InitFill(data);
    }

    static void InitFill(ResourceData data)
    {
        fullRes = data.Categories;
        resources = fullRes.Where(q => q.Name != "Fluids").SelectMany(q => q.Objects).Select(q => q.data).ToList();

        fluids = fullRes.Where(q => q.Name == "Fluids").SelectMany(q => q.Objects).Select(q => q.data).ToList();
        None = resources[0];
        Money = resources[1];
    }


    public static ResourceType GetResByName(string name)
        => Resources.First(q => q.Name == name);
    public static ResourceType GetResByIndex(int i)
        => Resources[i];

    public static ResourceType GetFluidByIndex(int index)
        => Fluids[index];

    public static List<string> GetResNamesList()
        => Resources.Select(q => q.Name).ToList();
    public static List<string> GetResNamesList(List<int> allowedCategories)
    {
        if (allowedCategories == null || allowedCategories.Count == 0)
            return GetResNamesList();
        List<string> names = new();
#if UNITY_EDITOR
        names.Add(None.Name);
#endif
        foreach (int i in allowedCategories)
            names.AddRange(FullRes[i].Objects.Select(q => q.Name));
        return names;
    }
    public static List<ResourceType> GetResList()
        => Resources.Skip(2).ToList();

    public static int GetResourceIndex(ResourceType resourceType)
        => Resources.IndexOf(resourceType);

    public static List<string> GetFluidNames()
        => Fluids.Select(q => q.Name).ToList();
}