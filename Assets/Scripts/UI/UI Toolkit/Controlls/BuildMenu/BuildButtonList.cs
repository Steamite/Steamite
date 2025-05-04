using System;
using System.Collections.Generic;
using System.Linq;
using AbstractControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace BottomBar.Building
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
        List<BuildingWrapper> wrappers;
        public BuildButtonList() { }
        public BuildButtonList(Action<int> changeAction)
        {
            pickingMode = PickingMode.Ignore;
            hierarchy[0].pickingMode = PickingMode.Ignore;
            contentContainer.parent.pickingMode = PickingMode.Ignore;
            contentContainer.parent.parent.pickingMode = PickingMode.Ignore;

            Init(changeAction);
            contentContainer.AddToClassList("build-bar");
            SetItemSource(null);

            unbindItem = DefaultUnBindItem;
        }

        public void SetItemSource(List<BuildingWrapper> _wrappers)
        {
            wrappers = _wrappers;
            if (_wrappers == null)
                style.display = DisplayStyle.None;
            else
            {
                style.display = DisplayStyle.Flex;
                itemsSource =
                    _wrappers.Select(
                        q => new RadioBuildButtonData(
                            q.building.objectName,
                            q.preview)
                            as RadioButtonData).ToList();
            }
        }

        

        protected override CustomRadioButton DefaultMakeItem()
        {
            CustomRadioButton button = new("build-button", -1, false, true);
            button.Add(new());
            button[0].name = "img";
            button[0].AddToClassList("building-background");
            button.Add(new Label());
            button[1].name = "label";
            button[1].AddToClassList("build-button-label");
            return button;
        }

        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            element.Q<VisualElement>("img").style.backgroundImage = new(((RadioBuildButtonData)itemsSource[index]).img);
            element.Q<Label>("label").text = ((RadioBuildButtonData)itemsSource[index]).text;
            ((Button)element).text = "";
            element.RegisterCallback<PointerEnterEvent>(Hover);
            element.RegisterCallback<PointerLeaveEvent>(EndHove);
        }

        protected void DefaultUnBindItem(VisualElement element, int index)
        {
            element.UnregisterCallback<PointerEnterEvent>(Hover); 
            element.UnregisterCallback<PointerLeaveEvent>(EndHove);
        }

        private void Hover(PointerEnterEvent evt)
        {
            int i = ((CustomRadioButton)evt.target).value;
            ToolkitUtils.localMenu.Open(wrappers[i].building, (CustomRadioButton)evt.target);
        }
        private void EndHove(PointerLeaveEvent evt)
        {
            ToolkitUtils.localMenu.Close();
        }

    }
}