using Assets.Scripts.Editor.Buildings.LevelList;
using Assets.Scripts.Editor.Columns;
using EditorWindows.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.CommonColumns
{
    public class CommonColumns : ColumnCreators<CommonActions>
    {
        BuildingRegister register;
        public CommonColumns(MultiColumnListView _view, BuildingRegister _register) : base(_view)
        {
            register = _register;
            actions.register = _register;
        }

        public override void CreateColumns()
        { 
            #region Asset
            view.columns["asset"].makeCell =
                () => new ObjectField() { enabledSelf = false, allowSceneObjects = false, objectType = typeof(Building) };
            view.columns["asset"].bindCell =
                (el, i) =>
                {
                    ObjectField field = el as ObjectField;
                    SerializedProperty wrapper = register.ObjectAt(i);
                    SerializedProperty building = wrapper.FindPropertyRelative("building");

                    field.BindProperty(building);
                };
            view.columns["asset"].unbindCell =
                (el, i) =>
                {
                    ObjectField field = el as ObjectField;
                    field.Unbind();
                };
            #endregion

            #region Type
            view.columns["type"].makeCell =
                () => new DropdownField();
            view.columns["type"].bindCell =
                (el, i) =>
                {
                    DropdownField field = (DropdownField)el;
                    field.choices = register.BuildingTypes.Select(q => q.Name).ToList();//.Select(q => q.Name).Where(q => !q.Contains("Pipe")).ToList();
                    field.value = ((BuildingWrapper)view.itemsSource[i]).Building
                        ? ((BuildingWrapper)view.itemsSource[i]).Building.GetType().ToString()
                        : "None";
                    field.RegisterValueChangedCallback(actions.TypeChange);
                };
            view.columns["type"].unbindCell =
                (el, i) =>
                {
                    DropdownField field = (DropdownField)el;
                    field.UnregisterValueChangedCallback(actions.TypeChange);
                };
            #endregion

            #region Level
            /*view.columns["level"].makeCell =
                () => new LevelCell(view);
            view.columns["level"].bindCell =
                (el, i) =>
                {
                    LevelCell cell = el as LevelCell;
                    cell.Open(view.itemsSource[i], i);
                };*/
            #endregion

            #region Cost
            view.columns["cost"].makeCell =
                () => new EditorResourceCell();//FieldLevelList<MoneyResource, ResourceCell>("costs");//new ResourceCell();
            view.columns["cost"].bindCell =
                (el, i) =>
                {
                    el.parent.focusable = true;
                    EditorResourceCell cell = el.Q<EditorResourceCell>();
                    SerializedObject wrapper = register.GetBuildingAt(i);
                    SerializedProperty cost = wrapper.FindProperty("cost");

                    cell.Open(cost);

                    /*BuildingWrapper wrapper = (BuildingWrapper)view.itemsSource[i];
                    Building building = wrapper.Building;
                    if (building == null)
                        return;

                    cell.Open(building.Cost, building, true);*/
                };
            #endregion

            #region Blueprint
            view.columns["blueprint"].makeCell =
                () =>
                {
                    Button b = new Button();
                    b.style.alignSelf = Align.Center;
                    b.style.justifyContent = Justify.Center;
                    return b;
                };
            view.columns["blueprint"].bindCell =
                (el, i) =>
                {
                    Button button = el.Q<Button>();
                    button.text = "Manage";
                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (building)
                    {
                        if (building.blueprint.itemList == null ||
                            building.blueprint.itemList.Count == 0 ||
                            (building is not Pipe &&
                                (building.blueprint.itemList.Count(q => q.itemType == GridItemType.Anchor) == 0 ||
                                 building.blueprint.itemList.Count(q => q.itemType == GridItemType.Entrance) == 0))
                            || (building is Pipe && building.blueprint.itemList.Count(q => q.itemType == GridItemType.Pipe) == 0))
                        {
                            button.style.color = Color.red;
                        }
                        else
                        {
                            button.style.color = Color.white;
                        }

                        button.RegisterCallback<ClickEvent>(actions.BlueprintEvent);
                        button.SetEnabled(true);
                    }
                    else
                    {
                        button.SetEnabled(false);
                    }
                };
            view.columns["blueprint"].unbindCell =
                (el, i) =>
                {
                    Button button = el.Q<Button>();
                    button.UnregisterCallback<ClickEvent>(actions.BlueprintEvent);
                };
            #endregion

            #region Preview
            view.columns.Add(new()
            {
                title = "Preview",
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    el.RegisterCallback<ClickEvent>(actions.PreviewClick);
                    el.style.backgroundImage = new StyleBackground(((BuildingWrapper)view.itemsSource[i]).preview);

                    el.style.width = 50;
                    el.style.height = 50;
                },
                unbindCell = (el, i) =>
                {
                    el.UnregisterCallback<ClickEvent>(actions.PreviewClick);
                },
                width = 50,
                resizable = false
            });
            #endregion

            #region
            view.columns.Add(new()
            {
                title = "Modifications",
                name = "modifications",
                makeCell = 
                    () => new ModificationCell(),
                bindCell =
                    (el, i) =>
                    {
                        ModificationCell cell = el as ModificationCell;

                        SerializedProperty wrapper = register.categoryObjects.GetArrayElementAtIndex(i);
                        cell.Open(wrapper);
                    },
                stretchable = true,
                
                    
            });
            #endregion
        }
    }
}
