using InfoWindowElements;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[UxmlElement]
public partial class ResourceCell : ResourceList
{
    Resource resource;
    MoneyResource moneyResource;
    public Object whatToSave;
    IntegerField capacityField;
    Label noneLabel;

    [UxmlAttribute]
    List<int> allowedCategories;
    public ResourceCell() : base()
    {
        focusable = true;
        showEmpty = true;
        unbindItem = UnbindItem;
        onAdd =
            (_) =>
            {
                resource.types.Add(ResFluidTypes.None);
                resource.ammounts.Add(0);
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
                    resource.types.RemoveAt(el.selectedIndex);
                    resource.ammounts.RemoveAt(el.selectedIndex);
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
                    (moneyResource).Money = new(ev.newValue);
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

    #region Value Updates
    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    private void ChangeType(ChangeEvent<string> evt)
    {
        int i = evt.target.GetRowIndex(false);
        int j = resource.types.IndexOf(ResFluidTypes.GetResByName(evt.newValue));
        if (j > -1 && i != j)
        {
            resource.ammounts[j] += resource.ammounts[i];
            resource.types.RemoveAt(i);
            resource.ammounts.RemoveAt(i);
            itemsSource = ToUIRes(resource);
        }
        else
            resource.types[i] = ResFluidTypes.GetResByName(evt.newValue);
        EditorUtility.SetDirty(whatToSave);
    }

    /// <summary>
    /// Changes the ammount of a given type (<paramref name="evt"/>).
    /// </summary>
    /// <param name="evt">Event with the new value and changed element.</param>
    private void ChangeVal(ChangeEvent<int> evt)
    {
        int i = evt.target.GetRowIndex(false);
        resource.ammounts[i] = evt.newValue;
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
        whatToSave = _whatToSave;
        if (_resource != null)
        {
            showAddRemoveFooter = true;
            capacityField.labelElement.text = _cost ? "Cost" : "Capacity";
            if (_resource is MoneyResource _moneyRes)
            {
                moneyResource = _moneyRes;
                capacityField.value = +_moneyRes.Money.BaseValue;
                resource = _moneyRes.EditorResource;
                capacityField.visible = true;
            }
            else
            {
                resource = _resource;
                capacityField.visible = false;
            }
            itemsSource = ToUIRes(resource);
            noneLabel.text = "Empty";
            style.display = DisplayStyle.Flex;
            capacityField.style.display = DisplayStyle.Flex;
        }
        else
        {
            resource = null;
            showAddRemoveFooter = false;
            itemsSource = new List<UIResource>();
            noneLabel.text = "Nothing";
            style.display = DisplayStyle.None;
            capacityField.style.display = DisplayStyle.None;
        }
    }
}
