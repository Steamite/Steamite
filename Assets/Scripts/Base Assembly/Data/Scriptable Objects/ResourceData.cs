using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Resources/ResourceData")]
public class ResourceData : DataHolder<ResourceTypeCategory, ResourceWrapper>
{
    public new const string PATH = "Assets/Game Data/ResourceData.asset";
}