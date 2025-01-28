using RadioGroups;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractControls
{    
    [UxmlElement]
    public partial class CustomRadioButtonList : ListView, IBindable
    {
        protected List<CustomRadioButton> _itemsSource;
        protected CustomRadioButton _selectedButton;

        protected ScrollView _scrollView;

        protected Action<int> changeEvent;


        int _selID;
        protected int SelectedId
        {
            get
            {
                return _selID;//_selectedButton != null ? _selectedButton.value : -1; 
            }
            set
            {
                _selID = value;
                changeEvent?.Invoke(value);
            }
        }

        #region List

        public CustomRadioButtonList()
        {
            // Initialize the internal item source
            _itemsSource = new List<CustomRadioButton>();
            // Default settings (adjust as needed)
            fixedItemHeight = 30;
            makeItem = DefaultMakeItem;
            bindItem = DefaultBindItem;
            bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign;
            reorderable = false;
            selectionType = SelectionType.None;
        }

        /// <summary>
        /// Adds an item to the list and refreshes the ListView
        /// </summary>
        /// <param name="item"></param>
        protected void AddItem(CustomRadioButton item)
        {
            _itemsSource.Add(item);
            makeItem();
            Rebuild();
        }
        /// <summary>
        /// Removes an item from the list and refreshes the ListView
        /// </summary>
        /// <param name="item"></param>
        protected void RemoveItem(CustomRadioButton item)
        {
            _itemsSource.Remove(item);
            if(item == _selectedButton)
            {
                _selectedButton?.Deselect();
                _selectedButton = null;
            }
            Rebuild();
        }
        /// <summary>
        /// // Removes an item from the list and refreshes the ListView
        /// </summary>
        /// <param name="index"></param>
        public void RemoveItem(int index)
        {
            _itemsSource.RemoveAt(index);
            if (index == SelectedId)
            {
                SelectedId = -1;
                _selectedButton?.Deselect();
            }
            Rebuild();
        }
        /// <summary>
        /// Clears all items from the list
        /// </summary>
        protected void ClearItems()
        {
            _itemsSource.Clear();
            Rebuild();
        }

        // Default method for creating an item (override in specific use cases)
        protected virtual CustomRadioButton DefaultMakeItem() => throw new NotImplementedException();

        // Default method for binding an item (override in specific use cases)
        protected virtual void DefaultBindItem(VisualElement element, int index)
        {
            element.RemoveFromClassList("unity-collection-view__item");
            element.RemoveFromClassList("unity-list-view__item");
            (element as CustomRadioButton).value = index;
            (element as CustomRadioButton).text = _itemsSource[index].text;
            //element.RegisterCallback<ClickEvent>((_) => (element as CustomRadioButton).Select());
            return;
        }

        #endregion

        public virtual void Init(Action<int> onChange)
        {
            itemsSource = _itemsSource;
            SelectedId = -1;
            changeEvent = onChange;
        }

        public virtual void Select(CustomRadioButton customRadioButton)
        {
            if(_selectedButton != customRadioButton)
                _selectedButton?.Deselect();
            _selectedButton = customRadioButton;
            SelectedId = _selectedButton.value;
        }
    }
}