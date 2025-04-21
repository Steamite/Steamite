using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorWindows.Research
{
    /// <summary>Used for modifying <see cref="ResearchData"/>.</summary>
    public class ResearchEditor : CategoryWindow<ResearchCategory, ResearchNode>
    {
        #region Variables
        [SerializeField] Texture2D plus;

        public ScrollView tree;
        bool showCreateButtons;

        public BuildingData buildingData;
        public ResearchNode activeNode;
        #endregion

        public void SaveValues() => EditorUtility.SetDirty((ResearchData)data);

        #region Opening
        /// <summary>Opens the window, if it's already opened close it.</summary>
        [MenuItem("Custom Editors/Research Editor %t", priority = 15)]
        public static void Open()
        {
            ResearchEditor wnd = GetWindow<ResearchEditor>();
            wnd.titleContent = new GUIContent("Research Editor");
        }

        /// <summary>Fills the button style and recalculates head placement</summary>
        protected override void CreateGUI()
        {
            activeNode = null;
            showCreateButtons = false;
            data = AssetDatabase.LoadAssetAtPath<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset");
            buildingData = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/Game Data/Research && Building/Build Data.asset");
            base.CreateGUI();

            Toggle toggle = rootVisualElement.Q<Toggle>("Show-Create");
            toggle.value = false;
            toggle.RegisterValueChangedCallback(
                (ev) =>
                {
                    showCreateButtons = !showCreateButtons;
                    for (int i = 0; i < 5; i++)
                    {
                        RepaintRow(i);
                    }
                });
            tree = rootVisualElement.Q<ScrollView>();
            categorySelector.index = 0;
        }
        #endregion

        #region Category Switching
        protected override bool LoadCategData(int index)
        {
            bool categoryExists = base.LoadCategData(index);

            if (categoryExists)
            {
                RecalculateAvailableBuildings();
                for (int i = 0; i < 5; i++)
                {
                    VisualElement row = new();
                    row.name = "Row";
                    row.Add(new Label(i.ToString()));
                    row.Add(new());
                    tree.Add(row);
                    RepaintRow(i);
                }
            }
            else
            {
                selectedCategory = new ResearchCategory();
                for (int i = 0; i < tree.childCount; i++)
                {
                    tree[i][1].Clear();
                }
            }
            return categoryExists;
        }

        public void RecalculateAvailableBuildings()
        {
            List<ResearchNode> nodes = data.Categories.SelectMany(q => q.Objects).ToList();
            for (int i = 0; i < buildingData.Categories.Count; i++)
            {
                BuildCategWrapper categ = buildingData.Categories[i];
                categ.availableBuildings = new();
                List<ResearchNode> categoryNodes = nodes.Where(q => q.nodeType == NodeType.Building && q.nodeCategory == i).ToList();
                for (int j = 0; j < categ.Objects.Count; j++)
                {
                    if (categoryNodes.FindIndex(q => q.nodeAssignee == categ.Objects[j].id) == -1)
                    {
                        categ.availableBuildings.Add(categ.Objects[j]);
                    }
                }
            }
        }
        #endregion

        #region Buttons
        public void RepaintRow(int i)
        {
            tree[i][1].Clear();
            int levelIndex = 0, lineCount = 0;
            foreach (ResearchNode node in selectedCategory.Objects.Where(q => q.level == i))
            {
                tree[node.level][1].Insert(levelIndex, new ResearchNodeElem(node, this, (ResearchData)data));
                if (node.unlockedBy.Count > 0)
                {
                    for (int j = 0; j < node.unlockedBy.Count; j++)
                    {
                        var x = i;
                        var y = j;
                        var lvl = levelIndex;
                        var ln = lineCount;
                        tree[i][1][levelIndex].RegisterCallback<GeometryChangedEvent>((_) => RedoLines(node, x, y, lvl, ln));
                    }
                    lineCount++;
                }
                levelIndex++;
            }
            if (showCreateButtons)
            {
                Button addButton = new Button(plus,
                            () =>
                            {
                                ((ResearchCategory)selectedCategory).AddNode(i);
                                RepaintRow(i);
                            });
                addButton.AddToClassList("add-button");
                tree[i][1].Add(addButton);
            }
        }

        #region Node events
        void RedoLines(ResearchNode node, int i, int index, int lvl, int lnCount)
        {
            int prequiseteIndex = selectedCategory.Objects.FindIndex(q => q.id == node.unlockedBy[index]);
            if (prequiseteIndex == -1)
                return;
            ResearchNode connectedNode = selectedCategory.Objects[prequiseteIndex];
            Button prequiseteButton = tree
                [connectedNode.level][1]
                [prequiseteIndex].Q<Button>("Bot");

            string lineName = $"{node.id}, {connectedNode.id}";
            List<VisualElement> prevLines = tree[i][1].Children().Where(q => q.name == lineName).ToList();
            foreach (VisualElement prevLine in prevLines)
                tree[i][1].Remove(prevLine);


            VisualElement line = new();
            line.name = lineName;
            line.AddToClassList("line");
            int height = (node.level - connectedNode.level - 1) * 300 + 28;
            float xPos = prequiseteButton.worldBound.x + prequiseteButton.worldBound.width / 2
                - tree[connectedNode.level][0].resolvedStyle.width + (2 * lnCount - 1);
            line.style.width = 2;
            line.style.height = height;
            line.style.left = xPos;
            line.style.top = -height;
            line.style.backgroundColor = node.lineColor;
            tree[node.level][1].Add(line);

            // Horizonal line
            line = new();
            line.name = lineName;
            line.AddToClassList("line");
            Button thisButton = tree[i][1][lvl].Q<Button>("Top");
            float width = thisButton.worldBound.x - 50 + thisButton.worldBound.width / 2 - 1;
            float tmp;
            if (width < xPos)
            {
                tmp = xPos;
                xPos = width;
                width = tmp - width;
                tmp = xPos;
            }
            else
            {
                tmp = width;
                width = width - xPos;
            }
            line.style.width = width;
            line.style.height = 2;
            line.style.left = xPos;
            line.style.top = (2 * lnCount);
            line.style.backgroundColor = node.lineColor;
            tree[node.level][1].Add(line);

            // Down line
            line = new();
            line.name = lineName;
            line.AddToClassList("line");
            line.style.width = 2;
            line.style.height = 20;
            line.style.left = tmp;
            line.style.top = 0;
            line.style.backgroundColor = node.lineColor;
            tree[node.level][1].Add(line);
        }

        public bool Exists(ResearchNode nodeData, int v)
        {
            if (v < 0)
                return
                    selectedCategory.Objects.IndexOf(nodeData) > 0 &&
                    selectedCategory.Objects[selectedCategory.Objects.IndexOf(nodeData) - 1].level == nodeData.level;
            else
                return
                    selectedCategory.Objects.IndexOf(nodeData) < selectedCategory.Objects.Count - 1 &&
                    selectedCategory.Objects[selectedCategory.Objects.IndexOf(nodeData) + 1].level == nodeData.level;
        }

        public List<string> GetActiveBuildings(ResearchNode node)
        {
            if (node.nodeCategory <= -1)
                return new() { "Select" };

            List<string> list = buildingData.Categories[node.nodeCategory].availableBuildings.Select(q => q.building?.objectName).Prepend("Select").ToList();
            if (node.nodeAssignee > -1)
            {
                BuildingWrapper wrapper = buildingData.Categories[node.nodeCategory].Objects.FirstOrDefault(q => q.id == node.nodeAssignee);
                if (wrapper != null)
                    list.Insert(1, wrapper.building.objectName);
                else
                    node.nodeAssignee = -1;
            }

            return list;
        }
        public List<string> GetActiveCategories
            (ResearchNode node) => node.nodeType == NodeType.Building
                ? buildingData.Categories.Select(q => q.Name).Prepend("Select").ToList()
                : new List<string>() { "Select" };

        /// <summary>
        /// Event callback for moving the node.
        /// </summary>
        /// <param name="moveBy">If -1 then left, if 1 then right.</param>
        public void Move(ResearchNode nodeData, int moveBy)
        {
            int i = selectedCategory.Objects.IndexOf(nodeData);
            selectedCategory.Objects[i] = selectedCategory.Objects[i + moveBy];
            selectedCategory.Objects[i + moveBy] = nodeData;
            for (int j = nodeData.level; j < 5; j++)
                RepaintRow(j);
        }

        public void Delete(ResearchNode node)
        {
            node.DisconnectNodes(selectedCategory.Objects);
            selectedCategory.Objects.Remove(node);
            RepaintRow(node.level);
            SaveValues();
        }
        #endregion

        #endregion
    }
}