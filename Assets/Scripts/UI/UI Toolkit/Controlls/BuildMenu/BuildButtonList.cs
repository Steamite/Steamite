using AbstractControls;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BottomBar.Building
{
    public class RadioBuildButtonData : RadioButtonData
    {
        public Sprite img;
        public bool unlocked;
        public RadioBuildButtonData(string _txt, Sprite _img, bool _unlocked) : base(_txt)
        {
            img = _img;
            unlocked = _unlocked;
        }
    }
    [UxmlElement]
    public partial class BuildButtonList : CustomRadioButtonList
    {
        const float itemWidth = 100f;
        const string BUILD_BUTTON_CLASS = "build-button";
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
            {
                itemsSource = null;
                style.display = DisplayStyle.None;
            }
            else
            {
                itemsSource = null;
                style.display = DisplayStyle.None;
                schedule.Execute(() =>
                {
                    style.display = DisplayStyle.Flex;
                    itemsSource =
                        _wrappers.Select(
                            q => new RadioBuildButtonData(
                                q.building.objectName,
                                q.preview,
                                q.unlocked)
                                as RadioButtonData).ToList();
                });
            }
        }



        protected override CustomRadioButton DefaultMakeItem()
        {
            CustomRadioButton button = new(BUILD_BUTTON_CLASS, -1, false, true);
            button.Add(new());
            button[0].name = "img";
            button[0].AddToClassList("building-background");
            button.Add(new Label());
            button[1].name = "label";
            button[1].AddToClassList("build-button-label");

            VisualElement blocker = new();
            blocker.AddToClassList("button-blocker");
            blocker.AddToClassList("not-research-blocker");
            blocker.pickingMode = PickingMode.Ignore;
            button[0].Add(blocker);

            blocker = new();
            button.Add(new());
            blocker.AddToClassList("button-blocker");
            blocker.AddToClassList("cannot-afford-blocker");
            blocker.pickingMode = PickingMode.Ignore;
            button[0].Add(blocker);

            return button;
        }

        public void UnlockActiveButton(int index)
        {
            ((RadioBuildButtonData)itemsSource[index]).unlocked = true;
            RefreshItem(index);
        }

        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            //element.style.display = DisplayStyle.Flex;
            if (((RadioBuildButtonData)itemsSource[index]).unlocked)
            {
                // hide locked blocker
                element[0][0].style.display = DisplayStyle.None;
                if (wrappers[index].building == SceneRefs.GridTiles.BuildPrefab)
                {
                    element.AddToClassList($"{BUILD_BUTTON_CLASS}-selected");
                }
            }
            else
            {
                // the locked blocker
                element[0][0].style.display = DisplayStyle.Flex;
            }
            element.Q<VisualElement>("img").style.backgroundImage = new(((RadioBuildButtonData)itemsSource[index]).img);
            element.Q<Label>("label").text = ((RadioBuildButtonData)itemsSource[index]).text;
            ((Button)element).text = "";

            DataBinding binding = BindingUtil.CreateBinding(nameof(ResourceDisplay.GlobalResources));
            binding.sourceToUiConverters.AddConverter<MoneyResource, StyleEnum<DisplayStyle>>((ref MoneyResource res) => 
            {
                if (wrappers[index].unlocked == false)
                    return DisplayStyle.None;

                if (MyRes.CanAfford(wrappers[index].building.Cost))
                {
                    return DisplayStyle.None;
                }
                else
                {
                    return DisplayStyle.Flex;
                }
            });
            element[0][1].dataSource = MyRes.resDataSource;
            element[0][1].SetBinding("style.display", binding);

            element.RegisterCallback<PointerEnterEvent>(Hover);
            element.RegisterCallback<PointerLeaveEvent>(EndHove);
        }

        protected void DefaultUnBindItem(VisualElement element, int index)
        {
            ToolkitUtils.ChangeClassWithoutTransition($"{BUILD_BUTTON_CLASS}-selected", BUILD_BUTTON_CLASS, element);
            element.AddToClassList(BUILD_BUTTON_CLASS);
            element.ClearBindings();
            element.dataSource = null;
            element.UnregisterCallback<PointerEnterEvent>(Hover);
            element.UnregisterCallback<PointerLeaveEvent>(EndHove);
        }

        private void Hover(PointerEnterEvent evt)
        {
            int i = ((CustomRadioButton)evt.target).selIndex;
            ToolkitUtils.localMenu.UpdateContent(wrappers[i], (CustomRadioButton)evt.target);
        }
        private void EndHove(PointerLeaveEvent evt)
        {
            ToolkitUtils.localMenu.Close();
        }

        public override bool Select(int index)
        {
            if (SelectedChoice > -1)
                ((CustomRadioButton)contentContainer.Children()
                    .FirstOrDefault(q => ((CustomRadioButton)q)?.selIndex == SelectedChoice))?.Deselect();
            if (index == -1 || SelectedChoice == index || !MyRes.CanAfford(wrappers[index].building.Cost))
            {
                SelectedChoice = -1;
                return false;
            }
            else if (wrappers[index].unlocked)
            {
                SelectedChoice = index;
                return true;
            }
            else
            {
                UIRefs.ResearchWindow.OpenWindow(wrappers[index]);
                return false;
            }
        }
    }
}