using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Resource), true)]
public class ResourceEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        SerializedObject serializedObject = property.serializedObject;
        Color color = new Color(0.2f, 0.2f, 0.2f, 1);

        #region Root style
        VisualElement root = new();
        root.style.paddingTop = 5;
        root.style.paddingLeft = 5;
        root.style.paddingRight = 5;
        root.style.paddingBottom = 5;
        root.style.marginBottom = 5;

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
        root.style.borderTopRightRadius = 5;

        root.style.backgroundColor = color;
        #endregion

        #region Label
        string labelText = property.displayName.Contains("Element ")
            ? property.displayName.Replace("Element", property.propertyPath.Split('.')[^3])
            : property.displayName;

        labelText = char.ToUpper(labelText[0]) + labelText.Substring(1);

        Label l = new Label(labelText);
        l.style.fontSize = 18;
        root.Add(l);
        #endregion

        #region Capacity
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
        /*if (property.boxedValue is CapacityResource || property.boxedValue is MoneyResource)
        {
            Resource resource = property.boxedValue as Resource;
            bool money = resource is MoneyResource;
            IntegerField integerField = new IntegerField(money ? "Cost" : "Capacity");
            integerField.value = money ? +(resource as MoneyResource).Money : + (resource as CapacityResource).capacity;
            integerField.style.marginBottom = 5;
            integerField.style.width = new Length(98, LengthUnit.Percent);
            integerField.RegisterValueChangedCallback((i) =>
            {
                if (money)
                    ((MoneyResource)property.boxedValue).Money.BaseValue = i.newValue;
                else
                    ((CapacityResource)property.boxedValue).capacity.BaseValue = i.newValue;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            });
            root.Add(integerField);
        }*/
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type

        #endregion

        #region ListView
        ListView listView = new();
        listView.style.backgroundColor = new Color(0, 0, 0, 0);
        listView.showAddRemoveFooter = true;
        listView.allowAdd = true;
        listView.allowRemove = true;

        listView.itemTemplate = Resources.Load<VisualTreeAsset>("UI Toolkit\\ResElem");
        listView.itemsSource = ((Resource)property.boxedValue).types;
        listView.horizontalScrollingEnabled = false;
        #region Actions
        listView.bindItem =
            (elem, i) =>
            {
                elem = elem.ElementAt(0);
                ((EnumField)elem.ElementAt(0)).value = ((Resource)property.boxedValue).types[i];
                ((EnumField)elem.ElementAt(0)).RegisterValueChangedCallback(
                    (enu) =>
                    {
                        property.FindPropertyRelative(
                            nameof(Resource.types)).GetArrayElementAtIndex(i).enumValueIndex = (int)(ResourceType)enu.newValue;
                        serializedObject.ApplyModifiedProperties();
                    });

                ((IntegerField)elem.ElementAt(1)).value = ((Resource)property.boxedValue).ammounts[i];
                ((IntegerField)elem.ElementAt(1)).RegisterValueChangedCallback(
                    (val) =>
                    {
                        property.FindPropertyRelative(
                            nameof(Resource.ammounts)).GetArrayElementAtIndex(i).intValue = val.newValue;
                        serializedObject.ApplyModifiedProperties();
                    });
            };

        listView.onAdd =
            (view) =>
            {
                int i = listView.itemsSource.Count;
                property.FindPropertyRelative(nameof(Resource.types))
                    .InsertArrayElementAtIndex(i);
                property.FindPropertyRelative(nameof(Resource.ammounts))
                    .InsertArrayElementAtIndex(i);
                property.serializedObject.ApplyModifiedProperties();

                listView.itemsSource.Add(ResourceType.Coal);
            };

        listView.onRemove =
            (view) =>
            {
                int sel = view.selectedIndex;
                if (sel == -1)
                    sel = listView.itemsSource.Count - 1;

                property.FindPropertyRelative(nameof(Resource.types))
                    .DeleteArrayElementAtIndex(sel);
                property.FindPropertyRelative(nameof(Resource.ammounts))
                    .DeleteArrayElementAtIndex(sel);
                property.serializedObject.ApplyModifiedProperties();

                listView.itemsSource.RemoveAt(sel);
                listView.RefreshItems();
            };
        #endregion

        root.Add(listView);
        #endregion

        return root;
    }
}