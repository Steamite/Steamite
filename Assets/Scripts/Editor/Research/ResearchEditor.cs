using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Overlays;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
public class ResearchEditor : EditorWindow
{
    ResearchData researchData;
    ResearchNode destroyMarked;
    string categName;
    const float nodeWidth = 175;
    const float nodeHeight = 100;
    const float nodeSpace = nodeWidth + 75;

    static bool selectedDestroy;
    static Vector2 scroll = new();
    static int selTab = 0;
    static int seconds = 0;
    Texture2D point;
    Texture2D circle;

    Vector2 startPoint;

    [MenuItem("Custom Editors/Research Editor", priority = 15)]
    public static void ShowWindow()
    {
        ResearchEditor window = GetWindow(typeof(ResearchEditor)) as ResearchEditor;
        window.titleContent = new("Research Editor");
        window.maximized = true;
        window.point = (Texture2D)Resources.Load("Textures/Point");
        window.circle = (Texture2D)Resources.Load("Textures/Circle");
        window.researchData = (ResearchData)Resources.Load("Holders/Data/ResearchData");
        window.researchData.Init();
    }

    private void OnGUI()
    {
        GUI.contentColor = Color.white;
        if (!researchData)
        {
            point = (Texture2D)Resources.Load("Textures/Point");
            circle = (Texture2D)Resources.Load("Textures/Circle");
            researchData = (ResearchData)Resources.Load("Holders/Data/ResearchData");
            researchData.Init();
        }

        if (GUI.Button(new Rect(10, 10, 50, 50), circle))
            Debug.Log("Clicked the button with an image");

        if (GUI.Button(new Rect(10, 70, 50, 30), "Click"))
            Debug.Log("Clicked the button with text");
        return;
        SwitchCategory();
    }

    void SwitchCategory()
    {
        string[] tabs = researchData.categories.Select(q => $"{q.categName} ({q.nodes.Count})").Append("create new").ToArray();
        selTab = EditorGUI.Popup(new(0, 0, 200, 20), selTab, tabs);
        int i = Event.current.button;
        if (selectedDestroy && (i == 1 || i == 2))
        {
            selectedDestroy = false;
            destroyMarked = null;
        }
        if (selTab == researchData.categories.Count)
        {
            categName = GUI.TextField(new(210, 0, 150, 20), categName);
            GUI.enabled = categName != "";
            if (GUI.Button(new(370, 0, 100, 20), new GUIContent("create")))
            {
                researchData.categories.Add(new(categName));
                categName = "";
                EditorUtility.SetDirty(researchData);
                return;
            }
        }
        else
        {
            categName = GUI.TextField(new(210, 0, 150, 20), categName);
            GUI.enabled = categName != "" && !researchData.categories.Select(q => q.categName).Contains(categName);
            if (GUI.Button(new(370, 0, 100, 20), new GUIContent("rename")))
            {
                researchData.categories[selTab].categName = categName;
                categName = "";
                EditorUtility.SetDirty(researchData);
                return;
            }
            else
            {
                GUI.enabled = categName == researchData.categories[selTab].categName;
                GUI.color = GUI.enabled ? Color.red : Color.white;
                if (GUI.Button(new(500, 0, 200, 20), new GUIContent("delete (enter category name)")))
                {
                    researchData.categories.RemoveAt(selTab);
                    categName = "";
                    EditorUtility.SetDirty(researchData);
                    return;
                }
                else
                {
                    GUI.enabled = true;
                    GUI.color = Color.white;
                    if (GUI.Button(new(750, 0, 100, 20), new GUIContent("add node")))
                    {
                        int headCount = researchData.categories[selTab].nodes.Count;
                        researchData.categories[selTab].nodes.Add(new(new(), $"node {headCount+1}"));
                        CalculateHeads();
                    }
                    GUI.BeginGroup(new(5, 25, position.width - 10, position.height - 30));
                    GUI.Box(new(0, 0, position.width, position.height - 30), "");
                    scroll = GUI.BeginScrollView(new Rect(0, 0, position.width - 10, position.height - 30), scroll, new Rect(0, 0, 100, 100));
                    RenderNodes(researchData.categories[selTab].nodes);
                    GUI.EndScrollView();

                    GUI.EndGroup();
                }
            }
        }
    }

    void CalculateHeads()
    {
        float headCount = researchData.categories[selTab].nodes.Count-1;
        float startX = (position.width / 2) - nodeWidth / 2 - (headCount * nodeSpace / 2);
        if (startX > 0)
        {
            foreach (ResearchNode node in researchData.categories[selTab].nodes)
            {
                node.gp = new(startX, 20);
                startX += nodeSpace;
            }
        }
        EditorUtility.SetDirty(researchData);
    }

    /// <summary>
    /// Handles Rendering of nodes.
    /// </summary>
    /// <param name="researchNodes">The research category that is to be rendered.</param>
    void RenderNodes(List<ResearchNode> researchNodes)
    {
        int i = 0;
        foreach (ResearchNode node in researchNodes)
        {
            GUI.BeginGroup(new(node.gp.x, node.gp.z, nodeWidth, nodeHeight));
            GUI.Box(new(0, 0, nodeWidth, nodeHeight-10), node.data);
            if (DestroyButton(node))
            {
                if (!selectedDestroy)
                {
                    researchData.DeselectBuilding(node);
                    researchNodes.RemoveAt(i);
                    CalculateHeads();
                }
                GUI.EndGroup();
                break;
            }
            else
            {
                if (CategoryPopup(node) || BuildPopup(node))
                {
                    EditorUtility.SetDirty(researchData);
                    GUI.EndGroup();
                    break;
                }
                else
                {
                    ConnectionButton();
                }
            }
            GUI.EndGroup();
            i++;
        }
        //DrawLine(new(800, 15), Event.current.mousePosition, 3);
    }
    /// <summary>
    /// Handles actions of the destroy button on node.
    /// </summary>
    /// <param name="node">Node that is beeing rendered.</param>
    /// <returns>if clicked on the destoy button</returns>
    bool DestroyButton(ResearchNode node)
    {
        if (selectedDestroy && destroyMarked == node)
        {
            GUI.color = Color.red;
            if (GUI.Button(new(5, 5, 15, 15), "X"))
            {
                selectedDestroy = false;
                destroyMarked = null;
                if (Event.current.button == 0)
                {
                    return true;
                }
            }
            GUI.color = Color.white;
        }
        else
        {
            if (GUI.Button(new(5, 5, 15, 15), "X") && Event.current.button == 0)
            {
                selectedDestroy = true;
                destroyMarked = node;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Renders the category popup.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>if the value changed</returns>
    bool CategoryPopup(ResearchNode node)
    {
        List<string> tabs = new() { "Select" };
        tabs.AddRange(researchData.buildButtons.buildingCategories.Select(q => $"{q.categName}"));
        if (node.buttonCategory == -1)
            GUI.contentColor = Color.red;
        int index = EditorGUI.Popup(new(5, 20, nodeWidth - 10, 20), node.buttonCategory + 1, tabs.ToArray()) - 1;
        GUI.contentColor = Color.white;
        if (index != node.buttonCategory)
        {
            researchData.DeselectBuilding(node);
            node.buttonCategory = index;
            node.buildButton = -1;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Renders the building selection popup.
    /// </summary>
    /// <param name="node">Node that is beeing rendered.</param>
    /// <returns>if the selected build changed</returns>
    bool BuildPopup(ResearchNode node)
    {
        if (node.buttonCategory > -1)
        {
            List<string> tabs = new() { "Select" };
            tabs.AddRange(researchData.GetUnssignedBuildings(node));
            int tmpIndex = node.buildButton == -1 ? 0 : 1;
            if (tmpIndex == 0)
                GUI.contentColor = Color.red;
            int index = EditorGUI.Popup(new(5, 40, nodeWidth - 10, 20), tmpIndex, tabs.ToArray());
            GUI.contentColor = Color.white;
            if (index != tmpIndex)
            {
                researchData.DeselectBuilding(node);
                node.buildButton = researchData.GetIndex(node.buttonCategory, tabs[index]);
                if (node.buildButton > -1)
                {
                    researchData.SelectBuilding(node);
                }
                return true;
            }
        }
        return false;
    }

    void ConnectionButton()
    {
        startPoint = new(10, 20);
        Vector2 endPoint = new(30, 50);
        if (GUI.Button(new(nodeWidth/2-10, nodeHeight-25, 25, 25), circle))
        {
            Debug.Log($"Holding down {seconds}");
            seconds += 1;
        }
        else
        {
            seconds = 0;
        }
    }
    private void DrawLine(Vector2 start, Vector2 end, int width)
    {
        Vector2 d = end - start;
        float a = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
        if (d.x < 0)
            a += 180;

        int width2 = (int)Mathf.Ceil(width / 2);

        GUIUtility.RotateAroundPivot(a, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), point);
        GUIUtility.RotateAroundPivot(-a, start);
    }


}
#endif