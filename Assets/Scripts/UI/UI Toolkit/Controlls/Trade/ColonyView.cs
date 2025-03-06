using InfoWindowElements;
using System.Collections.Generic;
using TradeData.Locations;
using TradeData.Stats;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class ColonyView : VisualElement, IInitiableUI
    {
        #region Variables
        VisualTreeAsset statAsset;
        [UxmlAttribute] VisualTreeAsset StatAsset { get => statAsset; set { statAsset = value; } }

        #endregion

        public ColonyView() : base()
        {
            VisualElement element;
            Label label;
            for (int i = 0; i < 2; i++)
            {
                element = new();
                label = new(i == 0 ? "Passive Production" : "Stats");
                label.name = "Header";
                element.Add(label);
                element.Add(new());
                element.ElementAt(1).name = "Stats";
                Add(element);
            }
            Debug.Log("Constructing colony view");
        }

        public void Init()
        {
            CreateStats(0, UIRefs.trading.colonyLocation.passiveProductions);
            CreateStats(1, UIRefs.trading.colonyLocation.stats);
            style.display = DisplayStyle.None;
        }

        void CreateStats(int i, List<ColonyStat> stats)
        {
            if (statAsset == null)
                return;
            VisualElement statGroup = ElementAt(i).ElementAt(1);
            statGroup.AddToClassList("stat-group");

            foreach (ColonyStat stat in stats)
            {
                VisualElement statElem = statAsset.CloneTree();
                statElem.AddToClassList("stat");
                statElem = statElem.ElementAt(0);
                ((Label)statElem.ElementAt(0)).text = stat.GetText(false);
                ((Label)statElem.ElementAt(2)).text = stat.name;
                statElem = statElem.ElementAt(1);

                RefreshStates(stat, statElem);
                statGroup.Add(statElem.parent.parent);
            }
        }

        void RefreshStates(ColonyStat stat, VisualElement parent, int maxAfford = -1)
        {
            VisualElement el;
            for (int stateLevel = ColonyStat.MAX_STAT_LEVEL; stateLevel > 0; stateLevel--)
            {
                el = parent.ElementAt(stateLevel-1);
                el.ClearClassList();
                el.AddToClassList("state-button");
                if (stateLevel >= stat.maxState)
                {
                    el.AddToClassList("locked");
                    continue;
                }
                else if (stateLevel <= stat.currentState)
                {
                    el.AddToClassList("completed");
                    el.UnregisterCallback<MouseEnterEvent>((eve) => ToolkitUtils.localMenu.Open(stat, (VisualElement)eve.target));
                    el.UnregisterCallback<MouseLeaveEvent>((eve) => ToolkitUtils.localMenu.Close());
                    continue;
                }
                else if (stateLevel <= maxAfford)
                    el.AddToClassList("available");
                el.RegisterCallback<MouseEnterEvent>((eve) => ToolkitUtils.localMenu.Open(stat, (VisualElement)eve.target));
                el.RegisterCallback<MouseLeaveEvent>((eve) => ToolkitUtils.localMenu.Close());
            }
        }
        public string Open()
        {
            style.display = DisplayStyle.Flex;
            ColonyLocation location = UIRefs.trading.colonyLocation;

            VisualElement statGroup = ElementAt(0).ElementAt(1);

            int i = 0;
            location.passiveProductions.ForEach(q => RefreshStates(q, statGroup.ElementAt(i++).ElementAt(0).ElementAt(1), q.CanAfford()));
            
            statGroup = ElementAt(1).ElementAt(1);
            i = 0;
            location.stats.ForEach(q => RefreshStates(q, statGroup.ElementAt(i++).ElementAt(0).ElementAt(1), q.CanAfford()));

            return UIRefs.trading.colonyLocation.name;
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }

        public void ShowDialog()
        {
            
        }
    }
}
