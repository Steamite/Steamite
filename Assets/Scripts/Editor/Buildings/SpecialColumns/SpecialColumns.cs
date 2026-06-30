using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.SpecialColumns
{
    public class SpecialColumns
    {
        SpecialActions actions;
        MultiColumnListView dataGrid;
        public SpecialColumns(MultiColumnListView _view)
        {
            dataGrid = _view;
            actions = new(dataGrid);
        }

        public void MakeColumns()
        {
            #region Assign Limit
            // Needs to have a field in the class, so it can be serialized.
            dataGrid.columns.Add(new()
            {
                name = "limit",
                title = "Assign",
                width = 75,
                resizable = false,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    Building buildingWrapper = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (buildingWrapper is IAssign)
                    {
                        if (((IAssign)buildingWrapper).AssignLimit == null)
                        {
                            ((IAssign)buildingWrapper).AssignLimit = new();
                            EditorUtility.SetDirty(buildingWrapper);
                        }
                        field.value = ((IAssign)buildingWrapper).AssignLimit.BaseValue;
                        field.SetEnabled(true);
                        field.RegisterValueChangedCallback(actions.AssignChange);
                    }
                    else
                    {
                        field.SetEnabled(false);
                    }
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    field.UnregisterValueChangedCallback(actions.AssignChange);
                }
            });
            #endregion

            #region Production

            ProductionRecipeHolder holder = AssetDatabase.LoadAssetAtPath<ProductionRecipeHolder>(ProductionRecipeHolder.PATH);
            dataGrid.columns.Add(new()
            {
                name = "recipes",
                title = "Recipes",
                width = 150,
                resizable = false,
                makeCell = () => new RecipeCell(),
                bindCell = (el, i) =>
                {
                    RecipeCell cell = el as RecipeCell;
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (building is IResourceProduction production)
                    {
                        cell.userData = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                        cell.Open(production);
                    }
                    else
                        cell.Open(null);
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    field.UnregisterValueChangedCallback(actions.ProdTimeChange);
                }
            });
            /*
            #region Time
            dataGrid.columns.Add(new()
            {
                name = "prodTime",
                title = "Prod. time",
                width = 75,
                resizable = false,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    if (((BuildingWrapper)dataGrid.itemsSource[i]).building is IProduction)
                    {
                        field.SetEnabled(true);
                        field.value = Convert.ToInt32(((IProduction)((BuildingWrapper)dataGrid.itemsSource[i]).building).ProdTime);
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

            dataGrid.columns.Add(new()
            {
                name = "prodCost",
                title = "Input",
                minWidth = dataGrid.columns["cost"].minWidth,
                maxWidth = dataGrid.columns["cost"].maxWidth,
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
                    Building b = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
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
            dataGrid.columns.Add(new()
            {
                name = "prod",
                title = "Yield",
                minWidth = dataGrid.columns["cost"].minWidth,
                maxWidth = dataGrid.columns["cost"].maxWidth,
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
                    Building b = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (b is IResourceProduction)
                        cell.Open(
                            ((IResourceProduction)b)?.ResourceYield.EditorResource,
                            ((BuildingWrapper)dataGrid.itemsSource[i]).building, false);
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
            dataGrid.columns.Add(new()
            {
                name = "canStore",
                title = "Can store",
                resizable = true,
                width = 150,
                makeCell = () => new Mask64Field(),
                bindCell = (el, i) =>
                {
                    Mask64Field field = (Mask64Field)el;
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (building is IStorage storage)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetResNamesList();
                        field.choices = choices;
                        field.value = storage.CanStoreMask;
                        field.RegisterValueChangedCallback(actions.CanStoreFluidsChange);
                    }
                    else if (building is FluidTank tank)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetFluidNames();
                        field.choices = choices;
                        field.value = tank.TypesToStore;
                        field.RegisterValueChangedCallback(actions.CanStoreFluidsChange);
                    }
                    else
                    {
                        el.style.display = DisplayStyle.None;
                    }
                },
                unbindCell = (el, i) =>
                {
                    Mask64Field field = (Mask64Field)el;
                    field.UnregisterValueChangedCallback(actions.CanStoreFluidsChange);
                },
            });
            #endregion

            #region Mask
            dataGrid.columns.Add(new()
            {
                name = "CategMask",
                title = "Category Mask",
                resizable = true,
                width = 150,
                makeCell = () => new MaskField(),
                bindCell = (el, i) =>
                {
                    MaskField field = (MaskField)el;
                    field.choices = Enum.GetNames(typeof(BuildingCategType)).ToList();
                    field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).building
                        ? ((BuildingWrapper)dataGrid.itemsSource[i]).building.BuildingCateg
                        : 0;
                    field.RegisterValueChangedCallback(actions.CategoryChange);
                },
                unbindCell = (el, i) =>
                {
                    MaskField field = (MaskField)el;
                    field.UnregisterValueChangedCallback(actions.CategoryChange);
                },
            });
            #endregion

            #region Capacity
            dataGrid.columns.Add(new()
            {
                name = "Capacity",
                title = "Capacity",
                resizable = true,
                width = 75,
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    el.Clear();
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;

                    IntegerField field = new();
                    if (building != null)
                    {
                        el.Add(field);
                        if (building is FluidTank tank)
                        {
                            if (tank.StoredFluids.capacity == null)
                                tank.StoredFluids.capacity = new(-1);
                            field.value = tank.StoredFluids.capacity.BaseValue;
                            field.RegisterValueChangedCallback(actions.FluidCapacityChanged);
                        }
                        else
                        {
                            if (building.LocalRes.capacity == null)
                                building.LocalRes.capacity = new(-1);
                            field.value = building.LocalRes.capacity.BaseValue;
                            field.RegisterValueChangedCallback(actions.StorageCapacityChanged);
                        }
                    }

                    if (building is FluidResProductionBuilding fluidRes)
                    {
                        field = new IntegerField();
                        if (fluidRes.StoredFluids.capacity == null)
                            fluidRes.StoredFluids.capacity = new(-1);
                        field.value = fluidRes.StoredFluids.capacity.BaseValue;

                        el.Add(field);
                        field.RegisterValueChangedCallback(actions.FluidCapacityChanged);
                    }

                },
                unbindCell = (el, i) =>
                {
                    try
                    {
                        (el[0] as IntegerField).UnregisterValueChangedCallback(actions.StorageCapacityChanged);
                        if (el.childCount > 1)
                            (el[1] as IntegerField).UnregisterValueChangedCallback(actions.FluidCapacityChanged);
                        el.Clear();
                    }
                    catch { }
                },
            });
            #endregion

            #region Range
            dataGrid.columns.Add(new()
            {
                name = "Range",
                title = "Range",
                resizable = true,
                width = 75,
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    el.Clear();
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (building == null || building is not IEffectObject effect)
                        return;

                    IntegerField field = new();
                    el.Add(field);

                    effect.Range ??= new(-1);
                    field.value = effect.Range.BaseValue;
                    field.RegisterValueChangedCallback(actions.RangeChange);
                },
                unbindCell = (el, i) =>
                {
                    if (el.childCount == 0)
                        return;

                    (el[0] as IntegerField)
                        .UnregisterValueChangedCallback(actions.RangeChange);
                    el.Clear();
                },
            });
            #endregion
        }

    }
}
