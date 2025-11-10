using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "InfoWindowControlHolder", menuName = "InfoWindowControlHolder", order = 99)]
public class InfoWindowControlHolder : ScriptableObject
{
    [SerializeField] List<VisualTreeAsset> visualTreeAssets;

    public void CreateElementByName(string name, VisualElement parent, object data, float maxWidth = 100)
    {
        VisualTreeAsset asset = visualTreeAssets.Find(q => q.name == name);
        if (asset)
        {
            asset.CloneTree(parent);
            VisualElement el = parent[parent.childCount - 1];
            parent.Add(el);
            el.dataSource = data;
            el.style.maxWidth = new Length(maxWidth, LengthUnit.Percent);
            ((IUIElement)el).Open(data);
        }
        else
        {
            Debug.LogError($"'{name}' is not present in the catalog.");
        }

    }
}