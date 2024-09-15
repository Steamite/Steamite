using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Overlays;
using System.Linq;
using UnityEngine;
using System;
using Codice.Client.Common.TreeGrouper;

#if UNITY_EDITOR
public class ResearchEditor : EditorWindow
{
    ResearchData researchData;
    int selTab = 0;
    ResearchNode destroyMarked;
    bool selectedDestroy;
    string categName;
    const float nodeWidth = 175;
    const float nodeSpace = nodeWidth + 75;

    [MenuItem("Custom Editors/Research Editor", priority=15)]
    public static void ShowWindow()
    {
        ResearchEditor window = GetWindow(typeof(ResearchEditor)) as ResearchEditor;
        window.titleContent = new("Research Editor");
        window.maximized = true;
        window.researchData = (ResearchData)Resources.Load("Holders/Data/ResearchData");
    }

    private void OnGUI()
    {
        if (!researchData)
            researchData = (ResearchData)Resources.Load("Holders/Data/ResearchData");
        string[] tabs = researchData.categories.Select(q => $"{q.categName} ({q.heads.Count})").Append("create new").ToArray();
        selTab = EditorGUI.Popup(new(0,0,200,20), selTab, tabs);
        SwitchCategory();
    }

    private void SwitchCategory()
    {
        //if (Event.current.button != -1)
        //{
        //    selectedDestroy = false;
        //}
        if (selTab == researchData.categories.Count)
        {
            categName = GUI.TextField(new(210, 0, 150, 20), categName);
            GUI.enabled = categName != "";
            if (GUI.Button(new(370, 0, 100, 20), new GUIContent("create")))
            {
                researchData.categories.Add(new(categName));
                categName = "";
                EditorUtility.SetDirty(researchData);
            }
        }
        else
        {
            categName = GUI.TextField(new(210, 0, 150, 20), categName);
            GUI.enabled = categName != "" && !researchData.categories.Select(q=> q.categName).Contains(categName);
            if (GUI.Button(new(370, 0, 100, 20), new GUIContent("rename")))
            {
                researchData.categories[selTab].categName = categName;
                categName = "";
                EditorUtility.SetDirty(researchData);
            }
            else
            {
                GUI.enabled = categName == researchData.categories[selTab].categName;
                GUI.color = GUI.enabled ? Color.red : Color.white;
                if(GUI.Button(new(500, 0, 200, 20), new GUIContent("delete (enter category name)")))
                {
                    researchData.categories.RemoveAt(selTab);
                    categName = "";
                    EditorUtility.SetDirty(researchData);
                }
                else
                {
                    GUI.enabled = true;
                    GUI.color = Color.white;
                    if (GUI.Button(new(750, 0, 100, 20), new GUIContent("add node")))
                    {
                        float headCount = researchData.categories[selTab].heads.Count;
                        if(headCount < 10)
                        {
                            float startX = (position.width / 2) - nodeWidth/2 - (headCount * nodeSpace / 2);
                            researchData.categories[selTab].heads.Add(new(new(), $"node {headCount + 1}"));
                            foreach (ResearchNode node in researchData.categories[selTab].heads)
                            {
                                node.rect = new(startX, 20, nodeWidth, 100);
                                startX += nodeSpace;
                            }
                        }
                    }
                    GUI.BeginGroup(new(5, 25, position.width-10, position.height - 30));
                    GUI.Box(new(0, 0, position.width, position.height - 30), "");
                    RenderNodes(researchData.categories[selTab].heads);
                    GUI.EndGroup();
                }
            }
        }

        void RenderNodes(List<ResearchNode> researchNodes)
        {
            if (researchNodes.Count == 6)
                researchNodes.RemoveRange(0, researchNodes.Count);
            int toDestroy = -1;

            foreach (ResearchNode node in researchNodes)
            {
                GUI.BeginGroup(node.rect);
                GUI.Box(new(0, 0, node.rect.width, node.rect.height), node.data);

                if (selectedDestroy && destroyMarked == node)
                {
                    GUI.color = Color.red;
                    if (GUI.Button(new(0, 0, 15, 15), "X"))
                    {
                        if (Event.current.button == 2)
                        {
                            selectedDestroy = false;
                            //toDestroy = node;
                        }
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    if (GUI.Button(new(0, 0, 15, 15), "X"))
                    {
                        selectedDestroy = true;
                        destroyMarked = node;
                    }
                }


                GUI.EndGroup();
            }
        }
    }
}
/*
public class RenameTab : EditorWindow
{
    Action<string> action;
    public static void Init(Action<string> _action)
    {
        RenameTab window = ScriptableObject.CreateInstance<RenameTab>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
        window.ShowPopup();
        window.action = _action;
    }

    void CreateGUI()
    {
        var label = new UnityEngine.UIElements.TextField("category name");
        rootVisualElement.Add(label);

        var button = new Button();
        button.text = "Rename";
        button.clicked += () => OnClick(true);
        rootVisualElement.Add(button);

        button = new Button();
        button.text = "Cancel";
        button.clicked += () => OnClick(false);
        rootVisualElement.Add(button);
    }
    void OnClick(bool isRename)
    {
        this.position = new(100, 100, 100, 100);
        if (!isRename)
            Close();
        string s = (rootVisualElement.Children().First(q => q is UnityEngine.UIElements.TextField) as UnityEngine.UIElements.TextField).value;
        if (s != "")
        {
            action(s);
            this.Close();
        }
    }
}*/
#endif