using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>The Blueprint editor window for mapping buildings to grid.</summary>
class BuildEditor : EditorWindow
{
    #region Variables
    /// <summary>Target blueprint.</summary>
    BuildingGrid actBuild;
    /// <summary>Bulding the modifing blueprint belongs to.</summary>
    static SerializedProperty blueprint;
    /// <summary>Map of the blueprint.</summary>
    GridItemType[] gridItemTypes;
    /// <summary>Can only be saved if there are any changes.</summary>
    bool canSave;

    int2 size;

    /// <summary>Number of optiuons in <see cref="GridItemType"/><summary>
    int maxItem = Enum.GetNames(typeof(GridItemType)).Length - 1;

    #endregion

    #region Opening
    /// <summary>
    /// Keeps just only one instance of this window, and fills it.
    /// </summary>
    /// <param name="_buildingWrapper">Building with the blueprint to modify.</param>
    public static void ShowWindow(SerializedProperty _buildingWrapper)
    {
        blueprint = _buildingWrapper.FindPropertyRelative(nameof(BuildingWrapper.blueprint));
        var v = GetWindow<BuildEditor>();
        v.maxSize = new(900, 900);
        v.minSize = new(600, 600);

        v.titleContent = new(_buildingWrapper.FindPropertyRelative(nameof(BuildingWrapper.Name)).stringValue);

        v.Load();
    }


    PropertyField meshField;
    PropertyField sizeField;

    void Load()
    {
        if (blueprint == null)
        {
            meshField.Unbind();
            sizeField.Unbind();
            size = new();
            return;
        }

        meshField.BindProperty(blueprint.FindPropertyRelative(nameof(BuildingGrid.mesh)));
        
        var sizeProp = blueprint.FindPropertyRelative(nameof(BuildingGrid.size));
        sizeField.BindProperty(sizeProp);
        sizeField.TrackPropertyValue(sizeProp, SizeChange);

        size = (int2)blueprint.FindPropertyRelative(nameof(BuildingGrid.size)).boxedValue;

        LoadGrid();

        RenderGrid();
    }

    void LoadGrid()
    {
        var itemArray = blueprint.FindPropertyRelative(nameof(BuildingGrid.itemArray));
        int length = itemArray.arraySize;
        gridItemTypes = new GridItemType[length];

        for (int i = 0; i < length; i++)
        {
            gridItemTypes[i] = (GridItemType)itemArray.GetArrayElementAtIndex(i).enumValueIndex;
        }
    }

    #endregion

    void CreateGUI()
    {
        rootVisualElement.Add(meshField = new());
        rootVisualElement.Add(sizeField = new());

        VisualElement fields = new()
        {
            style =
            {
                marginBottom = 15,
                marginTop = 15,
                height = 30,
                flexDirection = FlexDirection.Row
            }
        };
        rootVisualElement.Add(fields);
        var names = Enum.GetNames(typeof(GridItemType));

        for (int i = 0; i < names.Length; i++)
        {
            string nam = names[i];
            fields.Add(new Label(nam)
            {
                style =
                {
                    flexGrow = 1,
                    maxWidth = new Length(100f/names.Length, LengthUnit.Percent),
                    backgroundColor = ChangeColor((GridItemType)i),
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            });
        }
        rootVisualElement.Add(buttonContainer = new() { style = { flexDirection = FlexDirection.ColumnReverse, flexGrow = 1 } });
    }

    void SizeChange(SerializedProperty ev)
    {
        size = (int2)ev.boxedValue;
        int newSize = size.x * size.y;
        
        var itemArray = blueprint.FindPropertyRelative(nameof(BuildingGrid.itemArray));
        int length = itemArray.arraySize;
        itemArray.arraySize = newSize;

        for (int i = length; i < newSize; i++)
            itemArray.GetArrayElementAtIndex(i).enumValueIndex = 0;

        blueprint.serializedObject.ApplyModifiedProperties();


        gridItemTypes = new GridItemType[newSize];

        for (int i = 0; i < newSize; i++)
        {
            gridItemTypes[i] = (GridItemType)itemArray.GetArrayElementAtIndex(i).enumValueIndex;
        }
        

        RenderGrid();
    }


    VisualElement buttonContainer;

    void RenderGrid()
    {
        buttonContainer.Clear();
        int i = 0;

        for (int xa = 0; xa < size.y; xa++)
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
            for (int x = 0; x < size.x; x++)
            {
                Label label = new()
                {
                    userData = i,
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        flexGrow = 1,
                        maxWidth = new Length(100f/size.x, LengthUnit.Percent)
                    }
                };
                label.style.backgroundColor = ChangeColor(gridItemTypes[i]);
                label.text = gridItemTypes[i].ToString();
                label.RegisterCallback<PointerDownEvent>(Button_clicked);

                row.Add(label);
                i++;
            }
            buttonContainer.Add(row);
        }
    }

    void Button_clicked(PointerDownEvent ev)
    {
        Label label = (Label)ev.target;
        int i = (int)label.userData;

        if(ev.button == 0)
        {
            if ((int)gridItemTypes[i] == maxItem)
                gridItemTypes[i] = 0;
            else
                gridItemTypes[i]++;

        }
        else if(ev.button == 1) 
        {
            if (gridItemTypes[i] == 0)
                gridItemTypes[i] = (GridItemType)maxItem;
            else
                gridItemTypes[i]--;
        }

        label.style.backgroundColor = ChangeColor(gridItemTypes[i]);
        label.text = gridItemTypes[i].ToString();

        var en = blueprint.FindPropertyRelative(nameof(BuildingGrid.itemArray)).GetArrayElementAtIndex(i);
        en.enumValueIndex = (int)gridItemTypes[i];

        blueprint.serializedObject.ApplyModifiedProperties();
    }

    #region Buttons
    /// <summary>
    /// Colors the button by it's value.
    /// </summary>
    /// <param name="gridItem">Value of the button.</param>
    Color ChangeColor(GridItemType gridItem)
    {
        Color c;
        switch (gridItem)
        {
            case GridItemType.None:
                c = Color.black;
                break;
            case GridItemType.Road:
                c = Color.yellow;
                break;
            case GridItemType.Entrance:
                c = Color.white;
                break;
            case GridItemType.PassableRoad:
                c = Color.green;
                break;
            case GridItemType.Elevator:
                c = Color.red;
                break;
            case GridItemType.Pipe:
                c = Color.darkOrange;
                break;
            case GridItemType.WaterSource:
                c = Color.blue;
                break;
            case GridItemType.ResourceSource:
                c = Color.brown;
                break;
            default:
                c = Color.black;
                break;
        }

        return c / 2;
    }
    #endregion
}