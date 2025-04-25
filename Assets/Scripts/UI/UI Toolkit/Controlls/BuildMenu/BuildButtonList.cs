using System;
using System.Collections.Generic;
using System.Linq;
using AbstractControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuildMenu
{
    public class RadioBuildButtonData : RadioButtonData
    {
        public Sprite img;

        public RadioBuildButtonData(string _txt, Sprite _img) : base(_txt)
        {
            img = _img;
        }
    }
    [UxmlElement]
    public partial class BuildButtonList : CustomRadioButtonList
    {
        public BuildButtonList() { }
        public BuildButtonList(Action<int> changeAction)
        {
            Init(changeAction);
        }

        public void SetItemSource(List<BuildingWrapper> wrappers)
        {
            if (wrappers == null)
                style.display = DisplayStyle.None;
            else
                style.display = DisplayStyle.Flex;
            _itemsSource = 
                wrappers.Select(
                    q => new RadioBuildButtonData(
                        q.building.objectName, 
                        q.preview)
                        as RadioButtonData).ToList();
        }

        

        protected override CustomRadioButton DefaultMakeItem()
        {
            CustomRadioButton button = new();
            button.styleClass = "building-blueprint";
            button.Add(new());
            button[0].name = "img";
            button.Add(new Label());
            button[1].name = "label";
            return button;
        }

        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            element.Q<VisualElement>("img").style.backgroundImage = new(((BuildingWrapper)itemsSource[index]).preview);
            element.Q<Label>("label").text = ((BuildingWrapper)itemsSource[index]).building.objectName;
        }
    }
}