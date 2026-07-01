using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.LevelList
{
    [UxmlElement]
    public partial class LevelCell : VisualElement
    {
        Toggle toggle;
        SliderInt intSlider;
        //DropdownField dropdownField;
        MultiColumnListView listView;

        RadioButtonGroup group;
        public LevelCell()
        {

        }
        public LevelCell(MultiColumnListView view)
        {
            Add(intSlider = new SliderInt(1, Building.MAX_LEVEL) { showInputField = true});
            //Add(dropdownField = new DropdownField());
            Add(group = new RadioButtonGroup());
            listView = view;
        }

        public void Open(object data, int i)
        {
            BuildingWrapper wrapper = data as BuildingWrapper;
            if (wrapper.selectedLevel == -1)
                wrapper.selectedLevel = 0;

            Building building = wrapper.building;
            if (building == null)
                return;

            SerializedObject sO = new(building);
            SerializedProperty prop = sO.FindProperty(nameof(Building.maxLevel));
            intSlider.Unbind();
            intSlider.BindProperty(prop);
            intSlider.TrackPropertyValue(prop, 
                (p) => 
                {
                    if (wrapper.selectedLevel >= p.intValue)
                        wrapper.selectedLevel = p.intValue-1;
                    listView.RefreshItem(i);
                });


            List<string> choices = new();
            for (int j = 0; j < building.maxLevel; j++)
            {
                choices.Add(j.ToString());
            }
            /*dropdownField.style.display = DisplayStyle.None;
            dropdownField.choices = choices;

            dropdownField.SetValueWithoutNotify(wrapper.selectedLevel.ToString());
            dropdownField.UnregisterValueChangedCallback(Refresh);
            dropdownField.RegisterValueChangedCallback(Refresh);*/


            group.style.display = DisplayStyle.Flex;
            //group[1].style.flexDirection = FlexDirection.Row;
            group.SetValueWithoutNotify(wrapper.selectedLevel);
            group.choices = choices;
            group.UnregisterValueChangedCallback(Refresh);
            group.RegisterValueChangedCallback(Refresh);

        }

        void Refresh(ChangeEvent<string> ev)
        {
            int i = ev.target.GetRowIndex();
            Refresh(i, int.Parse(ev.newValue));
        }
        void Refresh(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Refresh(i, ev.newValue);
        }
        void Refresh(int i, int x)
        {
            (listView.itemsSource[i] as BuildingWrapper).selectedLevel = x;
            listView.RefreshItem(i);
        }
    }
}
