using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AbstractControls
{
    public class RadioButtonData
    {
        public string text;

        public RadioButtonData(string _text)
        {
            text = _text;
        }
    }

    /// <summary>
    /// List is for simple buttons under one parent. And have a lot of dynamic changes.
    /// </summary>
    [UxmlElement]
    public partial class CustomRadioButtonList : ListView, IBindable
    {
        protected new VisualElement contentContainer;
        protected Action<int> changeEvent;

        int _selID;
        protected int SelectedChoice
        {
            get
            {
                return _selID;
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
            // Default settings (adjust as needed)
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            makeItem = DefaultMakeItem;
            bindItem = DefaultBindItem;
            bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign;
            reorderable = false;
            selectionType = SelectionType.None;
        }

        /// <summary>
        /// // Removes an item from the list and refreshes the ListView
        /// </summary>
        /// <param name="index"></param>
        public void RemoveItem(int index)
        {
            itemsSource.RemoveAt(index);
            if (index == SelectedChoice)
            {
                SelectedChoice = -1;

                //_selectedButton?.Deselect();
            }
            RefreshItems();
        }
        /// <summary>
        /// Clears all items from the list
        /// </summary>
        protected void ClearItems()
        {
            itemsSource = null;
        }

        // Default method for creating an item (override in specific use cases)
        protected virtual CustomRadioButton DefaultMakeItem() => throw new NotImplementedException();

        // Default method for binding an item (override in specific use cases)
        protected virtual void DefaultBindItem(VisualElement element, int index)
        {
            element.RemoveFromClassList("unity-collection-view__item");
            element.RemoveFromClassList("unity-list-view__item");
            (element as CustomRadioButton).value = index;
            (element as CustomRadioButton).text = ((RadioButtonData)itemsSource[index]).text;
            return;
        }

        #endregion

        public virtual void Init(Action<int> onChange)
        {
            SelectedChoice = -1;
            changeEvent = onChange;
            contentContainer = this.Q<VisualElement>("unity-content-container");
        }

        /// <summary>
        /// Need to use with buttons instead of indexes for proper unbinding.
        /// </summary>
        /// <param name="customRadioButton">Button pressed.</param>
        public virtual void Select(int index)
        {
            if(SelectedChoice > -1)
                ((CustomRadioButton)contentContainer.Children()
                    .FirstOrDefault(q => ((CustomRadioButton)q)?.value == SelectedChoice))?.Deselect();
            if(SelectedChoice == index)
            {
                SelectedChoice = -1;
            }
            else
            {
                SelectedChoice = index;
            }
        }
    }
}