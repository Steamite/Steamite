using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            if (resources == null)
            {
                Debug.Log("Resources not ready");
                AsyncContext.Run(() => Init());
            }
            return resources;
        }
    }

    static List<ResourceType> fluids;
    static List<ResourceType> Fluids
    {
        get
        {
            if (fluids == null)
            {
                Debug.Log("Fluids not ready");
                Task.Run(Init);
            }
            return fluids;
        }
    }

#if UNITY_EDITOR
    [MenuItem("Custom Editors/Refresh Resource _&r", priority = 0)]
    [InitializeOnLoadMethod]
#endif
    public static async Task Init()
    {
        ResourceData data = await Addressables.LoadAssetAsync<ResourceData>(RESOURCE_PATH).Task;
        resources = data.Categories.Where(q => q.Name != "Fluids").SelectMany(q => q.Objects).Select(q => q.data).ToList();

        fluids = data.Categories.Where(q => q.Name == "Fluids").SelectMany(q => q.Objects).Select(q => q.data).ToList();
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
    public static List<ResourceType> GetResList()
        => Resources.Skip(2).ToList();

    public static int GetResourceIndex(ResourceType resourceType)
        => Resources.IndexOf(resourceType);

    public static List<string> GetFluidNames()
        => Fluids.Select(q => q.Name).ToList();
}