using Assets.Scripts.Editor.Buildings;
using BuildingStats;
using ResearchUI;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class StatRegister : DataGridWindow<BuildingStatCateg, Stat>
{
    ResearchData researchData;
    #region Opening
    /// <summary>Opens the window, if it's already opened close it.</summary>
    [MenuItem("Custom Editors/Building stat Registry %h", priority = 15)]
    public static void Open()
    {
        StatRegister wnd = GetWindow<StatRegister>();
        wnd.titleContent = new GUIContent("Stat Registry");

    }

    /// <summary>Fills the button style and recalculates head placement</summary>
    protected override void CreateGUI()
    {
        researchData = AssetDatabase.LoadAssetAtPath<ResearchData>(ResearchData.EDITOR_PATH);
        Holder = AssetDatabase.LoadAssetAtPath<StatData>(StatData.EDITOR_PATH);
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
            SelectedCategory = new BuildingStatCateg();
        }
        return boo;
    }
    #endregion


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
                        EditorUtility.SetDirty(Holder);
                    };
                listView.makeItem =
                    () =>
                    {
                        VisualElement element = new();
                        element.style.flexDirection = FlexDirection.Row;

                        MaskField field = new MaskField(Enum.GetNames(typeof(BuildingCategType)).ToList(), 0);
                        field.style.width = 350;
                        element.Add(field);

                        element.Add(new StatRow());
                        return element;
                    };
                listView.bindItem =
                    (el, j) =>
                    {
                        SerializedProperty statPair = categoryObjects
                            .GetArrayElementAtIndex(i)
                            .FindPropertyRelative(nameof(Stat.pairs))
                            .GetArrayElementAtIndex(j);

                        MaskField maskField = el[0] as MaskField;
                        SerializedProperty mask = statPair.FindPropertyRelative(nameof(StatPair.mask));

                        maskField.BindProperty(mask);
                        maskField.TrackPropertyValue(mask, (prop) => PairTypeChange(prop, el));

                        StatRow row = el[1] as StatRow;
                        SerializedProperty statValue = statPair.FindPropertyRelative(nameof(StatPair.statValue));

                        row.Open(statValue);
                    };
                listView.unbindItem =
                    (el, j) =>
                    {

                        ((MaskField)el[0]).Unbind();
                        // .UnregisterValueChangedCallback<int>(PairTypeChange);
                        ((EnumField)el[1]).Unbind();
                        //.UnregisterValueChangedCallback<Enum>(ModChange);
                        //((DropdownField)el[2]).UnregisterValueChangedCallback<string>(UnderPropChange);
                        //((FloatField)el[3]).UnregisterValueChangedCallback<float>(FloatChange);
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

    private void PairTypeChange(SerializedProperty property, VisualElement element)
    {
        int mask = property.intValue;
        element[1].SetEnabled(mask != 0);
    }

    #region Changes
    Vector2Int GetRowSmall<T>(ChangeEvent<T> ev)
    {
        VisualElement el = (VisualElement)ev.target;
        int i = el.parent.parent.IndexOf(el.parent);

        ListView view = el.GetParentOfType<ListView>();
        int j = view.GetRowIndex();
        return new(i, j);
    }

    void SaveStatChange(int index)
    {
        Stat stat = (Stat)dataGrid.itemsSource[index];
        dataGrid.RefreshItem(index);
        EditorUtility.SetDirty(Holder);
        researchData.Categories.SelectMany(q => q.Objects)
            .FirstOrDefault(q =>
                q.nodeType == NodeType.Stat &&
                q.objectConnection.categoryId == categIndex &&
                q.objectConnection.objectId == stat.id).GetDescr(stat);

    }

    /*
    void PairTypeChange(ChangeEvent<int> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        StatPair pair = ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x];
        pair.mask = ev.newValue;
        ((VisualElement)ev.target).parent[1].SetEnabled(pair.mask != 0);
        ((VisualElement)ev.target).parent[2].SetEnabled(pair.mask != 0 && pair.statValue.mod > 0);

        SaveStatChange(pos.y);
    }

    void ModChange(ChangeEvent<Enum> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        StatPair pair = ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x];
        pair.statValue.mod = (StatModifiers)ev.newValue;
        ((VisualElement)ev.target).parent[2].SetEnabled(pair.mask != 0 && pair.statValue.mod > 0);

        SaveStatChange(pos.y);
    }


    */
    /*void UnderPropChange(ChangeEvent<string> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        StatPair pair = ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x];
        pair.underProp = Enum.GetNames(modifiers[pair.mod]).ToList().IndexOf(ev.newValue);

        SaveStatChange(pos.y);
    }*/
    /*

    void FloatChange(ChangeEvent<float> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x].statValue.modAmmount = ev.newValue;

        SaveStatChange(pos.y);
    }

    void PercenageChange(ChangeEvent<bool> ev)
    {
        Vector2Int pos = GetRowSmall(ev);

        ((Stat)dataGrid.itemsSource[pos.y]).pairs[pos.x].statValue.percent = ev.newValue;

        SaveStatChange(pos.y);
    }
*/
    #endregion
}