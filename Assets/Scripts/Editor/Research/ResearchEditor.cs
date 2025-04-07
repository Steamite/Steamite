#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using static TreeEditor.TreeEditorHelper;

/// <summary>Used for modifying <see cref="ResearchData"/>.</summary>
public class ResearchEditor : EditorWindow
{
    #region Variables
    /// <summary>Data source.</summary>
    ResearchData researchData;

    /// <summary>Texture for lines.</summary>
    [SerializeField] Texture2D point;
    /// <summary>Texture for connection buttons</summary>
    [SerializeField] Texture2D circle;

	[SerializeField] private VisualTreeAsset treeAsset = default;
	[SerializeField] private VisualTreeAsset nodeAsset = default;
	/// <summary>Instance of the window.</summary>
	static EditorWindow open;

    TabView categoryView;
    #endregion

    #region Opening
    /// <summary>Opens the window, if it's already opened close it.</summary>
    [MenuItem("Custom Editors/Research Editor %t", priority = 15)]
    public static void Open()
    {
        ResearchEditor wnd = GetWindow<ResearchEditor>();
        wnd.titleContent = new GUIContent("Research Editor");
    }
    #endregion
    void SaveValues() => EditorUtility.SetDirty(researchData);
    /// <summary>Fills the button style and recalculates head placement</summary>
	private void CreateGUI()
	{
        return;
		VisualElement doc = treeAsset.Instantiate();
		rootVisualElement.Add(doc);

		categoryView = doc.Q<TabView>();
		researchData = (ResearchData)Resources.Load("Holders/Data/Research Data");
		researchData.Init();
		for (int i = 0; i < researchData.categories.Count; i++)
        {
            ResearchCategory category = researchData.categories[i];
			Tab tab = new(category.categName);
            categoryView.Insert(i, tab);
            for (int j = 0; j < 5; j++)
			{
                VisualElement row = new();
                row.name = "Row";
				tab.Add(row);
			}

            for (int j = 0; j < category.nodes.Count; j++)
            {
                CreateResearchButton(category.nodes[j], i);
            }
        }
	}

    void CreateResearchButton(ResearchNode nodeData, int categIndex)
    {
		VisualElement nodeElem = nodeAsset.CloneTree();
		categoryView[categIndex][nodeData.level].Add(nodeElem);
        DropdownField nodeType = nodeElem.Q<DropdownField>("Type");
        nodeType.index = nodeData.buildingNode ? 1 : 0;
        nodeType.RegisterValueChangedCallback<string>(
            (ev) => 
            {
                nodeData.buildingNode = nodeType.index == 1;
                nodeData.nodeCategory = -1;
                InitNodeCateg(nodeData, nodeElem);
                SaveValues();
			});

        InitResourceList(nodeElem.Q<ListView>("Cost"), nodeData);

		TextField nameField = nodeElem.Q<TextField>("Name");
		nameField.value = nodeData.name;
        nameField.RegisterValueChangedCallback<string>(
            (ev) =>
            {
                nodeData.name = ev.newValue;
				SaveValues();
			});

		IntegerField ammountField = nodeElem.Q<IntegerField>("Ammount");
		ammountField.value = nodeData.researchTime;
		ammountField.RegisterValueChangedCallback<int>(
            (ev) => 
            {
                nodeData.researchTime = ev.newValue;
				SaveValues();
			});

        InitNodeCateg(nodeData, nodeElem);
	}
    void InitResourceList(ListView resourceView, ResearchNode node)
    {
        List<int> res = new();
        for (int i = 0; i < node.reseachCost.type.Count; i++)
            res.Add(-1);
        resourceView.itemsSource = res;

        resourceView.onAdd = (_) => 
        {
            node.reseachCost.Add(0, 0);
            resourceView.itemsSource.Add(-1);
			resourceView.RefreshItems();
			SaveValues();
		};
        resourceView.onRemove = (_) =>
        {
            if(resourceView.selectedIndex > -1)
            {
                node.reseachCost.type.RemoveAt(resourceView.selectedIndex);
                node.reseachCost.ammount.RemoveAt(resourceView.selectedIndex);
				resourceView.itemsSource.RemoveAt(resourceView.selectedIndex);
				SaveValues();
			}
            resourceView.RefreshItems();
        };

        resourceView.bindItem = (el, i) =>
        {
            var x = i;
            EnumField resSelector = el.Q<EnumField>();
            resSelector.value = node.reseachCost.type[i];
            resSelector.RegisterValueChangedCallback<Enum>(
                (ev) => 
                {
                    node.reseachCost.type[x] = (ResourceType)ev.newValue;
					SaveValues();
				});

            IntegerField field = el.Q<IntegerField>();
            field.value = node.reseachCost.ammount[i];
            field.RegisterValueChangedCallback<int>(
				(ev) => 
                { 
                    node.reseachCost.ammount[x] = ev.newValue;
					SaveValues();
				});
		};
    }
    void InitNodeCateg(ResearchNode nodeData, VisualElement nodeElem)
	{
		DropdownField categ = nodeElem.Q<DropdownField>("Categ");
		if (nodeData.buildingNode)
        {
			categ.choices = researchData.categories.Select(q => q.categName).ToList();
			if (nodeData.nodeCategory == -1)
            {
                categ.choices.Insert(0, "Select");
				categ.index = 0;
			}
            else
            {
				categ.index = nodeData.nodeCategory;
			}

			categ.RegisterValueChangedCallback(
                (_) => 
                {
                    nodeData.nodeCategory = categ.index;
                    nodeData.nodeAssignee = -1;
					SaveValues();
                    InitAssigne(nodeData, nodeElem);
				});
		}
        InitAssigne(nodeData, nodeElem);
	}
    void InitAssigne(ResearchNode nodeData, VisualElement nodeElem)
    {
        DropdownField field = nodeElem.Q<DropdownField>("Assignee");
        if(nodeData.nodeCategory == -1)
        {
            field.style.display = DisplayStyle.None;
            return;
		}
		else
			field.style.display = DisplayStyle.Flex;

		if (nodeData.buildingNode)
        {
            field.choices = researchData.GetUnassignedBuildings(nodeData);
			if (nodeData.nodeAssignee == -1)
			{
				field.choices.Insert(0, "Select");
				field.index = 0;
			}
			else
			{
				field.index = nodeData.nodeCategory;
			}

			field.RegisterValueChangedCallback(
				(_) =>
				{
					nodeData.nodeAssignee = field.index;
					SaveValues();
				});
		}
	}
}
#endif