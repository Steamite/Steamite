using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "InfoWindowControlHolder", menuName = "InfoWindowControlHolder", order = 99)]
public class InfoWindowControlHolder : ScriptableObject
{
    [SerializeField] List<VisualTreeAsset> visualTreeAssets;

    public void CreateElementByName(string name, VisualElement parent, object data)
    {
        try
        {
            visualTreeAssets
                .Where(q => q != null)
                .FirstOrDefault(q => q.name == name).CloneTree(parent);
            parent[parent.childCount - 1].dataSource = data;
            ((IUIElement)parent[parent.childCount - 1]).Open(data);
        }
        catch (Exception e)
        {
            if (e is NullReferenceException)
                Debug.LogError(
                    $"Element with this name is not pressent in the collection: {name}\n" +
                    $"{e}");
        }
    }
}