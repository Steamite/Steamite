#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using static UnityEditor.Progress;

/// <summary>The Blueprint editor window for mapping buildings to grid.</summary>
class BuildEditor : EditorWindow
{
    #region Variables
    /// <summary>Target blueprint.</summary>
    static BuildingGrid actBuild;
    /// <summary>Bulding the modifing blueprint belongs to.</summary>
    static Building inspectedBuilding;
    /// <summary>Map of the blueprint.</summary>
    static GridItemType[,] gridItemTypes;
    /// <summary>Can only be saved if there are any changes.</summary>
    static bool canSave;
    /// <summary>Each building can only have ONE anchor.</summary>
    static bool hasAnchor;
    /// <summary>Width of <see cref="gridItemTypes"/>.</summary>
    static int width;
    /// <summary>Height of <see cref="gridItemTypes"/>.</summary>
    static int height;
    /// <summary>Number of optiuons in <see cref="GridItemType"/><summary>
    static int maxItem = Enum.GetNames(typeof(GridItemType)).Length;
    #endregion

    #region Opening
    /// <summary>
    /// Keeps just only one instance of this window, and fills it.
    /// </summary>
    /// <param name="_building">Building with the blueprint to modify.</param>
    public static void ShowWindow(Building _building)
    {
        canSave = false;
        hasAnchor = false;
        actBuild = _building.blueprint;
        inspectedBuilding = _building;
        GridPos pos;
        if (actBuild.itemList != null && actBuild.itemList.Count > 0)
        {
            width = (int)actBuild.size.x;
            height = (int)actBuild.size.z;
            if(width > 0 && height > 0)
                gridItemTypes = new GridItemType[width, height];
            for (int i = 0; i < actBuild.itemList.Count; i++)
            {
                pos = actBuild.itemList[i].pos;
                gridItemTypes[(int)(pos.x + actBuild.anchor.x), (int)(pos.z + actBuild.anchor.z)] = actBuild.itemList[i].itemType;
                if (actBuild.itemList[i].itemType == GridItemType.Anchor)
                    hasAnchor = true;
            }
        }
        else
        {
            width = 0;
            height = 0;
            gridItemTypes = null;
        }

        var v = GetWindow(typeof(BuildEditor));
        v.maxSize = new(600, 600);
        v.minSize = new(200, 200);
    }
    #endregion


    void OnGUI()
    {
        if (actBuild == null)
        {
            GUILayout.Label(new GUIContent("NO BUILDING SELECTED"));
            titleContent = new($"Build Editor - None");
        }
        else
        {
            titleContent = new($"Build Editor - {inspectedBuilding.name}");
            int item = 0;
            width = EditorGUILayout.IntField(new GUIContent("size x: "), width);
            height = EditorGUILayout.IntField(new GUIContent("size y: "), height);

            if(width > 0 & height > 0)
            {
                ManageGrid();
                GUI.enabled = canSave;
                if (canSave)
                    GUI.backgroundColor = Color.red;

                SaveButton();
                GUI.enabled = true;
                GUI.backgroundColor = Color.gray;
                GUILayout.BeginHorizontal();
                for (int i = 0; i < width; i++)
                {
                    GUILayout.BeginVertical();
                    for (int j = 0; j < height; j++)
                    {
                        GridButton(i, j, item);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    /// <summary>
    /// Looks for changes in width and height and calls resizes the grid when needed.
    /// </summary>
    void ManageGrid()
    {
        if (gridItemTypes == null)
        {
            canSave = true;
            gridItemTypes = new GridItemType[width, height];
        }
        else if (gridItemTypes.GetLength(0) != width || gridItemTypes.GetLength(1) != height)
        {
            canSave = true;
            GridItemType[,] tmpItems = new GridItemType[width, height];
            hasAnchor = false;
            for (int i = 0; i < width & i < gridItemTypes.GetLength(0); i++)
            {
                for (int j = 0; j < height & j < gridItemTypes.GetLength(1); j++)
                {
                    tmpItems[i, j] = gridItemTypes[i, j];
                    if (tmpItems[i, j] == GridItemType.Anchor)
                        hasAnchor = true;
                }
            }
            gridItemTypes = tmpItems;
        }
    }

    #region Buttons
    /// <summary>
    /// Colors the button by it's value.
    /// </summary>
    /// <param name="gridItem">Value of the button.</param>
    void ChangeColor(GridItemType gridItem)
    {
        switch (gridItem)
        {
            case GridItemType.None:
                GUI.backgroundColor = Color.black;
                break;
            case GridItemType.Road:
                GUI.backgroundColor = Color.yellow;
                break;
            case GridItemType.Water:
                GUI.backgroundColor = Color.blue;
                break;
            case GridItemType.Entrance:
                GUI.backgroundColor = Color.white;
                break;
            case GridItemType.Anchor:
                GUI.backgroundColor = Color.green;
                break;
        }
    }

    /// <summary>
    /// Creates a button for saving.
    /// </summary>
    void SaveButton()
    {
        if (GUILayout.Button(new GUIContent("Save")))
        {
            GridItemType actItem;
            Vector2 center = new();
            int centerChild = 0;
            Vector2 anchor = new();
            actBuild.itemList = new();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    actItem = gridItemTypes[i, j];
                    if (actItem == GridItemType.None)
                        continue;
                    actBuild.itemList.Add(new(new(i, j), actItem));
                    if (actItem == GridItemType.Road || actItem == GridItemType.Anchor)
                    {
                        center += new Vector2(i, j);
                        centerChild++;
                        if (actItem == GridItemType.Anchor)
                        {
                            anchor = new(i, j);
                        }
                    }
                }
            }
            foreach (NeededGridItem elem in actBuild.itemList)
            {
                elem.pos.x -= anchor.x;
                elem.pos.z = elem.pos.z - anchor.y;
            }

            center = center / centerChild;

            actBuild.moveBy.x = center.x - anchor.x;
            actBuild.moveBy.z = anchor.y - center.y;
            actBuild.anchor = new(anchor.x, anchor.y);
            actBuild.size = new(width, height);
            canSave = false;
            EditorUtility.SetDirty(inspectedBuilding);
        }
    }

    /// <summary>
    /// Handles rendering and input from blueprint buttons.
    /// </summary>
    /// <param name="i">x cord</param>
    /// <param name="j">y cord</param>
    /// <param name="item">value</param>
    void GridButton(int i, int j, int item)
    {
        ChangeColor(gridItemTypes[i, j]);
        if (GUILayout.Button(new GUIContent(gridItemTypes[i, j].ToString()), GUILayout.MaxWidth(position.width / width)))
        {
            if (Event.current.button == 0)
            {
                item = (int)gridItemTypes[i, j] + 1;
                if (item == maxItem)
                {
                    hasAnchor = false;
                    item = 0;
                }
                else if (item == maxItem - 1)
                {
                    if (hasAnchor)
                        item = 0;
                    else
                        hasAnchor = true;
                }
                gridItemTypes[i, j] = (GridItemType)item;
            }
            else if (Event.current.button == 1)
            {
                item = (int)gridItemTypes[i, j] - 1;
                if (item == -1)
                {
                    if (hasAnchor)
                        item = maxItem - 2;
                    else
                    {
                        item = maxItem - 1;
                        hasAnchor = true;
                    }
                }
                else if (item == maxItem - 2)
                    hasAnchor = false;
                gridItemTypes[i, j] = (GridItemType)item;
            }
            else if (Event.current.button == 2)
            {
                if (gridItemTypes[i, j] == GridItemType.Anchor)
                    hasAnchor = false;
                gridItemTypes[i, j] = GridItemType.None;
            }
            canSave = true;
        }
    }
    #endregion
}
#endif