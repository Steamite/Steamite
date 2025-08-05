using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "InfoWindowControlHolder", menuName = "InfoWindowControlHolder", order = 99)]
public class InfoWindowControlHolder : ScriptableObject
{
    [SerializeField] List<VisualTreeAsset> visualTreeAssets;

    public void CreateElementByName(string name, VisualElement parent, object data)
    {
        VisualTreeAsset asset = visualTreeAssets.Find(q => q.name == name);
        if (asset)
        {
            asset.CloneTree(parent);
            parent[parent.childCount - 1].dataSource = data;
            ((IUIElement)parent[parent.childCount - 1]).Open(data);
        }
        else
        {
            Debug.LogError($"'{name}' is not present in the catalog.");
        }

    }
}