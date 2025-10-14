using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceTypeHolder", menuName = "Resources/ResourceTypeHolder")]
public class ResourceTypeHolder : ScriptableObject
{
    public List<ResourceType> types;

    private void OnValidate()
    {
        types = types.Distinct().ToList();
    }
}
