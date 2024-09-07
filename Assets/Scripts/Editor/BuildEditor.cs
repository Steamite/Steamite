using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UIElements;
using System;
using UnityEditor.Graphs;

#if UNITY_EDITOR
class BuildEditor : EditorWindow
{
    static BuildingGrid actBuild;
    static SerializedProperty property;
    static GridItemType[,] GridItemTypes;
    static bool canSave;
    static bool hasAnchor;
    static int width;
    static int height;
    static int maxItem = Enum.GetNames(typeof(GridItemType)).Length;

    public static void ShowWindow(BuildingGrid buildingGrid, SerializedProperty _property)
    {
        canSave = false;
        hasAnchor = false;
        actBuild = buildingGrid;
        property = _property;
        GridPos pos;
        if (actBuild.itemList != null && actBuild.itemList.Count > 0)
        {
            width = (int)actBuild.size.x;
            height = (int)actBuild.size.z;
            if(width > 0 && height > 0)
                GridItemTypes = new GridItemType[width, height];
            for (int i = 0; i < actBuild.itemList.Count; i++)
            {
                pos = actBuild.itemList[i].pos;
                GridItemTypes[(int)(pos.x + actBuild.anchor.x), (int)(pos.z + actBuild.anchor.z)] = actBuild.itemList[i].itemType;
                if (actBuild.itemList[i].itemType == GridItemType.Anchor)
                    hasAnchor = true;
            }
        }
        else
        {
            width = 0;
            height = 0;
            GridItemTypes = null;
        }

        GetWindow(typeof(BuildEditor));
    }
    void OnGUI()
    {
        this.maxSize = new(600, 600);
        this.minSize = new(200, 200);
        
        if(actBuild == null)
        {
            GUILayout.Label(new GUIContent("NO BUILDING SELECTED"));
        }
        else
        {
            int item;
            width = EditorGUILayout.IntField(new GUIContent("size x: "), width);
            height = EditorGUILayout.IntField(new GUIContent("size y: "), height);

            if(width > 0 & height > 0)
            {
                ManageGrid();
                GUI.enabled = canSave;
                if (canSave)
                    GUI.backgroundColor = Color.red;

                if (GUILayout.Button(new GUIContent("Save")))
                {
                    GridItemType actItem;
                    Vector2 center = new();
                    int centerChild = 0;
                    Vector2 anchor = new();
                    actBuild.itemList = new();

                    for(int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            actItem = GridItemTypes[i, j];
                            if (actItem == GridItemType.None)
                                continue;
                            actBuild.itemList.Add(new(new(i,j), actItem));
                            if(actItem == GridItemType.Road || actItem == GridItemType.Anchor)
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
                    foreach(NeededGridItem elem in actBuild.itemList)
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
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.gray;
                GUILayout.BeginHorizontal();
                for (int i = 0; i < width; i++)
                {
                    GUILayout.BeginVertical();
                    for (int j = 0; j < height; j++)
                    {
                        ChangeColor(GridItemTypes[i, j]);
                        if (GUILayout.Button(new GUIContent(GridItemTypes[i,j].ToString()),GUILayout.MaxWidth(position.width / width)))
                        {
                            if(Event.current.button == 0)
                            {
                                item = (int)GridItemTypes[i, j] + 1;
                                if (item == maxItem)
                                {
                                    hasAnchor = false;
                                    item = 0;
                                }
                                else if(item == maxItem - 1)
                                {
                                    if (hasAnchor)
                                        item = 0;
                                    else
                                        hasAnchor = true;
                                }
                                GridItemTypes[i, j] = (GridItemType)item;
                            }
                            else if (Event.current.button == 1)
                            {
                                item = (int)GridItemTypes[i, j]-1;
                                if (item == -1)
                                {
                                    if(hasAnchor)
                                        item = maxItem - 2;
                                    else
                                    {
                                        item = maxItem - 1;
                                        hasAnchor = true;
                                    }
                                }
                                else if(item == maxItem - 2)
                                    hasAnchor = false;
                                GridItemTypes[i, j] = (GridItemType)item;
                            }
                            else if (Event.current.button == 2)
                            {
                                if (GridItemTypes[i, j] == GridItemType.Anchor)
                                    hasAnchor = false;
                                GridItemTypes[i, j] = GridItemType.None;
                            }
                            canSave = true;
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }
        // The actual window code goes here
    }

    void ManageGrid()
    {
        if (GridItemTypes == null)
        {
            canSave = true;
            GridItemTypes = new GridItemType[width, height];
        }
        else if (GridItemTypes.GetLength(0) != width || GridItemTypes.GetLength(1) != height)
        {
            canSave = true;
            ResizeGrid();
        }
    }

    void ResizeGrid()
    {
        GridItemType[,] tmpItems = new GridItemType[width, height];
        hasAnchor = false;
        for(int i = 0; i < width & i < GridItemTypes.GetLength(0); i++)
        {
            for (int j = 0; j < height & j < GridItemTypes.GetLength(1); j++)
            {
                tmpItems[i, j] = GridItemTypes[i, j];
                if (tmpItems[i, j] == GridItemType.Anchor)
                    hasAnchor = true;
            }
        }
        GridItemTypes = tmpItems;
    }

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
}
#endif