using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Resources/ResourceData")]
public class ResourceData : DataHolder<ResourceTypeCategory, ResourceWrapper>
{
    public new const string PATH = "ResourceData";
#if UNITY_EDITOR
    public new const string EDITOR_PATH = "Assets/Game Data/ResourceData.asset";

#endif
}