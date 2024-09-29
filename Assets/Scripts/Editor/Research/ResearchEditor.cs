using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
public class ResearchEditor : EditorWindow
{
    ResearchData researchData;
    ResearchNode selectedNode;
    string categName = "";
    string nodeLevel = "0";
    const float nodeWidth = 175;
    const float nodeHeight = 120;
    const float nodeSpace = nodeWidth + 75;

    static bool destroing;
    static bool connecting;
    static bool disconecting;
    static Vector2 scroll = new();
    static int selTab = -1;
    Texture2D point;
    Texture2D circle;
    GUIStyle circleButtonStyle;

    static EditorWindow open;

    [MenuItem("Custom Editors/Research Editor %t", priority = 15)]
    public static void ShowWindow()
    {
        if (open == null)
        {
            open = GetWindow(typeof(ResearchEditor));
            (open as ResearchEditor).Init();
        }
        else
            open.Close();
    }

    void Init()
    {
        titleContent = new("Research Editor");
        point = (Texture2D)Resources.Load("Textures/Point");
        circle = (Texture2D)Resources.Load("Textures/Circle");
        researchData = (ResearchData)Resources.Load("Holders/Data/ResearchData");
        researchData.Init();
        circleButtonStyle = new GUIStyle();
        circleButtonStyle.normal.background = circle;
        circleButtonStyle.padding = new RectOffset(0, 0, 0, 0);
        CalculateHeads();
    }
    private void OnGUI()
    {
        GUI.contentColor = Color.white;
        if (!researchData)
        {
            Init();
        }
        if (selTab == -1)
            selTab = 0;
        SwitchCategory();
    }


    void SwitchCategory()
    {
        string[] tabs = researchData.categories.Select(q => $"{q.categName} ({q.nodes.Count})").Append("create new").ToArray();
        selTab = EditorGUI.Popup(new(0, 0, 200, 20), selTab, tabs);
        int i = Event.current.button;
        if ((destroing || connecting) && (i == 1 || i == 2))
        {
            destroing = false;
            connecting = false;
            selectedNode = null;
        }
        if (selTab == researchData.categories.Count)
        {
            categName = GUI.TextField(new(210, 0, 150, 20), categName);
            GUI.enabled = categName != "" && !researchData.categories.Select(q => q.categName).Contains(categName);
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
                GUI.contentColor = GUI.enabled ? Color.red : Color.white;
                if (GUI.Button(new(500, 0, 200, 20), new GUIContent("delete (enter category name)")))
                {
                    researchData.categories.RemoveAt(selTab);
                    categName = "";
                    selTab = -1;
                    EditorUtility.SetDirty(researchData);
                    Init();
                    return;
                }
                else
                {
                    GUI.enabled = true;
                    int level = -1;
                    if (int.TryParse(nodeLevel, out level) && level > -1 && level < 5)
                        GUI.contentColor = Color.white;
                    else
                    {
                        GUI.contentColor = Color.red;
                        level = -1;
                    }
                    GUI.Label(new(750, 0, 70, 20), "level (0-4)");
                    nodeLevel = GUI.TextField(new(815, 0, 20, 20), nodeLevel);
                    
                    GUI.enabled = level > -1 && level < 5;
                    GUI.contentColor = Color.white;
                    if (GUI.Button(new(845, 0, 100, 20), new GUIContent("add node")) && level > -1 && level < 5)
                    {
                        int headCount = researchData.categories[selTab].nodes.Where(q=> q.gp.level == level).Count();
                        if(headCount < 5)
                        {
                            if (researchData.categories[selTab].nodes.Count > 0)
                                headCount = researchData.categories[selTab].nodes[^1].id;
                            else
                                headCount = 0;
                            researchData.categories[selTab].nodes.Add(new(new(), $"node {headCount + 1}", level, headCount));
                            CalculateHeads();
                        }
                    }
                    GUI.enabled = true;

                    if (GUI.Button(new(position.width - 100, 0, 100, 20), new GUIContent("Init")))
                    {
                        Init();
                        return;
                    }
                    GUI.BeginGroup(new(5, 25, position.width - 10, position.height - 30));
                    GUI.Box(new(0, 0, position.width, position.height - 30), "");
                    scroll = GUI.BeginScrollView(new Rect(0, 0, position.width - 10, position.height - 30), scroll, new Rect(0, 0, 1000, 900), false, true);
                    RenderNodes(researchData.categories[selTab].nodes);
                    GUI.EndScrollView();
                    GUI.EndGroup();
                }
            }
        }
    }

    void CalculateHeads()
    {
        if(selTab > -1 && selTab < researchData.categories.Count)
        {
            for (int i = 0; i < 5; i++)
            {
                List<ResearchNode> nodes = researchData.categories[selTab].nodes.Where(q => q.gp.level == i).ToList();
                float startX = (position.width / 2) - nodeWidth / 2 - ((nodes.Count - 1) * (nodeSpace / 2));
                if (startX > 0)
                {
                    foreach (ResearchNode node in nodes)
                    {
                        node.gp.x = startX;
                        node.gp.z = 20 + ((nodeHeight + 50) * node.gp.level);
                        node.realX = (node.gp.x + nodeWidth / 2) * (1920 / position.width);
                        startX += nodeSpace;
                    }
                }
            }
            researchData.categories[selTab].nodes = researchData.categories[selTab].nodes.OrderBy(q => q.gp.level).ToList();
            EditorUtility.SetDirty(researchData);
        }
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
            GUI.Box(new(0, 0, nodeWidth, nodeHeight-10), GUIContent.none);
            GUIStyle style = new();
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            GUI.Label(new(0, 20, nodeWidth, 20), node.name, style);
            if (DestroyButton(node))
            {
                if (!destroing)
                {
                    node.DisconnectNodes(researchNodes);
                    researchData.DeselectBuilding(node);
                    researchNodes.RemoveAt(i);
                    CalculateHeads();
                }
                GUI.EndGroup();
                break;
            }
            else if(CategoryPopup(node) || BuildPopup(node) || InConnectionButton(node))
            {
                EditorUtility.SetDirty(researchData);
                GUI.EndGroup();
                break;
            }
            else
            {


                if (int.TryParse(GUI.TextField(new(42, nodeHeight - 33, 30, 20), node.researchTime.ToString()), out int newResTime))
                {
                    node.researchTime = newResTime;
                    GUI.contentColor = Color.white;
                }
                else
                    GUI.contentColor = Color.red;

                GUI.Label(new(5, nodeHeight - 35, 30, 20), "time:");
                GUI.contentColor = Color.white;
                OutConnectionButton(node);
            }
            GUI.EndGroup();
            Vector2 start = new(node.gp.x + nodeWidth / 2, node.gp.z + 10);
            foreach (ResearchNode rNode in researchNodes.Where(q => node.unlockedBy.Contains(q.id)))
            {
                Vector2 end = new(rNode.gp.x + nodeWidth / 2, rNode.gp.z + nodeHeight - 10);
                GUI.color = Color.red;
                DrawLine(start, new(start.x, start.y - 20));
                DrawLine(new(start.x, start.y - 20), new(end.x, start.y - 20));
                DrawLine(new(end.x, start.y - 20), end);
                GUI.color = Color.white;
            }
            i++;
        }
    }

    /// <summary>
    /// Handles actions of the destroy button on node.
    /// </summary>
    /// <param name="node">Node that is beeing rendered.</param>
    /// <returns>if clicked on the destoy button</returns>
    bool DestroyButton(ResearchNode node)
    {
        if (destroing && selectedNode == node)
        {
            GUI.color = Color.red;
            if (GUI.Button(new(5, 5, 15, 15), "X"))
            {
                destroing = false;
                selectedNode = null;
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
                destroing = true;
                selectedNode = node;
                return true;
            }
        }
        return false;
    }
    bool InConnectionButton(ResearchNode node)
    {
        if (connecting && selectedNode != node && !node.unlockedBy.Contains(selectedNode.id) && node.gp.level > selectedNode.gp.level)
            GUI.backgroundColor = Color.green;
        else
            GUI.backgroundColor = Color.gray;
        if (GUI.Button(new((nodeWidth - 15f) / 2, 5, 15, 15), new GUIContent(), circleButtonStyle))
        {
            if(connecting && selectedNode != node)
            {
                selectedNode.ConnectNode(node);
                selectedNode = null;
                connecting = false;
                disconecting = false;
                return true;
            }
            else
            {
                if (disconecting && selectedNode != null)
                {
                    for (int i = node.unlockedBy.Count - 1; i >= 0; i--)
                    {
                        node.DisconnectNode(false, i, researchData.categories[selTab].nodes);
                    }
                    disconecting = false;
                    selectedNode = null;
                }
                else
                {
                    selectedNode = node;
                    disconecting = true;
                }
            }
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
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
        int index = EditorGUI.Popup(new(5, 40, nodeWidth - 10, 20), node.buttonCategory + 1, tabs.ToArray()) - 1;
        GUI.contentColor = Color.white;
        if (index != node.buttonCategory)
        {
            researchData.DeselectBuilding(node);
            node.buttonCategory = index;
            if (index > -1)
                node.name = researchData.buildButtons.buildingCategories[node.buttonCategory].categName;
            else
                node.name = $"node {node.id}";
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
            tabs.AddRange(researchData.GetUnassignedBuildings(node));
            int tmpIndex = node.buildButton == -1 ? 0 : 1;
            if (tmpIndex == 0)
                GUI.contentColor = Color.red;
            int index = EditorGUI.Popup(new(5, 60, nodeWidth - 10, 20), tmpIndex, tabs.ToArray());
            GUI.contentColor = Color.white;
            if (index != tmpIndex)
            {
                researchData.DeselectBuilding(node);
                node.buildButton = researchData.GetIndex(node.buttonCategory, tabs[index]);
                if (node.buildButton > -1)
                {
                    researchData.SelectBuilding(node);
                    node.name = tabs[index];
                }
                else
                {
                    node.name = researchData.buildButtons.buildingCategories[node.buttonCategory].categName;
                }
                return true;
            }
        }
        return false;
    }

    void OutConnectionButton(ResearchNode node)
    {
        if (connecting && selectedNode == node)
            GUI.backgroundColor = Color.yellow;
        else
            GUI.backgroundColor = Color.red;

        if (GUI.Button(new(nodeWidth / 2 - 10, nodeHeight - 25, 20, 20), new GUIContent(), circleButtonStyle))
        {
            if(selectedNode == node)
            {
                selectedNode = null;
                connecting = false;
            }
            else
            {
                selectedNode = node;
                connecting = true;
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        int width = 4;
        if(start.x > end.x)
        {
            Vector2 tmp = new(start.x, start.y);
            start = new(end.x, end.y);
            end = new(tmp.x, tmp.y);
        }
        Vector2 d = end - start;
        float a = Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x);
        if (d.x < 0)
            a += 180;

        int width2 = (int)Mathf.Ceil(width / 2);

        GUIUtility.RotateAroundPivot(a, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), point);
        GUIUtility.RotateAroundPivot(-a, start);
    }
}
#endif