using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

public class QuestCompositorList<T> : MultiColumnListView where T : IQuestCompositor
{
    protected QuestHolder holder;
    protected Quest data;
    protected List<Type> types;

    public QuestCompositorList()
    {
        showAddRemoveFooter = true;
        allowAdd = true;
        allowRemove = true;

        columns.Add(new Column()
        {
            title = "Type",
            width = 300,
            resizable = false,
            stretchable = true,
            makeCell = () =>
            {
                return new DropdownField()
                {
                    name = "type",
                };
            },
            bindCell = (el, i) =>
            {
                DropdownField dropdownField = el as DropdownField;
                dropdownField.choices = types.Select(q => q.Name).ToList();
                dropdownField.SetValueWithoutNotify(itemsSource[i].GetType().Name);
                dropdownField.RegisterValueChangedCallback(TypeChange);
            },
            unbindCell = (el, i) =>
            {
                DropdownField dropdownField = el as DropdownField;
                dropdownField.UnregisterValueChangedCallback(TypeChange);
            }
        });
    }
    public virtual void Bind(QuestHolder _holder, Quest _data, List<Type> _types)
    {
        holder = _holder;
        data = _data;
        types = _types;
    }

    #region Changes
    void TypeChange(ChangeEvent<string> ev)
    {
        int i = ev.target.GetRowIndex();
        Type t = types.FirstOrDefault(q => q.Name == ev.newValue);
        if (t != null && data.GetType() != t)
        {
            itemsSource[i] = Activator.CreateInstance(t);
            EditorUtility.SetDirty(holder);
            RefreshItem(i);
        }
    }
    protected void ButtonClick(int i, VisualElement element)
    {
        EditorWindow win = EditorWindow.GetWindow(typeof(EmptyEditorWindow));
        win.rootVisualElement.Clear();
        win.rootVisualElement.Add(element);
    }
    #endregion
}
