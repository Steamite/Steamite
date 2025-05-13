using BuildingStats;
using EditorWindows;
using EditorWindows.Research;
using ResearchUI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;
using static UnityEditor.Progress;
using System.Collections.Generic;
using BottomBar.Building;

public class BuildingStatEditor : DataGridWindow<BuildingStatCateg, Stat>
{
    #region Opening
    /// <summary>Opens the window, if it's already opened close it.</summary>
    [MenuItem("Custom Editors/Stat Editor %h", priority = 15)]
    public static void Open()
    {
        BuildingStatEditor wnd = GetWindow<BuildingStatEditor>();
        wnd.titleContent = new GUIContent("Stat Editor");
    }

    /// <summary>Fills the button style and recalculates head placement</summary>
    protected override void CreateGUI()
    {
        data = AssetDatabase.LoadAssetAtPath<StatData>("Assets/Game Data/Research && Building/Stats.asset");
        base.CreateGUI();
        categorySelector.index = 0;
    }
    protected override bool LoadCategData(int index)
    {
        bool boo = base.LoadCategData(index);
        if (boo)
        {

        }
        else
        {
            selectedCategory = new BuildingStatCateg();
        }
        return boo;
    }
    #endregion


    protected override void AddEntry(BaseListView _)
    {
        selectedCategory.Objects.Add(new Stat(data.UniqueID()));            
        base.AddEntry(_);
    }

    protected override void CreateColumns()
    {
        base.CreateColumns();

        #region Mask
        dataGrid.columns.Add(new()
        {
            name = "Mask",
            title = "Mask",
            stretchable = true,
            resizable = true,
            makeCell = () => new ListView(),
            bindCell = (el, i) =>
            {
                ListView listView = el as ListView;
                listView.allowAdd = true;
                listView.allowRemove = true;
                listView.showAddRemoveFooter = true;

                listView.itemsSource = ((Stat)dataGrid.itemsSource[i]).pairs;
                listView.onAdd =
                    (el) =>
                    {
                        el.itemsSource.Add(new StatPair());
                        EditorUtility.SetDirty(data);
                    };
                listView.makeItem = 
                    () => 
                    {
                        VisualElement element = new();
                        element.style.flexDirection = FlexDirection.Row;
                        element.Add(new EnumField(BuildingCategType.Population));
                        element.Add(new EnumField(StatModifiers.AssignLimit));
                        element.Add(new FloatField() {});
                        return element;
                    };
                listView.bindItem = 
                    (el, j) => 
                    {
                        EnumField enumField = el[0] as EnumField;
                        enumField.value = ((Stat)dataGrid.itemsSource[i]).pairs[j].type;
                        enumField.RegisterValueChangedCallback<Enum>(PairTypeChange);

                        enumField = el[1] as EnumField;
                        enumField.value = ((Stat)dataGrid.itemsSource[i]).pairs[j].mod;
                        enumField.RegisterValueChangedCallback<Enum>(ModChange);

                        FloatField intField = el[2] as FloatField;
                        intField.value = ((Stat)dataGrid.itemsSource[i]).pairs[j].modAmmount;
                        intField.RegisterValueChangedCallback<float>(FloatChange);
                    };
                listView.unbindItem =
                    (el, j) =>
                    {
                        ((EnumField)el[0]).UnregisterValueChangedCallback<Enum>(PairTypeChange);
                        ((EnumField)el[1]).UnregisterValueChangedCallback<Enum>(ModChange);
                        ((FloatField)el[2]).UnregisterValueChangedCallback<float>(FloatChange);
                    };
            },
            unbindCell =
                (el, i) =>
                {
                    ((ListView)el).itemsSource = null;
                }
        });
        #endregion
    }

    void PairTypeChange(ChangeEvent<Enum> ev)
    {
        VisualElement el = (VisualElement)ev.target;
        int i = el.parent.parent.IndexOf(el.parent);

        ListView view = ToolkitUtils.GetParentOfType<ListView>(el);
        int j = GetRowIndex(view);

        List<StatPair> pairs = ((Stat)dataGrid.itemsSource[j]).pairs;
        pairs[i].type = (BuildingCategType)ev.newValue;
        for (int x = pairs.Count -1; x > -1; x--)
        {
            if(pairs.Count(q=> q.type == pairs[x].type) > 1)
            {
                pairs.RemoveAt(x);
            }
        }
        ((Stat)dataGrid.itemsSource[j]).pairs = pairs;
        dataGrid.RefreshItem(j);
        EditorUtility.SetDirty(data);
    }

    void ModChange(ChangeEvent<Enum> ev)
    {
        VisualElement el = (VisualElement)ev.target;
        int i = el.parent.parent.IndexOf(el.parent);

        ListView view = ToolkitUtils.GetParentOfType<ListView>(el);
        int j = GetRowIndex(view);

        ((Stat)dataGrid.itemsSource[j]).pairs[i].mod = (StatModifiers)ev.newValue;
        dataGrid.RefreshItem(j);
        EditorUtility.SetDirty(data);
    }

    void FloatChange(ChangeEvent<float> ev)
    {
        VisualElement el = (VisualElement)ev.target;
        int i = el.parent.parent.IndexOf(el.parent);

        ListView view = ToolkitUtils.GetParentOfType<ListView>(el);
        int j = GetRowIndex(view);

        ((Stat)dataGrid.itemsSource[j]).pairs[i].modAmmount = (float)ev.newValue;
        dataGrid.RefreshItem(j);
        EditorUtility.SetDirty(data);
    }
}