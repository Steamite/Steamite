using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

[Serializable]
public class ResourceSkin
{
    public string name;
    public Color color;
    public Sprite icon;
}

[CreateAssetMenu(fileName = "Resource Skin", menuName = "Scriptable Objects/Resource Skin")]
public class ResourceSkins : ScriptableObject
{
    [SerializeField]
    public List<ResourceSkin> skins = new();
    
    private void OnValidate()
    {
        string[] resNames = Enum.GetNames(typeof(ResourceType));
        
        while(skins.Count > resNames.Length)
        {
            skins.RemoveAt(skins.Count - 1);
        }
        while (skins.Count < resNames.Length)
        {
            skins.Add(new());
        }


        for (int i = 0; i < skins.Count; i++)
        {
            skins[i].name = resNames[i];
        }
    }

    public Color GetResourceColor(ResourceType resourceType)
    {
        return skins.FirstOrDefault(q => q.name == resourceType.ToString()).color;
    }
}
