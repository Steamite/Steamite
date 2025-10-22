using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Resources/ResourceData")]
public class ResourceData : DataHolder<ResourceTypeCategory, ResourceWrapper>
{
    public new const string PATH = "Assets/Game Data/ResourceData.asset";
}