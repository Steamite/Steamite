using System.Collections.Generic;
using System.Linq;
using TradeData.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(StatData))]
public class ColonyStatEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        SerializedObject serializedObject = property.serializedObject;
        List<ColonyStat> stats = Resources.LoadAll<ColonyStat>("Holders/Data/Stats").ToList();
        List<ColonyStat> prods = Resources.LoadAll<ColonyStat>("Holders/Data/Prods").ToList();
        StatData config = ((StatData)property.boxedValue);
        Resize(stats.Count, config.stats, serializedObject);
        Resize(prods.Count, config.production, serializedObject);

        #region Root
        VisualElement root = new();
        root.style.paddingTop = 5;
        root.style.paddingLeft = 5;
        root.style.paddingRight = 5;
        root.style.paddingBottom = 5;
        root.style.marginBottom = 5;
        /*
        root.style.borderTopWidth = 1;
        root.style.borderBottomWidth = 1;
        root.style.borderLeftWidth = 1;
        root.style.borderRightWidth = 1;

        root.style.borderTopColor = Color.black;
        root.style.borderBottomColor = Color.black;
        root.style.borderLeftColor = Color.black;
        root.style.borderRightColor = Color.black;

        root.style.borderBottomLeftRadius = 5;
        root.style.borderBottomRightRadius = 5;
        root.style.borderTopLeftRadius = 5;
        root.style.borderTopRightRadius = 5;*/

        root.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);

        Label l = new Label(property.displayName);
        l.style.fontSize = 18;
        root.Add(l);
        #endregion

        CreateList(
            root,
            nameof(StatData.stats),
            ((StatData)property.boxedValue).stats,
            stats,
            property);
        CreateList(
            root,
            nameof(StatData.production),
            ((StatData)property.boxedValue).production,
            prods,
            property);
        return root;
    }

    ListView CreateList(VisualElement root, string name, List<MinMax> config, List<ColonyStat> stats, SerializedProperty property)
    {
        ListView list = new();
        root.Add(new Label(name));
        root.Add(list);

        list.style.backgroundColor = new Color(0, 0, 0, 0);
        list.style.marginBottom = 5;

        list.showAddRemoveFooter = false;
        list.selectionType = SelectionType.None;

        list.itemTemplate = Resources.Load<VisualTreeAsset>("UI Toolkit/StatElem");
        list.itemsSource = config;
        list.fixedItemHeight = 40;
        list.horizontalScrollingEnabled = false;

        list.bindItem =
            (elem, i) =>
            {
                elem = elem[0];
                ((Label)elem[0]).text = stats[i].name;
                elem = elem[1];
                ((IntegerField)elem[0][1]).value = config[i].min;
                ((IntegerField)elem[0][1]).RegisterValueChangedCallback(
                    (minValue) =>
                    {
                        MinMax max = ((MinMax)property.FindPropertyRelative(name).GetArrayElementAtIndex(i).boxedValue);
                        max.min = Mathf.Clamp(minValue.newValue, 0, config[i].max);
                        config[i].min = max.min;
                        ((IntegerField)elem[0][1]).value = max.min;

                        property.FindPropertyRelative(name).GetArrayElementAtIndex(i).boxedValue = max;
                        property.serializedObject.ApplyModifiedProperties();
                    });

                ((IntegerField)elem[1][1]).value = config[i].max;
                ((IntegerField)elem[1][1]).RegisterValueChangedCallback(
                    (maxValue) =>
                    {
                        MinMax max = ((MinMax)property.FindPropertyRelative(name).GetArrayElementAtIndex(i).boxedValue);
                        max.max = Mathf.Clamp(maxValue.newValue, config[i].min, 5);
                        config[i].max = max.max;
                        ((IntegerField)elem[1][1]).value = max.max;

                        property.FindPropertyRelative(name).GetArrayElementAtIndex(i).boxedValue = max;
                        property.serializedObject.ApplyModifiedProperties();
                    });
            };
        return list;
    }

    void Resize(int sourceCount, List<MinMax> stats, SerializedObject serializedObject)
    {
        if (stats.Count != sourceCount)
        {
            while (stats.Count < sourceCount)
                stats.Add(new());
            while (stats.Count > sourceCount)
                stats.RemoveAt(stats.Count - 1);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
