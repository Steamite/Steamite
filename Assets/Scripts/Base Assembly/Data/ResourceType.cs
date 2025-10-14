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
}

[Serializable]
public class ResourceWrapper : DataObject
{
    [SerializeField]public ResourceType data;

    public ResourceWrapper(int _id) : base(_id)
    {
    }

    public ResourceWrapper() : base() { }
}


public static class ResFluidTypes
{
    const string RES_PATH = "Assets/Game Data/Resource Types/Resource/Resources.asset";
    const string FLUID_PATH = "Assets/Game Data/Resource Types/Fluid/Fluids.asset";
    const string MONEY_PATH = "Assets/Game Data/Resource Types/Money.asset";
    const string RESOURCE_PATH = "Assets/Game Data/ResourceData.asset";

    public static ResourceType Money;

    public static ResourceType None;
    
    static List<ResourceType> resources;
    static List<ResourceType> fluids;
#if UNITY_EDITOR
    [MenuItem("Custom Editors/Refresh Resource _&r", priority = 0)]
#endif
    public static async Task Init()
    {
        ResourceData data = await Addressables.LoadAssetAsync<ResourceData>(RESOURCE_PATH).Task;
        resources = data.Categories.SelectMany(q => q.Objects).Select(q => q.data).ToList();

        fluids = data.Categories.SelectMany(q => q.Objects).Select(q => q.data).ToList();
        None = resources[0];
        Money = resources[1];
    }

    public static ResourceType GetResByName(string name)
        => resources.First(q => q.Name == name);
    public static ResourceType GetResByIndex(int i)
        => resources[i];

    public static ResourceType GetFluidByIndex(int index)
        => fluids[index];


    public static List<string> GetResNamesList()
        => resources.Select(q => q.Name).ToList();
    public static List<ResourceType> GetResList()
        => resources.Skip(2).ToList();

    public static int GetResourceIndex(ResourceType resourceType)
        => resources.IndexOf(resourceType);

    public static List<string> GetFluidNames()
        => fluids.Select(q => q.Name).ToList();
}