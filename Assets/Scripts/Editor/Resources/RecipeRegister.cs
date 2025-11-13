using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

public class RecipeRegister : DataGridWindow<ProductionRecipeCategory, ProductionRecipe>
{
    static List<Type> types = TypeCache.GetTypesDerivedFrom(typeof(ProductionRecipe)).Prepend(typeof(ProductionRecipe)).ToList();
    static List<string> choices = types.Select(q => q.Name).ToList();

    [MenuItem("Custom Editors/Recipe register %u", priority = 14)]
    public static void Open()
    {
        RecipeRegister wnd = GetWindow<RecipeRegister>();
        wnd.titleContent = new("Recipe Register");
        wnd.minSize = new(800, 400);
    }
    protected override void CreateGUI()
    {
        holder = AssetDatabase.LoadAssetAtPath<ProductionRecipeHolder>(ProductionRecipeHolder.PATH);
        base.CreateGUI();
        rootVisualElement.Q<Button>("Rebind-Create").clicked += async () => await ResFluidTypes.Init();
        categorySelector.index = 0;
    }
    protected override void CreateColumns()
    {
        base.CreateColumns();
        dataGrid.columns.Add(new()
        {
            name = "time",
            title = "Time (in ticks)",
            minWidth = 80,
            makeCell = () =>
            {
                return new IntegerField();
            },
            bindCell = (el, i) =>
            {
                IntegerField field = (IntegerField)el;
                field.value = selectedCategory.Objects[i].timeInTicks;
                field.RegisterValueChangedCallback(TimeChange);
            },
            unbindCell = (el, i) =>
            {
                ((IntegerField)el).UnregisterValueChangedCallback(TimeChange);
            }
        });
        dataGrid.columns.Add(new()
        {
            name = "cost",
            title = "Cost",
            minWidth = 250,
            makeCell = () =>
            {
                return new ResourceCell() { allowedCategories = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 } };
            },
            bindCell = (el, i) =>
            {
                ResourceCell cell = (ResourceCell)el;
                cell.Open(selectedCategory.Objects[i].resourceCost, holder, true);
            }
        });

        dataGrid.columns.Add(new()
        {
            name = "yield",
            title = "Yield",
            minWidth = 250,
            makeCell = () =>
            {
                return new ResourceCell() { allowedCategories = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 } };
            },
            bindCell = (el, i) =>
            {
                ResourceCell cell = (ResourceCell)el;
                cell.Open(selectedCategory.Objects[i].resourceYield, holder, false);
            }
        });
    }

    void TimeChange(ChangeEvent<int> ev)
    {
        int row = ev.target.GetRowIndex();
        selectedCategory.Objects[row].timeInTicks = ev.newValue;
        EditorUtility.SetDirty(holder);
    }

}

