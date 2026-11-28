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
        dataGrid.columns["id"].makeCell =
            () =>
            {
                Label l = new Label();
                l.AddToClassList("cell");
                return l;
            };
        dataGrid.columns["id"].bindCell =
            (el, i) => ((Label)el).text = SelectedCategory.Objects[i]?.id.ToString();
        #endregion

        #region Name
        dataGrid.columns["name"].makeCell =
            () => new TextField();
        dataGrid.columns["name"].bindCell =
            (el, i) =>
            {
                TextField field = (TextField)el;
                if (((DATA_TYPE)dataGrid.itemsSource[i])?.Name != null)
                {
                    field.value = ((DATA_TYPE)dataGrid.itemsSource[i]).Name;
                    field.RegisterCallback<FocusOutEvent>(NameChange);
                }
                else
                    field.value = "";
            };
        dataGrid.columns["name"].unbindCell =
            (el, i) =>
            {
                TextField field = (TextField)el;
                field.UnregisterCallback<FocusOutEvent>(NameChange);
            };
        #endregion
        #region Description
        dataGrid.columns.Add(new()
        {
            name = "description",
            title = "Descr",
            makeCell = () => new TextField(),
            bindCell = (el, i) =>
            {
                TextField field = (TextField)el;
                field.value = ((DATA_TYPE)dataGrid.itemsSource[i]).description;
                field.RegisterCallback<FocusOutEvent>(DescrChange);
            },
            unbindCell =
            (el, i) =>
            {
                TextField field = (TextField)el;
                field.Unbind();
                field.UnregisterCallback<FocusOutEvent>(DescrChange);
            }

        });
        #endregion
        #endregion
    }

    /// <summary>Event for changing entry names.</summary>
    protected virtual void NameChange(FocusOutEvent ev)
    {
        string value;
        if (ev.target is TextElement)
        {
            TextElement field = (TextElement)ev.target;
            value = field.text.Trim(' ');
            field.text = value;
        }
        else
        {
            TextField field = (TextField)ev.target;
            value = field.value.Trim(' ');
            field.value = value;
        }

        int i = ((VisualElement)ev.target).GetRowIndex();
        if (((DATA_TYPE)dataGrid.itemsSource[i]).Name != value)
        {
            ((DATA_TYPE)dataGrid.itemsSource[i]).Name = value;
            EditorUtility.SetDirty(Holder);
        }
    }
    protected virtual void DescrChange(FocusOutEvent ev)
    {
        string value;
        if (ev.target is TextElement)
        {
            TextElement field = (TextElement)ev.target;
            value = field.text.Trim(' ');
            field.text = value;
        }
        else
        {
            TextField field = (TextField)ev.target;
            value = field.value.Trim(' ');
            field.value = value;
        }

        int i = ((VisualElement)ev.target).GetRowIndex();
        if (((DATA_TYPE)dataGrid.itemsSource[i]).description != value)
        {
            ((DATA_TYPE)dataGrid.itemsSource[i]).description = value;
            EditorUtility.SetDirty(Holder);
        }
    }
    #endregion
}