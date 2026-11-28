using Assets.Scripts.Editor.Research.Windows;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DataGridWindow<CATEG_TYPE, DATA_TYPE> : CategoryWindow<CATEG_TYPE, DATA_TYPE>
    where CATEG_TYPE : DataCategory<DATA_TYPE>
    where DATA_TYPE : DataObject
{
    protected MultiColumnListView dataGrid;


    protected override void CreateGUI()
    {
        base.CreateGUI();

        dataGrid = rootVisualElement.Q<MultiColumnListView>("Data");
        dataGrid.onAdd = (el) => AddEntry(el);
        dataGrid.onRemove = RemoveEntry;
        CreateColumns();
    }

    #region Reload
    protected override bool LoadCategData(int index)
    {
        bool boo = base.LoadCategData(index);
        if (boo)
        {
            dataGrid.style.display = DisplayStyle.Flex;
            dataGrid.BindProperty(serializedEntries);//.itemsSource = SelectedCategory.Objects;
        }
        else
        {
            dataGrid.Unbind();
            dataGrid.style.display = DisplayStyle.None;
        }
        return boo;
    }
    #endregion


    #region Entry managment
    protected virtual void AddEntry(BaseListView _)
    {
        int index = serializedEntries.arraySize;
        serializedEntries.InsertArrayElementAtIndex(index);
        var param = serializedEntries.GetArrayElementAtIndex(index);
        param.managedReferenceValue = Activator.CreateInstance<DATA_TYPE>();
        param.FindPropertyRelative(nameof(DataObject.id)).intValue = Holder.UniqueID();

        serializedHolder.ApplyModifiedProperties();
        dataGrid.RefreshItems();
    }
    protected void RemoveEntry(BaseListView _) => RemoveEntry(_.selectedIndex);
    protected virtual void RemoveEntry(int i)
    {
        serializedEntries.DeleteArrayElementAtIndex(i);
        serializedEntries.serializedObject.ApplyModifiedProperties();
        LoadCategData(categorySelector.index);
    }
    #endregion


    #region Collumns
    /// <summary>Function for creating and binding the columns.</summary>
    protected virtual void CreateColumns()
    {
        #region Base
        #region ID
        dataGrid.columns.Insert(0, new()
        {
            title = "ID",
            width = 100,
            makeCell =
            () =>
            {
                Label l = new Label();
                l.AddToClassList("cell");
                return l;
            },
            bindCell =
            (el, i) => ((Label)el).text = SelectedCategory.Objects[i]?.id.ToString()
        });
        #endregion

        #region Name
        dataGrid.columns.Insert(1, new()
        {
            title = "Name",
            resizable = true,
            width = 200,
            makeCell = () => new TextField(),
            bindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.BindProperty(GetWrapperAt(i).FindPropertyRelative(nameof(DataObject.Name)));
            },
            unbindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.Unbind();
            }
        });
        #endregion
        #region Description
        dataGrid.columns.Insert(2, new()
        {
            name = "description",
            title = "Descr",
            width = 200,
            makeCell = () => new TextField() { multiline = true},
            bindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.BindProperty(GetWrapperAt(i).FindPropertyRelative(nameof(DataObject.description)));
            },
            unbindCell =
            (el, i) =>
            {
                TextField field = (TextField)el;
                field.Unbind();
            }
        });
        #endregion
        #endregion
    }
    #endregion
}