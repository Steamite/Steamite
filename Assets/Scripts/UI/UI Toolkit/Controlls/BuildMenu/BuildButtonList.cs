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
            contentContainer.AddToClassList("build-bar");
            SetItemSource(null);
        }

        public void SetItemSource(List<BuildingWrapper> wrappers)
        {
            if (wrappers == null)
                style.display = DisplayStyle.None;
            else
            {
                style.display = DisplayStyle.Flex;
                itemsSource =
                    wrappers.Select(
                        q => new RadioBuildButtonData(
                            q.building.objectName,
                            q.preview)
                            as RadioButtonData).ToList();
            }
        }

        

        protected override CustomRadioButton DefaultMakeItem()
        {
            CustomRadioButton button = new("building-button", -1, false, true);
            button.Add(new());
            button[0].name = "img";
            button[0].AddToClassList("building-background");
            button.Add(new Label());
            button[1].name = "label";
            return button;
        }

        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            element.Q<VisualElement>("img").style.backgroundImage = new(((RadioBuildButtonData)itemsSource[index]).img);
            element.Q<Label>("label").text = ((RadioBuildButtonData)itemsSource[index]).text;
        }
    }
}