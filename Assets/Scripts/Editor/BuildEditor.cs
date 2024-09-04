using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UIElements;
using System;

#if UNITY_EDITOR
class BuildEditor : EditorWindow
{
    static BuildingGrid actBuild;
    static NeededGridItem[,] neededGridItems;
    static bool canSave;
    static bool hasAnchor;
    static int width;
    static int height;
    static int maxItem = Enum.GetNames(typeof(NeededGridItem)).Length;

    public static void ShowWindow(BuildingGrid buildingGrid)
    {
        canSave = false;
        hasAnchor = false;
        actBuild = buildingGrid;

        if (actBuild.buildingGrid != null)
        {
            width = actBuild.buildingGrid.Length;
            if (width > 0 && actBuild.buildingGrid[0] != null)
            {
                height = actBuild.buildingGrid[0].values.Length;
                neededGridItems = new NeededGridItem[width, height];
            }
            for (int i = 0; i < actBuild.buildingGrid.Length; i++)
            {
                for(int j = 0; j < actBuild.buildingGrid[i].values.Length; j++)
                {
                    neededGridItems[i, j] = actBuild.buildingGrid[i].values[j];
                    if (neededGridItems[i, j] == NeededGridItem.Anchor)
                        hasAnchor = true;
                }
            }
        }
        else
        {
            width = 0;
            height = 0;
            neededGridItems = null;
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
                    NeededGridItem actItem;
                    Vector2Int min = new(int.MaxValue, int.MaxValue);
                    Vector2Int max = new(-1, -1);

                    Vector2 anchor = new();
                    actBuild.buildingGrid = new Wrapper<NeededGridItem>[width];
                    for(int i = 0; i < width; i++)
                    {
                        actBuild.buildingGrid[i] = new Wrapper<NeededGridItem>();
                        actBuild.buildingGrid[i].values = new NeededGridItem[height];
                        for (int j = 0; j < height; j++)
                        {
                            actItem = neededGridItems[i, j];
                            actBuild.buildingGrid[i].values[j] = actItem;
                            if(actItem == NeededGridItem.Road || actItem == NeededGridItem.Anchor)
                            {
                                if (i < min.x)
                                    min.x = i;
                                if (j < min.y)
                                    min.y = j;
                                if (i > max.x)
                                    max.x = i;
                                if (j > max.y)
                                    max.y = j;
                                if(actItem == NeededGridItem.Anchor)
                                {
                                    anchor = new(i, j);
                                }
                            }
                        }
                    }
                    actBuild.moveBy = new((float)(max.x - min.x) / 2, (float)(max.y - min.y) / 2);
                    actBuild.moveBy.x -= anchor.x;
                    actBuild.moveBy.z = anchor.y - actBuild.moveBy.z;
                    canSave = false;
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.gray;
                GUILayout.BeginHorizontal();
                for (int i = 0; i < width; i++)
                {
                    GUILayout.BeginVertical();
                    for (int j = 0; j < height; j++)
                    {
                        ChangeColor(neededGridItems[i, j]);
                        if (GUILayout.Button(new GUIContent(neededGridItems[i,j].ToString()),GUILayout.MaxWidth(position.width / width)))
                        {
                            if(Event.current.button == 0)
                            {
                                item = (int)neededGridItems[i, j] + 1;
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
                                neededGridItems[i, j] = (NeededGridItem)item;
                            }
                            else if (Event.current.button == 1)
                            {
                                item = (int)neededGridItems[i, j]-1;
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
                                neededGridItems[i, j] = (NeededGridItem)item;
                            }
                            else if (Event.current.button == 2)
                            {
                                if (neededGridItems[i, j] == NeededGridItem.Anchor)
                                    hasAnchor = false;
                                neededGridItems[i, j] = NeededGridItem.None;
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
        if (neededGridItems == null)
        {
            canSave = true;
            neededGridItems = new NeededGridItem[width, height];
        }
        else if (neededGridItems.GetLength(0) != width || neededGridItems.GetLength(1) != height)
        {
            canSave = true;
            ResizeGrid();
        }
    }

    void ResizeGrid()
    {
        NeededGridItem[,] tmpItems = new NeededGridItem[width, height];
        hasAnchor = false;
        for(int i = 0; i < width & i < neededGridItems.GetLength(0); i++)
        {
            for (int j = 0; j < height & j < neededGridItems.GetLength(1); j++)
            {
                tmpItems[i, j] = neededGridItems[i, j];
                if (tmpItems[i, j] == NeededGridItem.Anchor)
                    hasAnchor = true;
            }
        }
        neededGridItems = tmpItems;
    }

    void ChangeColor(NeededGridItem gridItem)
    {
        switch (gridItem)
        {
            case NeededGridItem.None:
                GUI.backgroundColor = Color.black;
                break;
            case NeededGridItem.Road:
                GUI.backgroundColor = Color.yellow;
                break;
            case NeededGridItem.Water:
                GUI.backgroundColor = Color.blue;
                break;
            case NeededGridItem.Entrance:
                GUI.backgroundColor = Color.white;
                break;
            case NeededGridItem.Anchor:
                GUI.backgroundColor = Color.green;
                break;
        }
    }
}
#endif