using Assets.Scripts.Editor.Research;
using InfoWindowElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[UxmlElement]
public partial class EditorResourceCell : VisualElement
{
    SerializedProperty resourceProperty;
    EditorResourceList resourceList;
    IntegerField capacityField;

    [UxmlAttribute]
    public List<int> allowedCategories;

    public EditorResourceCell()
    {
        resourceList = new();
        Add(resourceList);
        focusable = true;
/*
        showEmpty = true;
        unbindItem = UnbindItem;
        onAdd = Add;
        onRemove = Remove;
        allowAdd = true;
        allowRemove = false;
        selectionType = SelectionType.Single;*/

        #region Capacity Field
        capacityField = new IntegerField("Capacity");
        capacityField.Q<Label>().style.minWidth = 0;
        /*capacityField.
        capacityField.RegisterValueChangedCallback<int>(
            (ev) =>
            {
                if (moneyResource != null)
                    (moneyResource).Money = new(ev.newValue);
                EditorUtility.SetDirty(whatToSave);
            });*/
        capacityField.style.width = new Length(50, LengthUnit.Percent);
        capacityField.style.position = Position.Absolute;
        capacityField.style.left = 0;
        capacityField.style.bottom = 0;
        hierarchy.Add(capacityField);
        #endregion
    }
    /*
    #region Item Events

    protected virtual void Add(BaseListView _)
    {
        resource.types.Add(ResFluidTypes.None);
        resource.ammounts.Add(0);
        itemsSource = ToUIRes(resource);
        EditorUtility.SetDirty(whatToSave);
    }

    protected virtual void Remove(BaseListView el)
    {
        if (el.selectedIndex > -1 && el.selectedIndex < itemsSource.Count)
        {
            if (selectedIndex == itemsSource.Count - 1)
                allowRemove = false;
            resource.types.RemoveAt(el.selectedIndex);
            resource.ammounts.RemoveAt(el.selectedIndex);
            itemsSource = ToUIRes(resource);
            EditorUtility.SetDirty(whatToSave);
        }
    }

    protected override VisualElement MakeItem()
    {
        VisualElement visualElement = new();
        visualElement.style.flexDirection = FlexDirection.Row;
        visualElement.focusable = true;
        DropdownField dropField = new();
        dropField.style.width = new Length(100, LengthUnit.Pixel);
        IntegerField integerField = new();
        integerField.style.flexGrow = 1;
        visualElement.Add(dropField);
        visualElement.Add(integerField);
        visualElement.RegisterCallback<PointerDownEvent>(
            evt =>
            {
                allowRemove = true;
                SetSelection(evt.target.GetRowIndex(false));
                evt.StopPropagation();
            });
        return visualElement;
    }

    protected override void BindItem(VisualElement el, int i)
    {
        el.RemoveFromClassList("unity-collection-view__item");
        DropdownField type = el.Q<DropdownField>();
        type.choices = ResFluidTypes.GetResNamesList(allowedCategories);
        type.value = ((UIResource)itemsSource[i]).type?.Name;
        type.RegisterValueChangedCallback(ChangeType);
        type.style.marginRight = 10;

        IntegerField value = el.Q<IntegerField>();
        value.value = ((UIResource)itemsSource[i]).ammount;
        value.RegisterValueChangedCallback(ChangeVal);
    }

    private void UnbindItem(VisualElement el, int i)
    {
        DropdownField type = el.Q<DropdownField>();
        type.UnregisterValueChangedCallback(ChangeType);

        IntegerField value = el.Q<IntegerField>();
        value.UnregisterValueChangedCallback(ChangeVal);
    }

    protected override VisualElement MakeNoneElement()
    {
        VisualElement el = base.MakeNoneElement();
        noneLabel = el.Q<Label>();
        noneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

        return el;
    }
    #endregion
    */
/*
    #region Value Updates
    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    protected virtual void ChangeType(ChangeEvent<string> evt)
    {
        int i = evt.target.GetRowIndex(false);
        ResourceType t = ResFluidTypes.GetTypeByName(evt.newValue);
        int j = resource.types.IndexOf(t);
        if (j > -1 && i != j)
        {
            resource.ammounts[j] += resource.ammounts[i];
            resource.types.RemoveAt(i);
            resource.ammounts.RemoveAt(i);
            itemsSource = ToUIRes(resource);
        }
        else
            resource.types[i] = t;
        EditorUtility.SetDirty(whatToSave);
    }


    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    protected virtual void ChangeVal(ChangeEvent<int> evt)
    {
        int i = evt.target.GetRowIndex(false);
        resource.ammounts[i] = evt.newValue;
        EditorUtility.SetDirty(whatToSave);
    }
    #endregion
*/

    public void Unbind()
    {
        resourceList.Unbind();
    }

    public void Open(SerializedProperty property)
    {
        resourceList.Open(property);

        if (property == null)
            return;

        switch (property.type)
        {
            case nameof(CapacityResource):
                capacityField.BindProperty(
                    property
                        .FindPropertyRelative("capacity")
                        .FindPropertyRelative("baseValue")
                    );
                break;
            case nameof(MoneyResource):
                capacityField.BindProperty(
                    property
                        .FindPropertyRelative("money")
                        .FindPropertyRelative("baseValue")
                    );
                break;
            default:
                capacityField.style.display = DisplayStyle.None;
                break;
        }
    }
}
