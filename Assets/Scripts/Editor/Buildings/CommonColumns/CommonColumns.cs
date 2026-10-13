using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
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
            view.columns["name"].makeCell = () =>
            {
                return new TextField();
            };
            view.columns["name"].bindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.BindProperty(register.GetBuildingWrapperAt(i).FindPropertyRelative(nameof(BuildingWrapper.Name)));
            };
            view.columns["name"].unbindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.Unbind();
            };

            view.columns["description"].makeCell = () =>
            {
                return new TextField() { multiline = true, };
            };
            view.columns["description"].bindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.BindProperty(register.GetBuildingWrapperAt(i).FindPropertyRelative(nameof(BuildingWrapper.description)));
            };
            view.columns["description"].unbindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.Unbind();
            };


            #region Type
            view.columns["type"].makeCell =
                () => new DropdownField();
            view.columns["type"].bindCell =
                (el, i) =>
                {
                    DropdownField field = (DropdownField)el;
                    field.choices = register.BuildingTypes.Select(q => q.Name).ToList();//.Select(q => q.Name).Where(q => !q.Contains("Pipe")).ToList();

                    // register.GetBuildingWrapperAt(i).FindPropertyRelative("building").type
                    field.value = register.GetBuildingWrapperAt(i).FindPropertyRelative("building").type;//((BuildingWrapper)view.itemsSource[i]).Building.GetType().ToString();
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
                    cell.Open(register.GetBuildingWrapperAt(i).FindPropertyRelative(nameof(BuildingWrapper.Cost)));
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
                    button.RegisterCallback<ClickEvent>(actions.BlueprintEvent);

                    /*if (building)
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

                        
                        button.SetEnabled(true);
                    }
                    else
                    {
                        button.SetEnabled(false);
                    }*/
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
                    //el.RegisterCallback<ClickEvent>(actions.PreviewClick);
/*
                    el.style.backgroundImage =
                        new StyleBackground((Sprite)register.GetBuildingWrapperAt(i).FindPropertyRelative(nameof(BuildingWrapper.preview)).boxedValue);
*/
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
