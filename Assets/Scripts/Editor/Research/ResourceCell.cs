using InfoWindowElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[UxmlElement]
public partial class ResourceCell : ResourceList
{

    Resource resource;
    Resource moneyResource;
    public Object whatToSave;
    IntegerField capacityField;
    Label noneLabel;

    public ResourceCell() : base()
    {
        focusable = true;
        showEmpty = true;
        unbindItem = UnbindItem;
        onAdd =
            (_) =>
            {
                resource.type.Add(ResourceType.None);
                resource.ammount.Add(0);
                itemsSource = ToUIRes(resource);
                EditorUtility.SetDirty(whatToSave);
            };
        onRemove =
            (el) =>
            {
                if (el.selectedIndex > -1 && el.selectedIndex < itemsSource.Count)
                {
                    if (selectedIndex == itemsSource.Count - 1)
                        allowRemove = false;
                    resource.type.RemoveAt(el.selectedIndex);
                    resource.ammount.RemoveAt(el.selectedIndex);
                    itemsSource = ToUIRes(resource);
                    EditorUtility.SetDirty(whatToSave);
                }
            };
        allowAdd = true;
        allowRemove = false;
        selectionType = SelectionType.Single;

        #region Capacity Field
        capacityField = new IntegerField("Capacity");
        capacityField.Q<Label>().style.minWidth = 0;
        capacityField.RegisterValueChangedCallback<int>(
            (ev) =>
            {
                if (moneyResource != null)
                    ((MoneyResource)moneyResource).Money = new(ev.newValue);
                EditorUtility.SetDirty(whatToSave);
            });
        capacityField.style.width = new Length(50, LengthUnit.Percent);
        capacityField.style.position = Position.Absolute;
        capacityField.style.left = 0;
        capacityField.style.bottom = 0;
        hierarchy.Add(capacityField);
        #endregion
    }
    #region Item Events
    protected override VisualElement MakeItem()
    {
        VisualElement visualElement = new();
        visualElement.style.flexDirection = FlexDirection.Row;
        visualElement.focusable = true;
        EnumField dropField = new(ResourceType.None);
        dropField.style.width = new Length(100, LengthUnit.Pixel);
        IntegerField integerField = new();
        integerField.style.flexGrow = 1;
        visualElement.Add(dropField);
        visualElement.Add(integerField);
        visualElement.RegisterCallback<PointerDownEvent>(
            evt =>
            {
                allowRemove = true;
                SetSelection(GetRowIndex((VisualElement)evt.target));
                evt.StopPropagation();
            });
        return visualElement;
    }

    protected override void BindItem(VisualElement el, int i)
    {
        el.RemoveFromClassList("unity-collection-view__item");
        EnumField type = el.Q<EnumField>();
        type.value = ((UIResource)itemsSource[i]).type;
        type.RegisterValueChangedCallback<Enum>(ChangeType);
        type.style.marginRight = 10;

        IntegerField value = el.Q<IntegerField>();
        value.value = ((UIResource)itemsSource[i]).ammount;
        value.RegisterValueChangedCallback<int>(ChangeVal);
    }

    private void UnbindItem(VisualElement el, int i)
    {
        EnumField type = el.Q<EnumField>();
        type.UnregisterValueChangedCallback<Enum>(ChangeType);

        IntegerField value = el.Q<IntegerField>();
        value.UnregisterValueChangedCallback<int>(ChangeVal);
    }

    protected override VisualElement MakeNoneElement()
    {
        VisualElement el = base.MakeNoneElement();
        noneLabel = el.Q<Label>();
        noneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

        return el;
    }
    #endregion

    #region Value Updates
    int GetRowIndex(VisualElement element)
    {
        while (!element.ClassListContains("unity-list-view__item"))
        {
            element = element.parent;
        }
        return element.parent.IndexOf(element);
    }

    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    private void ChangeType(ChangeEvent<Enum> evt)
    {
        int i = GetRowIndex((VisualElement)evt.target);
        int j = resource.type.IndexOf((ResourceType)evt.newValue);
        if (j > -1 && i != j)
        {
            resource.ammount[j] += resource.ammount[i];
            resource.type.RemoveAt(i);
            resource.ammount.RemoveAt(i);
            itemsSource = ToUIRes(resource);
        }
        else
            resource.type[i] = (ResourceType)evt.newValue;
        EditorUtility.SetDirty(whatToSave);
    }

    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    private void ChangeVal(ChangeEvent<int> evt)
    {
        int i = GetRowIndex((VisualElement)evt.target);
        resource.ammount[i] = evt.newValue;
        EditorUtility.SetDirty(whatToSave);
    }
    #endregion

    /// <summary>
    /// Preps the resource List using <paramref name="_resource"/>.
    /// </summary>
    /// <param name="_resource">Editing resource.</param>
    /// <param name="_whatToSave">Object containing the resource.</param>
    /// <param name="_cost">Is it a cost resource?</param>
    public void Open(Resource _resource, Object _whatToSave, bool _cost)
    {
        resource = _resource;
        whatToSave = _whatToSave;
        if (resource != null)
        {
            showAddRemoveFooter = true;
            capacityField.labelElement.text = _cost ? "Cost" : "Capacity";
            if (resource is MoneyResource)
            {
                capacityField.value = +((MoneyResource)_resource).Money.BaseValue;
                moneyResource = _resource;
                resource = ((MoneyResource)_resource).EditorResource;
            }
            else
                capacityField.visible = false;
            itemsSource = ToUIRes(resource);
            noneLabel.text = "Empty";
            style.display = DisplayStyle.Flex;
        }
        else
        {
            showAddRemoveFooter = false;
            itemsSource = new List<UIResource>();
            noneLabel.text = "Nothing";
            style.display = DisplayStyle.None;
        }
    }
}
