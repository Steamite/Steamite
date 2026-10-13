using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Editor.Columns;
using Assets.Scripts.Editor.Shortcuts;
using EditorWindows.Windows;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.SpecialColumns
{
    public class SpecialColumns : ColumnCreators<SpecialActions>
    {
        BuildingRegister register;
        public SpecialColumns(MultiColumnListView _view, BuildingRegister _register) : base(_view)
        {
            register = _register;
        }

        public override void CreateColumns()
        {
            #region Assign Limit
            // Needs to have a field in the class, so it can be serialized.
            view.columns.Add(new()
            {
                name = "limit",
                title = "Assign",
                width = 75,
                resizable = false,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();

                    var building = register.GetBuildingWrapperAt(i).FindPropertyRelative("building");

                    if (building.GetPropertyType() is IAssign)
                    {
                        field.enabledSelf = true;
                        building
                            .FindPropertyRelative("assignData")
                            .FindPropertyRelative("AssignLimit")
                            .FindPropertyRelative("baseValue");
                    }
                    else
                        field.enabledSelf = false;
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    el.Unbind();
                }
            });
            #endregion

            #region Production

            ProductionRecipeHolder holder = AssetDatabase.LoadAssetAtPath<ProductionRecipeHolder>(ProductionRecipeHolder.PATH);
            view.columns.Add(new()
            {
                name = "recipes",
                title = "Recipes",
                width = 150,
                resizable = false,
                makeCell = () => new RecipeCell(),
                bindCell = (el, i) =>
                {/*
                    var building = register.GetBuildingWrapperAt(i).FindPropertyRelative("building");

                    RecipeCell cell = el as RecipeCell;
                    
                    if (building.boxedValue is IResourceProduction production)
                    {
                        cell.userData = ((BuildingWrapper)view.itemsSource[i]).Building;
                        cell.Open(production);
                    }
                    else
                        cell.Open(null);*/
                },
                unbindCell = (el, i) =>
                {
                    RecipeCell cell = el as RecipeCell;
                }
            });
            #endregion

            #region Storing
            view.columns.Add(new()
            {
                name = "canStore",
                title = "Can store",
                resizable = true,
                width = 150,
                makeCell = () => new Mask64Field(),
                bindCell = (el, i) =>
                {
                    var building = register.GetBuildingWrapperAt(i).FindPropertyRelative("building");


                    Mask64Field field = (Mask64Field)el;
                    if (building.GetPropertyType() is IStorage storage)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetResNamesList();
                        field.choices = choices;

                        field.BindProperty(
                            register.GetBuildingWrapperAt(i)
                            .FindPropertyRelative("building")
                            .FindPropertyRelative("canStoreInt"));
                    }
                    /*else if (building is FluidTank tank)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetFluidNames();
                        field.choices = choices;

                        field.BindProperty(sO.FindProperty(nameof(FluidTank.TypesToStore)));
                    }*/
                    else
                    {
                        el.style.display = DisplayStyle.None;
                    }
                },
                unbindCell = (el, i) =>
                {
                    Mask64Field field = (Mask64Field)el;
                    field.Unbind();
                },
            });
            #endregion


            #region Mask
            view.columns.Add(new()
            {
                name = "CategMask",
                title = "Category Mask",
                resizable = true,
                width = 150,
                makeCell = () => new MaskField(),
                bindCell = (el, i) =>
                {
                    // TODO maybe move to the building instead of the wrapper?

                    /*MaskField field = (MaskField)el;
                    field.choices = Enum.GetNames(typeof(BuildingCategType)).ToList();
                    */
                    //field.BindProperty(build.FindPropertyRelative("buildingCategories"));
                },
                unbindCell = (el, i) =>
                {
                    MaskField field = (MaskField)el;
                    field.Unbind();
                },
            });
            #endregion

            #region Capacity
            view.columns.Add(new()
            {
                name = "Capacity",
                title = "Capacity",
                resizable = true,
                width = 75,
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    // TODO
                    /*
                    el.Clear();
                    BuildingNew building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (building == null)
                        return;

                    SerializedObject sO = new(building);
                    IntegerField field = new();
                    el.Add(field);

                    if (building is FluidTank tank)
                    {
                        field.BindProperty(
                            sO.FindProperty("storedFluid")
                            .FindPropertyRelative(nameof(CapacityResource.capacity))
                            .FindPropertyRelative("baseValue"));
                    }
                    else
                    {
                        field.BindProperty(
                            sO.FindProperty("localRes")
                            .FindPropertyRelative(nameof(CapacityResource.capacity))
                            .FindPropertyRelative("baseValue"));
                    }

                    if (building is FluidResProductionBuilding fluidRes)
                    {
                        field = new IntegerField();
                        el.Add(field);

                        field.BindProperty(
                            sO.FindProperty("storedFluids")
                            .FindPropertyRelative(nameof(CapacityResource.capacity))
                            .FindPropertyRelative("baseValue"));
                    }*/

                },
                unbindCell = (el, i) =>
                {
                    for (int j = 0; j < el.childCount; j++)
                    {
                        el[j].Unbind();
                    }
                    el.Clear();
                },
            });
            #endregion

            #region Range
            view.columns.Add(new()
            {
                name = "Range",
                title = "Range",
                resizable = true,
                width = 75,
                makeCell = () =>  new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el as IntegerField;
                    var building = register.GetBuildingWrapperAt(i).FindPropertyRelative("building");

                    if (building.GetPropertyType() is not IEffectObject effect)
                    {
                        field.enabledSelf = false;
                        return;
                    }

                    effect.Range ??= new(-1);
                    building
                        .FindPropertyRelative("range")
                        .FindPropertyRelative("baseValue");
                },
                unbindCell = (el, i) =>
                {
                    (el as IntegerField).Unbind();
                },
            });
            #endregion

            view.columns.Add(new()
            {
                name = "stability",
                title = "Stability",
                resizable = true,
                width = 75,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField intField = (IntegerField)el;

                    var building = register.GetBuildingWrapperAt(i).FindPropertyRelative("building");

                    if (building.GetPropertyType() is not IStabilitySupport)
                    {
                        intField.enabledSelf = false;
                        return;
                    }
                    intField.enabledSelf = true;

                    SerializedProperty prop = 
                        building.FindPropertyRelative(
                        nameof(IStabilitySupport.SupportValue)
                        .LowerCamelCase());

                    intField.BindProperty(prop);
                },
                unbindCell = (el, i) =>
                {
                    el[0].Unbind();
                },
            });
        }
    }
}
