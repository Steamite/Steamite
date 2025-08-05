using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace ResearchUI
{

    public enum NodeType
    {
        Dummy,
        Stat,
        Building
    }

    /// <summary>Represents one thing to research, can have prequiseites and folowing ones.</summary>
    [Serializable]
    public class ResearchNode : DataObject, IUpdatable
    {
        #region Variables

        /// <summary>Level of the research</summary>
        [SerializeField] public int level;

        /// <summary>Was it already researched?</summary>
        [SerializeField] public bool researched;
        public void RegisterFinishCallback(Action action) => onFinishResearch += action;
        /// <summary>Called when research is finished.</summary>
        event Action onFinishResearch;
        /// <summary>Current progress time.</summary>
        [SerializeField] float currentTime = -1;
        /// <summary>Current progress time and if the research is finished then call all events.</summary>
        [CreateProperty]
        public float CurrentTime
        {
            get => currentTime;
            set
            {
                currentTime = value;
                if (currentTime >= researchTime)
                {
                    researched = true;
                    onFinishResearch?.Invoke();
                    onFinishResearch = null;
                }
                else
                    UIUpdate(nameof(CurrentTime));
            }
        }
        /// <summary>Target progress time.</summary>
        [SerializeField] public int researchTime;

        /// <summary>Cost to start research(WIP).</summary>
        [SerializeField] public MoneyResource reseachCost;

        /// <summary>If the button is unlocking a building.</summary>
        [SerializeField] public NodeType nodeType;
        /// <summary>Category of foreing elements that is assigned to.</summary>
        [SerializeField] public int nodeCategory;
        /// <summary>Element id from the category.</summary>
        [SerializeField] public int nodeAssignee;
        /// <summary>Prequisite needed nodes.</summary>
        [SerializeField] public List<int> unlockedBy;
        /// <summary>Next nodes.</summary>
        [SerializeField] public List<int> unlocks;
        public string description;

        [NonSerialized] public Sprite preview;

        [SerializeField] public Color lineColor = Color.red;


        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            try
            {
                ResearchNode node = (ResearchNode)obj;
                if (id == node?.id)
                    return true;
            }
            catch
            {
                //Debug.Log("Somthing went wrong");
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        public ResearchNode() { }

        public ResearchNode(ResearchNode node)
        {
            id = node.id;
            level = node.level;
            Name = node.Name;
            currentTime = node.currentTime;
            researchTime = node.researchTime;
            nodeCategory = node.nodeCategory;
            nodeAssignee = node.nodeAssignee;
            researched = node.researched;
            unlockedBy = node.unlockedBy;
            unlocks = node.unlocks;
            reseachCost = new();
            if (node.reseachCost != null)
            {
                reseachCost.types = node.reseachCost.types.ToList();
                reseachCost.ammounts = node.reseachCost.ammounts.ToList();
            }
        }
        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
        #region Editor modifications
#if UNITY_EDITOR

        /// <summary>
        /// Used when creating.
        /// </summary>
        /// <param name="_gp"></param>
        /// <param name="_data"></param>
        /// <param name="level"></param>
        public ResearchNode(int _level, int _id) : base(_id)
        {
            level = _level;
            Name = $"node {_level}";

            id = _id;
            researchTime = 100 * (_level + 1);

            currentTime = 0;
            researched = false;
            nodeCategory = -1;
            nodeAssignee = -1;
            unlockedBy = new();
            unlocks = new();
            reseachCost = new();
        }


        /// <summary>
        /// Connects this node as a prequisete to <paramref name="node"/>.
        /// </summary>
        /// <param name="node">Connecting node.</param>
        public void ConnectNode(ResearchNode node)
        {
            if (!unlocks.Contains(node.id))
            {
                unlocks.Add(node.id);
                node.unlockedBy.Add(id);
            }
        }

        /// <summary>
        /// Disconnects all connected nodes.
        /// </summary>
        public void DisconnectNodes(List<ResearchNode> category, bool justTop = false)
        {

            if (justTop == false)
            {
                for (int i = unlocks.Count - 1; i >= 0; i--)
                    DisconnectNode(true, i, category);
            }

            for (int i = unlockedBy.Count - 1; i >= 0; i--)
                DisconnectNode(false, i, category);
        }

        /// <summary>
        /// Disconnects one node.
        /// </summary>
        /// <param name="disconectUp">if true disconects node form unlocks</param>
        /// <param name="index">index in list</param>
        public void DisconnectNode(bool disconectUp, int index, List<ResearchNode> category)
        {
            if (disconectUp)
            {
                category.Find(q => q.id == unlocks[index]).unlockedBy.Remove(id);
                unlocks.RemoveAt(index);
            }
            else
            {
                category.Find(q => q.id == unlockedBy[index]).unlocks.Remove(id);
                unlockedBy.RemoveAt(index);
            }
        }

        public string GetDescr(BuildingStats.Stat stat)
        {
            if (stat == null)
                return description;
            string statDescription = "";
            string[] strings = Enum.GetNames(typeof(BuildingCategType));
            foreach (var pair in stat.pairs)
            {
                statDescription += "\n";
                int mask = pair.mask;
                bool prev = false;
                if (mask == -1)
                {
                    statDescription += "Everything";
                }
                else
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        if ((mask & 1) == 1)
                        {
                            if (prev)
                                statDescription += ", ";
                            else
                                prev = true;
                            statDescription += strings[i];
                        }
                        mask = mask >> 1;
                        if (mask == 0)
                            break;
                    }
                }
                if (pair.percent)
                    statDescription += $": {pair.mod} {pair.modAmmount}%";
                else
                    statDescription += $": {pair.mod} by {pair.modAmmount}";
            }
            strings = description.Split('$');
            description = $"{strings[0]}${statDescription}";
            return description;
        }
#endif
        #endregion
    }

    /// <summary>Research Category groups nodes into logical pages.</summary>
    [Serializable]
    public class ResearchCategory : DataCategory<ResearchNode>
    {
        #region Editor
#if UNITY_EDITOR
        public void AddNode(int level, ResearchData data)
        {
            ResearchNode node = new(level, data.UniqueID());

            Objects.Add(node);
            Objects = Objects.OrderBy(q => q.level).ToList();
        }
#endif
        #endregion

        public ResearchCategory() { }

        public bool CheckPrequisite(ResearchNode node, Action unlockAction)
        {
            bool result = true;
            for (int i = node.unlockedBy.Count - 1; i > -1; i--)
            {
                ResearchNode n = Objects.Find(q => q.id == node.unlockedBy[i]);
                if (n.researched)
                    node.unlockedBy.RemoveAt(i);
                else
                {
                    if (unlockAction != null)
                    {
                        n.RegisterFinishCallback(() =>
                        {
                            if (CheckPrequisite(node, null))
                                unlockAction();
                        });
                    }
                    result = false;
                }
            }
            return result;
        }

    }

    /// <summary>Contains all research data, that can be edited.</summary>
    [CreateAssetMenu(fileName = "ResearchData", menuName = "UI Data/Research Holder", order = 2)]
    public class ResearchData : DataHolder<ResearchCategory, ResearchNode>
    {
    }
}
