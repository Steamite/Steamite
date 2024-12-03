using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractControls
{
    public class CustomRadioButtonGroup : ListView, IBindable
    {
        protected List<CustomRadioButton> _itemsSource;
        protected CustomRadioButton _selectedButton;

        protected VisualElement _contentContainer;
        protected ScrollView _scrollView;

        event Action<int> changeEvent;


        int _selID;
        protected int selectedId
        {
            get
            {
                return _selID;//_selectedButton != null ? _selectedButton.value : -1; 
            }
            private set
            {
                _selID = value;
                changeEvent?.Invoke(value);
            }
        }

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
        protected void AddItem(CustomRadioButton item)
        {
            _itemsSource.Add(item);
            Rebuild();
        }

        // Removes an item from the list and refreshes the ListView
        protected void RemoveItem(CustomRadioButton item)
        {
            _itemsSource.Remove(item);
            Rebuild();
        }

        // Clears all items from the list
        protected void ClearItems()
        {
            _itemsSource.Clear();
            Rebuild();
        }

        // Default method for creating an item (override in specific use cases)
        protected virtual CustomRadioButton DefaultMakeItem()
        {
            return new CustomRadioButton();
        }

        // Default method for binding an item (override in specific use cases)
        protected virtual void DefaultBindItem(VisualElement element, int index)
        {
            element.RemoveFromClassList("unity-collection-view__item");
            element.RemoveFromClassList("unity-list-view__item");
            (element as CustomRadioButton).value = index;
            return;
        }

        #endregion

        public virtual void Init(Action<int> onChange)
        {
            _contentContainer = this.Q<VisualElement>("unity-content-container");
            _contentContainer.style.flexGrow = 1;
            changeEvent = onChange;
            if (_itemsSource.Count > 0) {
                _selectedButton?.Deselect();
                _selectedButton = null;
            }
        }

        public virtual void Select(CustomRadioButton customRadioButton)
        {
            _selectedButton?.Deselect();
            _selectedButton = customRadioButton;
            selectedId = _selectedButton.value;
        }
    }
}