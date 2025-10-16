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
    [Obsolete("Use Name", true)]
    string name;
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


