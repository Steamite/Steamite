using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class CustomRadioButtonGroup : ListView
{
    List<CustomRadioButton> _itemsSource;
    CustomRadioButton _selectedButton;

    VisualElement _contentContainer;
    ScrollView _scrollView;
    public int value => _selectedButton.value;

    #region Base
    [Obsolete]
    public new class UxmlFactory : UxmlFactory<CustomRadioButtonGroup, UxmlTraits> { }

    public CustomRadioButtonGroup()
    {
        // Initialize the internal item source
        _itemsSource = new List<CustomRadioButton>();
        itemsSource = _itemsSource;

        // Default settings (adjust as needed)
        fixedItemHeight = 30;
        makeItem = DefaultMakeItem;
        bindItem = DefaultBindItem;
        bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign;
    }

    // Adds an item to the list and refreshes the ListView
    public void AddItem(CustomRadioButton item)
    {
        _itemsSource.Add(item);
        Rebuild();
    }

    // Removes an item from the list and refreshes the ListView
    public void RemoveItem(CustomRadioButton item)
    {
        _itemsSource.Remove(item);
        Rebuild();
    }

    // Clears all items from the list
    public void ClearItems()
    {
        _itemsSource.Clear();
        Rebuild();
    }

    // Default method for creating an item (override in specific use cases)
    private CustomRadioButton DefaultMakeItem()
    {
        return new CustomRadioButton();
    }

    // Default method for binding an item (override in specific use cases)
    private void DefaultBindItem(VisualElement element, int index)
    {
        element.RemoveFromClassList("unity-collection-view__item");
        element.RemoveFromClassList("unity-list-view__item");
        (element as CustomRadioButton).text = _itemsSource[index].data;
        (element as CustomRadioButton).style.fontSize = new(new Length(40, LengthUnit.Percent));
        (element as CustomRadioButton).style.height = new(new Length(98.3f, LengthUnit.Pixel));
        (element as CustomRadioButton).RegisterCallback<ClickEvent>((element as CustomRadioButton).Select);

        return;
    }
    #endregion

    public void Init()
    {
        for (int i = 0; i < 9; i++)
        {
            AddItem(new($"Save {i}#", i));
        }
        _contentContainer = this.Q<VisualElement>("unity-content-container");
        _contentContainer.style.flexGrow = 1;
    }

    public void Select(CustomRadioButton customRadioButton)
    {
        _selectedButton?.Deselect();
        _selectedButton = customRadioButton;
    }
}
