#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System;

/// <summary>Used for modifying <see cref="ResearchData"/>.</summary>
public class ResearchEditor : EditorWindow
{
    #region Variables
    /// <summary>Data source.</summary>
    ResearchData researchData;
    /// <summary>Last interacted node.</summary>
    ResearchNode selectedNode;
    /// <summary>Name of the opened category.</summary>
    string categName = "";
    /// <summary>Node depth to add to</summary>
    string nodeLevel = "0";
    
    /// <summary>Node width</summary>
    const float nodeWidth = 175;
    /// <summary>Node height</summary>
    const float nodeHeight = 195;
    /// <summary><see cref="nodeWidth"/> + space around</summary>
    const float nodeSpace = nodeWidth + 75;

    /// <summary>If the last click was on the destroy button</summary>
    static bool destroing;
    /// <summary>If the last click was on the connect button</summary>
    static bool connecting;
    /// <summary>If the last click was on the disconnect button</summary>
    static bool disconecting;
    /// <summary>Scroll position.</summary>
    static Vector2 scroll = new();
    /// <summary>Selected category tab.</summary>
    static int selTab = -1;
    /// <summary>Texture for lines.</summary>
    [SerializeField]Texture2D point;
    /// <summary>Texture for connection buttons</summary>
    [SerializeField] Texture2D circle;
    /// <summary>Style for buttons</summary>
    GUIStyle circleButtonStyle;

    /// <summary>Instance of the window.</summary>
    static EditorWindow open;
    #endregion

    #region Opening
    /// <summary>Opens the window, if it's already opened close it.</summary>
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
    
    /// <summary>Fills the button style and recalculates head placement</summary>
    void Init()
    {
        titleContent = new("Research Editor");
        researchData = (ResearchData)Resources.Load("Holders/Data/Research Data");
        researchData.Init();
        circleButtonStyle = new GUIStyle();
        circleButtonStyle.normal.background = circle;
        circleButtonStyle.padding = new RectOffset(0, 0, 0, 0);
        maxSize = new(1234, 1440);
        minSize = new(1234, 500);
        maximized = false;
        CalculateHeads();
    }
    #endregion

    void OnGUI()
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
    /// <summary>Handles the top bar</summary>
    void SwitchCategory()
    {
        string[] tabs = researchData.categories.Select(q => $"{q.categName} ({q.nodes.Count(q => q.buildButton > -1)})").Append("create new").ToArray();
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
                        int headCount = researchData.categories[selTab].nodes.Where(q=> q.gp.y == level).Count();
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
                    scroll = GUI.BeginScrollView(new Rect(0, 0, position.width - 10, position.height - 30), scroll, new Rect(0, 0, 1000, 5*(nodeHeight+50)), false, true);
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
                List<ResearchNode> nodes = researchData.categories[selTab].nodes.Where(q => q.gp.y == i).ToList();
                float startX = (position.width / 2) - nodeWidth / 2 - ((nodes.Count - 1) * (nodeSpace / 2));
                if (startX > 0)
                {
                    foreach (ResearchNode node in nodes)
                    {
                        node.gp.x = startX;
                        node.gp.z = 20 + ((nodeHeight + 50) * node.gp.y);
                        node.realX = (node.gp.x + nodeWidth / 2f) * (1920f / 1234f);
                        startX += nodeSpace;
                    }
                }
            }
            researchData.categories[selTab].nodes = researchData.categories[selTab].nodes.OrderBy(q => q.gp.y).ToList();
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
            Color background = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            if (node.buildButton > -1)
                GUI.Box(new(0, 0, nodeWidth, nodeHeight - 10), GUIContent.none); 
            else if (node.buttonCategory > -1)
                GUI.Box(new(0, 0, nodeWidth, 100), GUIContent.none);
            else
                GUI.Box(new(0, 0, nodeWidth, 65), GUIContent.none);
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
            else if (CategoryPopup(node) || BuildPopup(node))
            {
                EditorUtility.SetDirty(researchData);
                GUI.EndGroup();
                break;
            }
            else if(node.buildButton > -1)
            {
                if (ResearchCost(node) || InConnectionButton(node))
                {
                    EditorUtility.SetDirty(researchData);
                    GUI.EndGroup();
                    break;
                }
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
                GUI.color = new(0.1f, 0.1f, 0.55f);
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
        if (connecting && selectedNode != node && !node.unlockedBy.Contains(selectedNode.id) && node.gp.y > selectedNode.gp.y)
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
    bool ResearchCost(ResearchNode node)
    {
        List<string> resourceTypes = Enum.GetNames(typeof(ResourceType)).ToList();

        // filter options
        List<string> resourceTypesFiltred = new() { "select" };
        resourceTypesFiltred.AddRange(resourceTypes);
        resourceTypesFiltred.RemoveAll(q => node.reseachCost.type.Contains((ResourceType)resourceTypes.IndexOf(q)));
        GUI.Label(new(5, 80, nodeWidth, 20), "Cost");

        for (int i = 0; i < 3 && i < node.reseachCost.type.Count + 1; i++)
        {
            int yPos = 100 + (i*20);
            if (node.reseachCost.type.Count == i)
            {
                GUI.contentColor = Color.red;
                int index = EditorGUI.Popup(new(5, yPos, nodeWidth/3, 20), 0, resourceTypesFiltred.ToArray());
                index = resourceTypes.IndexOf(resourceTypesFiltred[index]);
                if(index > -1)
                {
                    node.reseachCost.type.Add((ResourceType)index);
                    node.reseachCost.ammount.Add(-1);
                    return true;
                }
                break;
            }
            else
            {
                List<string> tempFiltred = resourceTypesFiltred.ToList();
                int lastIndex = tempFiltred.IndexOf(node.reseachCost.type[i].ToString());
                if(lastIndex == -1)
                {
                    GUI.contentColor = Color.white;
                    tempFiltred.Insert(1, node.reseachCost.type[i].ToString());
                    lastIndex = 1;
                }
                else
                    GUI.contentColor = Color.red;
                int index = EditorGUI.Popup(new(5, yPos, nodeWidth /3, 20), lastIndex, tempFiltred.ToArray());
                if(index != lastIndex)
                {
                    index = resourceTypes.IndexOf(tempFiltred[index]);
                    if(index > -1)
                    {
                        node.reseachCost.type[i] = (ResourceType)index;
                    }
                    else
                    {
                        node.reseachCost.type.RemoveAt(i);
                        node.reseachCost.ammount.RemoveAt(i);
                    }
                    return true;
                }
            }

            string ammount = GUI.TextField(new(nodeWidth/3 + 10, yPos, (2*nodeWidth/3)-15, 20), node.reseachCost.ammount[i].ToString());
            int x = 0;
            if (ammount == "")
                node.reseachCost.ammount[i] = 0;
            if(int.TryParse(ammount, out x))
            {
                node.reseachCost.ammount[i] = x;
            }

            //int index = EditorGUI.Popup(new(5, 40, nodeWidth - 10, 20), node.reseachCost + 1, resourceTypesFiltred.ToArray()) - 1;
        }
        GUI.contentColor = Color.white;
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
            GUI.backgroundColor = new(0.1f, 0.1f, 0.75f);

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