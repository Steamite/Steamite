using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class RecipeCell : ListView, IUIElement
{
    Object data;
    List<DataAssign> list;
    ProductionRecipeHolder holder;
    public RecipeCell()
    {
        makeItem = () => new DropdownField();
        holder = AssetDatabase.LoadAssetAtPath<ProductionRecipeHolder>(ProductionRecipeHolder.PATH);
        bindItem = (el, i) =>
        {
            DropdownField field = el as DropdownField;
            field.choices = holder.ObjectChoices(true);
            ProductionRecipe recipe = holder.GetObjectBySaveIndex((DataAssign)itemsSource[i]);
            if (recipe != null)
                field.value = recipe.Name;
            else
                field.value = "None";

            field.RegisterValueChangedCallback(OnChange);
        };
        onAdd = (_list) =>
        {
            list.Add(new DataAssign(-1, -1));
            EditorUtility.SetDirty(data as Object);
        };
        allowAdd = true;
        allowRemove = true;
        showAddRemoveFooter = true;
    }


    private void OnChange(ChangeEvent<string> ev)
    {
        int i = ev.target.GetRowIndex(false);
        if (ev.newValue == "None")
            list[i] = new(-1, -1);
        else
            list[i] = holder.GetSaveIndexByName(ev.newValue);
        EditorUtility.SetDirty(userData as Object);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">needs to be List(DataAssign)</param>
    public void Open(object data)
    {
        if (data == null)
        {
            style.display = DisplayStyle.None;
        }
        else
        {
            style.display = DisplayStyle.Flex;
            list = data as List<DataAssign>;
            itemsSource = list;
        }
    }
}
