using BuildingStats;
using ResearchUI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal.VR;
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
        public StatData statData;
        public ResearchNode activeNode;
        #endregion

//        public void SaveValues() => EditorUtility.SetDirty((ResearchData)data);

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
            data = AssetDatabase.LoadAssetAtPath<ResearchData>(
                "Assets/Game Data/Research && Building/Research Data.asset");
            buildingData = AssetDatabase.LoadAssetAtPath<BuildingData>(
                "Assets/Game Data/Research && Building/Build Data.asset");
            statData = AssetDatabase.LoadAssetAtPath<StatData>(
                "Assets/Game Data/Research && Building/Stats.asset");

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
                RecalculateAvailable(true);
                RecalculateAvailable(false);
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

        public void RecalculateAvailableByNode(ResearchNode _nodeData)
        {
            if (_nodeData.nodeType == NodeType.Building)
                RecalculateAvailable(true);
            else if (_nodeData.nodeType == NodeType.Stat)
                RecalculateAvailable(false);
        }

        public void RecalculateAvailable(bool buildings)
        {
            if (buildings)
            {
                RecalculateAvailableObjects<BuildingData, BuildCategWrapper, BuildingWrapper>(buildingData, NodeType.Building);
            }
            else
            {
                RecalculateAvailableObjects<StatData, BuildingStatCateg, Stat>(statData, NodeType.Stat);
            }

        }

        void RecalculateAvailableObjects<T_H, T_C, T_O>(T_H holder, NodeType type)
            where T_H : DataHolder<T_C, T_O>
            where T_C : DataCategory<T_O>
            where T_O : DataObject
        {
            List<ResearchNode> nodes = data.Categories.SelectMany(q => q.Objects).ToList();
            for (int i = 0; i < holder.Categories.Count; i++)
            {
                T_C categ = holder.Categories[i];
                categ.availableObjects = new();
                List<ResearchNode> categoryNodes = nodes.Where(q => q.nodeType == type && q.nodeCategory == i).ToList();
                for (int j = 0; j < categ.Objects.Count; j++)
                {
                    if (categoryNodes.FindIndex(q => q.nodeAssignee == categ.Objects[j].id) == -1)
                    {
                        categ.availableObjects.Add(categ.Objects[j]);
                    }
                }
            }
        }


        #endregion

        #region Buttons
        /// <summary>Repaints the whole row.</summary>
        /// <param name="level">Row level, that is to be repainted.</param>
        public void RepaintRow(int level)
        {
            tree[level][1].Clear();
            RedoLines(level);
            if (showCreateButtons)
            {
                Button addButton = new Button(plus,
                    () =>
                    {
                        ((ResearchCategory)selectedCategory).AddNode(level, (ResearchData)data);
                        RepaintRow(level);
                    });
                addButton.AddToClassList("add-button");
                tree[level][1].Add(addButton);
            }
        }

        #region Lines
        /// <summary>Creates nodes in the <paramref name="level"/> and lines, that are needed to unlock the nodes.</summary>
        /// <param name="level">Row to make lines for</param>
        void RedoLines(int level)
        {
            int levelIndex = 0, lineCount = 0;
            foreach (ResearchNode node in selectedCategory.Objects.Where(q => q.level == level))
            {
                tree[level][1].Insert(levelIndex, new ResearchNodeElem(node, this, (ResearchData)data));
                if (node.unlockedBy.Count > 0)
                {
                    for (int j = 0; j < node.unlockedBy.Count; j++)
                    {
                        var unlockedId = node.unlockedBy[j];
                        var lvlIndex = levelIndex;
                        var lnCount = lineCount;
                        tree[level][1][levelIndex].RegisterCallback<GeometryChangedEvent>(
                            (_) => RedoLines(node, unlockedId, lvlIndex, lnCount));
                    }
                    lineCount++;
                }
                levelIndex++;
            }
        }

        /// <summary>Creates the lines from the <paramref name="node"/>.</summary>
        /// <param name="node">Node to create lines from.</param>
        /// <param name="unlockedBy">Unlocked by id.</param>
        /// <param name="lvlIndex">Index in level.</param>
        /// <param name="lnCount">Number of done lines.</param>
        void RedoLines(ResearchNode node, int unlockedBy, int lvlIndex, int lnCount)
        {
            int prequiseteIndex = GetIndexInRow(unlockedBy);
            if (prequiseteIndex == -1)
                return;
            ResearchNode connectedNode = selectedCategory.Objects.First(q => q.id == unlockedBy);
            Button prequiseteButton = tree
                [connectedNode.level][1]
                [prequiseteIndex].Q<Button>("Bot");

            string lineName = $"{node.id}, {connectedNode.id}";
            List<VisualElement> prevLines = tree[node.level][1].Children().Where(q => q.name == lineName).ToList();
            foreach (VisualElement prevLine in prevLines)
                tree[node.level][1].Remove(prevLine);

            #region Long Line
            VisualElement line = new();
            line.name = lineName;
            line.AddToClassList("line");
            int height = (node.level - connectedNode.level - 1) * 350 + 28;
            float xPos =
                prequiseteButton.worldBound.x +
                (prequiseteButton.worldBound.width / 2) -
                tree[connectedNode.level][0].resolvedStyle.width +
                (2 * lnCount - 1);
            line.style.width = 2;
            line.style.height = height;
            line.style.left = xPos;
            line.style.top = -height;
            line.style.backgroundColor = node.lineColor;
            tree[node.level][1].Add(line);
            #endregion

            #region Horizontal
            // Horizonal line
            line = new();
            line.name = lineName;
            line.AddToClassList("line");
            Button thisButton = tree[node.level][1][lvlIndex].Q<Button>("Top");
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
            line.style.top = 2 * lnCount;
            line.style.backgroundColor = node.lineColor;
            tree[node.level][1].Add(line);
            #endregion

            #region Down
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
            #endregion
        }
        #endregion

        #region Node events

        /// <summary>Looks if the node is on has a node on left/right side.</summary>
        /// <param name="nodeData">Node that's being asked for.</param>
        /// <param name="left">Look left if true.</param>
        /// <returns>True if a node has an adjacent node on the said side.</returns>
        public bool Exists(ResearchNode nodeData, bool left)
        {
            int index = selectedCategory.Objects.IndexOf(nodeData);
            if (left)
                return
                    index > 0 &&
                    selectedCategory.Objects[index - 1].level == nodeData.level;
            else
                return
                    index < selectedCategory.Objects.Count - 1 &&
                    selectedCategory.Objects[index + 1].level == nodeData.level;
        }

        /// <summary>Returns a list of posible assignable buildings for the <paramref name="node"/>.</summary>
        /// <param name="node">Node that is beeing asked for.</param>
        /// <returns>Returns the a list of all possible choices.</returns>
        public List<string> GetAvailable(ResearchNode node)
        {
            if (node.nodeCategory <= -1 && node.nodeAssignee == -1)
                return new() { "Select" };

            switch (node.nodeType)
            {
                case NodeType.Building:
                    return Available<BuildCategWrapper, BuildingWrapper>(buildingData, node); 
                case NodeType.Stat:
                    return Available<BuildingStatCateg, Stat>(statData, node);
            }
            return new() { "Select" };
        }

        List<string> Available<T_CAT, T_OBJ>(DataHolder<T_CAT, T_OBJ> dataHolder, ResearchNode node) 
            where T_CAT: DataCategory<T_OBJ>
            where T_OBJ: DataObject
        {
            List<string> str = new() { "Select" };
            if (node.nodeAssignee > -1)
                str.Add(dataHolder.Categories[node.nodeCategory].Objects.Find(q => q.id == node.nodeAssignee).GetName());
            str.AddRange(dataHolder.Categories[node.nodeCategory].availableObjects.Select(q => q.GetName()));
            return str;
        }

        /// <summary>Gets list of assignable categories.</summary>
        /// <param name="node">Node that's beeing asked for.</param>
        /// <returns>Returns list of category choices.</returns>
        public List<string> GetActiveCategories
            (ResearchNode node) => node.nodeType == NodeType.Building
                ? buildingData.Categories.Select(q => q.Name).Prepend("Select").ToList()
                : statData.Categories.Select(q => q.Name).Prepend("Select").ToList();

        /// <summary>Event callback for moving the node.</summary>
        /// <param name="moveBy">If -1 then left, if 1 then right.</param>
        public void Move(ResearchNode nodeData, int moveBy)
        {
            int i = selectedCategory.Objects.IndexOf(nodeData);
            selectedCategory.Objects[i] = selectedCategory.Objects[i + moveBy];
            selectedCategory.Objects[i + moveBy] = nodeData;
            for (int j = nodeData.level; j < 5; j++)
                RepaintRow(j);
        }

        /// <summary></summary>
        /// <param name="node"></param>
        public void Delete(ResearchNode node)
        {
            node.DisconnectNodes(selectedCategory.Objects);
            selectedCategory.Objects.Remove(node);
            RepaintRow(node.level);
            SaveValues();
        }
        #endregion

        #endregion

        #region Indexes
        /// <summary>Gets index of a node.</summary>
        /// <param name="node">Node to want the index.</param>
        /// <returns>Index of a <paramref name="node"/>.</returns>
        public int GetIndexInRow(ResearchNode node) =>
            selectedCategory.Objects.FindIndex(q => q.id == node.id) -
            selectedCategory.Objects.FindIndex(q => q.level == node.level);
        
        /// <summary>Gets index of a node by id.</summary>
        /// <param name="id">Id of the node.</param>
        /// <returns>Index of node by <paramref name="id"/></returns>
        public int GetIndexInRow(int id)
        {
            int i = selectedCategory.Objects.FindIndex(q => q.id == id);
            return i - selectedCategory.Objects.FindIndex(q => q.level == selectedCategory.Objects[i].level);
        }
        #endregion
    }
}