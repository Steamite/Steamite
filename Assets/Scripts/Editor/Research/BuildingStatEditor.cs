using BuildingStats;
using ResearchUI;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingStatEditor : DataGridWindow<BuildingStatCateg, Stat>
{
    /*enum NormalProp 
    {
        Value
    };
    enum ResourceProp
    {
        Resource,
        Capacity
    }
    readonly Dictionary<StatModifiers, Type> modifiers = new Dictionary<StatModifiers, Type>() 
    {
        { StatModifiers.Nothing, typeof(NormalProp)},
        { StatModifiers.Cost, typeof(NormalProp)},
        { StatModifiers.AssignLimit, typeof(NormalProp)},
        { StatModifiers.ProdSpeed, typeof(NormalProp)},
        { StatModifiers.InputResource, typeof(NormalProp)},
        { StatModifiers.ProductionYield, typeof(NormalProp)},
        { }
    };*/

    ResearchData researchData;
    #region Opening
    /// <summary>Opens the window, if it's already opened close it.</summary>
    [MenuItem("Custom Editors/Building stat Editor %h", priority = 15)]
    public static void Open()
    {
        BuildingStatEditor wnd = GetWindow<BuildingStatEditor>();
        wnd.titleContent = new GUIContent("Stat Editor");

    }

    /// <summary>Fills the button style and recalculates head placement</summary>
    protected override void CreateGUI()
    {
        data = AssetDatabase.LoadAssetAtPath<StatData>("Assets/Game Data/Research && Building/Stats.asset");
        researchData = AssetDatabase.LoadAssetAtPath<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset");
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
            resizable = false,
            makeCell = () => new ListView(),
            bindCell = (el, i) =>
            {
                ListView listView = el as ListView;
                listView.allowAdd = true;
                listView.allowRemove = true;
                listView.reorderable = true;
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

                        MaskField field = new MaskField(Enum.GetNames(typeof(BuildingCategType)).ToList(), 0);
                        field.style.width = 350;

                        element.Add(field);
                        EnumField enumField = new EnumField(StatModifiers.Cost);
                        enumField.style.flexGrow = 1;
                        element.Add(enumField);

                        DropdownField dropdownField = new DropdownField();
                        //dropdownField.style.width = 200;
                        dropdownField.style.width = 0;
                        element.Add(dropdownField);

                        FloatField floatField = new FloatField();
                        floatField.style.width = 50;
                        element.Add(floatField);

                        Toggle toggle = new Toggle("%");
                        toggle.style.flexDirection = FlexDirection.RowReverse;
                        toggle.Q<Label>().style.paddingLeft = 10;
                        toggle.Q<Label>().style.minWidth = 40;
                        toggle.Q<Label>().style.maxWidth = 40;
                        element.Add(toggle);
                        return element;
                    };
                listView.bindItem =
                    (el, j) =>
                    {
                        StatPair pair = ((Stat)dataGrid.itemsSource[i]).pairs[j];
                        MaskField maskField = el[0] as MaskField;
                        maskField.value = pair.mask;
                        maskField.RegisterValueChangedCallback<int>(PairTypeChange);

                        EnumField enumField = el[1] as EnumField;
                        enumField.value = pair.mod;
                        enumField.SetEnabled(pair.mask != 0);
                        enumField.RegisterValueChangedCallback<Enum>(ModChange);

                        /*DropdownField dropdown = el[2] as DropdownField;
                        dropdown.choices = Enum.GetNames(modifiers[pair.mod]).ToList();
                        dropdown.value = Enum.GetName(modifiers[pair.mod], pair.underProp);
                        dropdown.SetEnabled(pair.mask != 0 && pair.mod > 0);
                        dropdown.RegisterValueChangedCallback<string>(UnderPropChange);*/


                        FloatField intField = el[3] as FloatField;
                        intField.value = pair.modAmmount;
                        intField.RegisterValueChangedCallback<float>(FloatChange);

                        Toggle toggle = el[4] as Toggle;
                        toggle.value = pair.percent;
                        toggle.RegisterValueChangedCallback<bool>(PercenageChange);
                    };
                listView.unbindItem =
                    (el, j) =>
                    {
                        ((MaskField)el[0]).UnregisterValueChangedCallback<int>(PairTypeChange);
                        ((EnumField)el[1]).UnregisterValueChangedCallback<Enum>(ModChange);
                        //((DropdownField)el[2]).UnregisterValueChangedCallback<string>(UnderPropChange);
                        ((FloatField)el[3]).UnregisterValueChangedCallback<float>(FloatChange);
                    };
            },
            unbindCell =
                (el, i) =>
                {
                    ((ListView)el).Clear();
                }
        });
        #endregion
    }


    Vector2Int GetRowSmall<T>(ChangeEvent<T> ev)
    {
        VisualElement el = (VisualElement)ev.target;
        int i = el.parent.parent.IndexOf(el.parent);

        ListView view = ToolkitUtils.GetParentOfType<ListView>(el);
        int j = GetRowIndex(view);
        return new(i, j);
    }

    void SaveStatChange(int index)
    {
        Stat stat = ((Stat)dataGrid.itemsSource[index]);
        dataGrid.RefreshItem(index);
        EditorUtility.SetDirty(data);
        researchData.Categories.SelectMany(q => q.Objects)
            .FirstOrDefault(q =>
                q.nodeType == NodeType.Stat &&
                q.nodeCategory == categIndex &&
                q.nodeAssignee == stat.id).GetDescr(stat);

    }

    #region Changes
    void PairTypeChange(ChangeEvent<int> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        StatPair pair = ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x];
        pair.mask = ev.newValue;
        ((VisualElement)ev.target).parent[1].SetEnabled(pair.mask != 0);
        ((VisualElement)ev.target).parent[2].SetEnabled(pair.mask != 0 && pair.mod > 0);

        SaveStatChange(pos.y);
    }

    void ModChange(ChangeEvent<Enum> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        StatPair pair = ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x];
        pair.mod = (StatModifiers)ev.newValue;
        ((VisualElement)ev.target).parent[2].SetEnabled(pair.mask != 0 && pair.mod > 0);

        SaveStatChange(pos.y);
    }


    /*void UnderPropChange(ChangeEvent<string> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        StatPair pair = ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x];
        pair.underProp = Enum.GetNames(modifiers[pair.mod]).ToList().IndexOf(ev.newValue);

        SaveStatChange(pos.y);
    }*/

    void FloatChange(ChangeEvent<float> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x].modAmmount = ev.newValue;

        SaveStatChange(pos.y);
    }

    void PercenageChange(ChangeEvent<bool> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x].percent = ev.newValue;

        SaveStatChange(pos.y);
    }

    #endregion
}