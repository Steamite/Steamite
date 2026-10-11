

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.Analytics.IAnalytic;

public static class ResFluidTypes
{
    static ResourceData data;
    public static ResourceType Money;

    static ResourceType none;
    public static ResourceType None => none;

    static List<ResourceType> resources;

    static List<ResourceType> fluids;

    static List<ResourceTypeCategory> fullRes;
    public static List<ResourceTypeCategory> FullRes => fullRes;

    public static async Awaitable Init()
    {
        InitFill(await Addressables.LoadAssetAsync<ResourceData>(ResourceData.PATH).Task);
    }

#if UNITY_EDITOR
    static bool loaded = false;
    [InitializeOnLoadMethod]
    public static void Clear()
    {
        loaded = false;
    }

    [MenuItem("Custom Editors/Refresh Resource _&r", priority = 0)]
    public static void EditorInit()
    {
        if (loaded == true)
            return;

        InitFill(AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.EDITOR_PATH));
        loaded = true;
    }
#endif

    static void InitFill(ResourceData _data)
    {
        data = _data;
        fullRes = _data.Categories;
        resources = fullRes.Skip(1).SkipLast(1).SelectMany(q => q.Objects).Select(q => q.data).ToList();

        fluids = fullRes[^1].Objects.Select(q => q.data).ToList();
        none = fullRes[0].Objects.First(q => q.Name == "None").data;
        Money = fullRes[0].Objects.First(q => q.Name == "Money").data;
        Debug.Log("Resources loaded");
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
        ResourceType type = resources.FirstOrDefault(q => q.Name == name);
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
        ResourceType type = fluids.FirstOrDefault(q => q.Name == name);
        if (type == null)
        {
            if (name == "None")
                return None;
        }
        return type;
    }

    public static ResourceType GetResByIndex(int i)
    {
        return resources[i];
    }
    
    public static int GetResMax() => resources.Count;
    

    public static ResourceType GetFluidByIndex(int index) => fluids[index];
    
    public static List<string> GetOutpostTypes = new()
        {
            "None",
            "Meat",
            "Wood"
        };

    public static List<string> GetResNamesList()
        => resources.Select(q => q.Name).ToList();
    public static List<string> GetResNamesList(List<int> allowedCategories)
    {
        List<string> names = new();
        //#if UNITY_EDITOR
        names.Add(none.Name);
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
        => resources.Skip(2).ToList();

    public static int GetResourceIndex(ResourceType resourceType)
        => resources.IndexOf(resourceType);

    public static List<string> GetFluidNames()
        => fluids.Select(q => q.Name).ToList();

    public static List<ResourceType> LoadTypeList(List<DataAssign> types)
    {
        List<ResourceType> results = new();
        foreach (var t in types)
        {
            results.Add(LoadType(t));
        }
        return results;
    }
    public static ResourceType LoadType(DataAssign type)
    {
        return fullRes
            .FirstOrDefault(q => q.id == type.categoryId)
            .Objects
            .FirstOrDefault(q => q.id == type.objectId)?.data;
    }


    public static DataAssign GetSaveIndex(ResourceType q)
    {
        return data.GetSaveIndexByName(q.Name);
    }

    public static ResourceData GetData()
        => data;

}