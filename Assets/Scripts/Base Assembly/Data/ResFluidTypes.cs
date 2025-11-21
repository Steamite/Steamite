using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.AddressableAssets;

public static class ResFluidTypes
{
    static ResourceData data;
    public static ResourceType Money;

    static ResourceType none;
    public static ResourceType None
    {
        get
        {
#if UNITY_EDITOR
            if (none == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.PATH));
#endif
            return none;
        }
    }

    static List<ResourceType> resources;
    static List<ResourceType> Resources
    {
        get
        {
#if UNITY_EDITOR
            if (resources == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.PATH));
#endif
            return resources;
        }
    }

    static List<ResourceType> fluids;
    static List<ResourceType> Fluids
    {
        get
        {
#if UNITY_EDITOR
            if (fluids == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.PATH));
#endif
            return fluids;
        }
    }

    static List<ResourceTypeCategory> fullRes;
    public static List<ResourceTypeCategory> FullRes
    {
        get
        {
#if UNITY_EDITOR
            if (fullRes == null)
                InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.PATH));
#endif 
            return fullRes;
        }
    }
#if UNITY_EDITOR
    [MenuItem("Custom Editors/Refresh Resource _&r", priority = 0)]
    [InitializeOnLoadMethod]
#endif
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async Task Init()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
#if UNITY_EDITOR
        data = AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.EDITOR_PATH);
#else
        data = await Addressables.LoadAssetAsync<ResourceData>(ResourceData.PATH).Task;
#endif
        InitFill(data);
    }

    static void InitFill(ResourceData data)
    {
        fullRes = data.Categories;
        resources = fullRes.Skip(1).SkipLast(1).SelectMany(q => q.Objects).Select(q => q.data).ToList();

        fluids = fullRes[^1].Objects.Select(q => q.data).ToList();
        none = fullRes[0].Objects.First(q => q.Name == "None").data;
        Money = fullRes[0].Objects.First(q => q.Name == "Money").data;
    }

    public static ResourceType GetTypeByName(string name)
    {
        ResourceType type = null;
        if ((type = GetResByName(name)) != null)
            return type;
        else
            return GetFluidByName(name);
    }

    public static ResourceType GetResByName(string name)
    {
        ResourceType type = Resources.FirstOrDefault(q => q.Name == name);
        if (type == null)
        {
            if (name == "None")
                return None;
            else if (name == "Money")
                return Money;
        }
        return type;
    }

    public static ResourceType GetFluidByName(string name)
    {
        ResourceType type = Fluids.FirstOrDefault(q => q.Name == name);
        if (type == null)
        {
            if (name == "None")
                return None;
        }
        return type;
    }

    public static ResourceType GetResByIndex(int i)
        => Resources[i];

    public static ResourceType GetFluidByIndex(int index)
        => Fluids[index];

    public static List<string> GetResNamesList()
        => Resources.Select(q => q.Name).ToList();
    public static List<string> GetResNamesList(List<int> allowedCategories)
    {
        List<string> names = new();
        //#if UNITY_EDITOR
        names.Add(None.Name);
        //#endif
        if (allowedCategories == null || allowedCategories.Count == 0)
            foreach (var category in fullRes)
            {
                names.AddRange(category.Objects.Select(q => q.Name));
                names.Add("");
            }
        else
            foreach (int i in allowedCategories)
            {
                names.AddRange(FullRes[i].Objects.Select(q => q.Name));
                names.Add("");
            }
        return names;
    }
    public static List<ResourceType> GetResList()
        => Resources.Skip(2).ToList();

    public static int GetResourceIndex(ResourceType resourceType)
        => Resources.IndexOf(resourceType);

    public static List<string> GetFluidNames()
        => Fluids.Select(q => q.Name).ToList();

    public static List<ResourceType> LoadTypeList(List<DataAssign> types)
    {
        List<ResourceType> results = new();
        foreach (var t in types)
        {
            results.Add(fullRes.FirstOrDefault(q => q.id == t.categoryId).Objects.FirstOrDefault(q => q.id == t.objectId)?.data);
        }
        return results;
    }

    public static DataAssign GetSaveIndex(ResourceType q)
    {
        return data.GetSaveIndexByName(q.Name);
    }

    public static ResourceData GetData()
        => data;

}