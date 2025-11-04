using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceType", menuName = "Resources/ResourceType")]
public class ResourceType : ScriptableObject
{
    [Obsolete("Use Name", true)]
    new string name;
    public string Name;
    public Color color;
    public Texture2D image;
}

[Serializable]
public class ResourceWrapper : DataObject
{
    [SerializeField] public ResourceType data;

    public ResourceWrapper(int _id) : base(_id)
    {
    }

    public ResourceWrapper() : base() { }
}


