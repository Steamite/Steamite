using System;
using UnityEngine;

[Serializable]
public class ResourceType : DataObject
{
    public Color color;
    public Texture2D image;

    public ResourceType(int _id) : base(_id)
    {
    }

    public ResourceType() : base() { }
}


