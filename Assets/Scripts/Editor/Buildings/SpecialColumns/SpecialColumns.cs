using Assets.Scripts.Editor.Columns;
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
        public SpecialColumns(MultiColumnListView _view) : base(_view)
        {
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
                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    field.SetEnabled(false);
                    if (building == null || building is not IAssign assign)
                        return;
                    SerializedObject sO = new(building);
                    field.BindProperty(
                        sO
                        .FindProperty("assignData")
                        .FindPropertyRelative("AssignLimit")
                        .FindPropertyRelative("baseValue"));
/*
                    if (((IAssign)building).AssignLimit == null)
                    {
                        ((IAssign)building).AssignLimit = new();
                        EditorUtility.SetDirty(building);
                    }
                    field.value = ((IAssign)building).AssignLimit.BaseValue;
                    field.RegisterValueChangedCallback(actions.AssignChange);*/
                    field.SetEnabled(true);
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    el.Unbind();
                    //field.UnregisterValueChangedCallback(actions.AssignChange);
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
                {
                    RecipeCell cell = el as RecipeCell;
                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (building is IResourceProduction production)
                    {
                        cell.userData = ((BuildingWrapper)view.itemsSource[i]).Building;
                        cell.Open(production);
                    }
                    else
                        cell.Open(null);
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                }
            });
            /*
            #region Time
            view.columns.Add(new()
            {
                name = "prodTime",
                title = "Prod. time",
                width = 75,
                resizable = false,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    if (((BuildingWrapper)view.itemsSource[i]).building is IProduction)
                    {
                        field.SetEnabled(true);
                        field.value = Convert.ToInt32(((IProduction)((BuildingWrapper)view.itemsSource[i]).building).ProdTime);
                        field.RegisterValueChangedCallback(ProdTimeChange);
                    }
                    else
                        field.SetEnabled(false);
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    field.UnregisterValueChangedCallback(ProdTimeChange);
                }
            });
            #endregion

            #region Input

            view.columns.Add(new()
            {
                name = "prodCost",
                title = "Input",
                minWidth = view.columns["cost"].minWidth,
                maxWidth = view.columns["cost"].maxWidth,
                resizable = true,

                makeCell = () =>
                {
                    VisualElement element = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    element.Add(new ResourceCell());
                    element.Add(new Label("Fluid:") { name = "fluidLabel"});
                    element.Add(new ResourceCell());
                    return element;
                },
                bindCell = (el, i) =>
                {
                    el.parent.focusable = true;
                    ResourceCell rCell = el[0] as ResourceCell;
                    Building b = ((BuildingWrapper)view.itemsSource[i]).building;
                    if (b is IResourceProduction resProd)
                        rCell.Open(
                            resProd.ResourceCost.EditorResource,
                            b, false);
                    else
                        rCell.Open(null, null, false);
                    
                    ResourceCell fCell = el[2] as ResourceCell;
                    if (b is FluidResProductionBuilding fluidProd)
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.Flex;
                        fCell.Open(
                            fluidProd.FluidCost,
                            b, false);
                    }
                    else
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.None;
                        fCell.Open(null, null, false);
                    }
                }
            });
            #endregion

            #region Yeild
            view.columns.Add(new()
            {
                name = "prod",
                title = "Yield",
                minWidth = view.columns["cost"].minWidth,
                maxWidth = view.columns["cost"].maxWidth,
                resizable = true,

                makeCell = () => 
                {
                    VisualElement element = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    element.Add(new ResourceCell());
                    element.Add(new Label("Fluid:") { name = "fluidLabel" });
                    element.Add(new ResourceCell());
                    return element;
                },
                bindCell = (el, i) =>
                {
                    el.parent.focusable = true;
                    ResourceCell cell = el[0] as ResourceCell;
                    Building b = ((BuildingWrapper)view.itemsSource[i]).building;
                    if (b is IResourceProduction)
                        cell.Open(
                            ((IResourceProduction)b)?.ResourceYield.EditorResource,
                            ((BuildingWrapper)view.itemsSource[i]).building, false);
                    else
                        cell.Open(null, null, false);

                    ResourceCell fCell = el[2] as ResourceCell;
                    if (b is FluidResProductionBuilding fluidProd)
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.Flex;
                        fCell.Open(
                            fluidProd.FluidYeild,
                            b, false);
                    }
                    else
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.None;
                        fCell.Open(null, null, false);
                    }
                }
            });
            
            #endregion
            */
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
                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (building == null)
                        return;


                    Mask64Field field = (Mask64Field)el;

                    SerializedObject sO = new(building);

                    if (building is IStorage storage)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetResNamesList();
                        field.choices = choices;

                        field.BindProperty(sO.FindProperty("canStoreInt"));
                    }
                    else if (building is FluidTank tank)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetFluidNames();
                        field.choices = choices;

                        field.BindProperty(sO.FindProperty(nameof(FluidTank.TypesToStore)));
                    }
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

                    Building b = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (b == null)
                        return;
                    SerializedObject build = new(b);

                    MaskField field = (MaskField)el;
                    field.choices = Enum.GetNames(typeof(BuildingCategType)).ToList();
                    field.BindProperty(build.FindProperty("buildingCategories"));
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
                    el.Clear();
                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
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
                    }

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
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    el.Clear();
                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (building == null || building is not IEffectObject effect)
                        return;

                    IntegerField field = new();
                    el.Add(field);
                    SerializedObject sO = new(building);
                    effect.Range ??= new(-1);
                    field.BindProperty(sO.FindProperty("range").FindPropertyRelative("baseValue"));
                },
                unbindCell = (el, i) =>
                {
                    if (el.childCount == 0)
                        return;
                    el[0].Unbind();
                    el.Clear();
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
                    intField.Unbind();
                    intField.style.display = DisplayStyle.None;

                    Building building = ((BuildingWrapper)view.itemsSource[i]).Building;
                    if (building is not IStabilitySupport)
                        return;

                    intField.style.display = DisplayStyle.Flex;

                    SerializedObject serializedObject = new(building);
                    SerializedProperty prop = serializedObject.FindProperty(
                        nameof(IStabilitySupport.SupportValue).LowerCamelCase());

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
