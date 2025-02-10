using System.Collections.Generic;
using TradeData.Stats;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class ColonyView : VisualElement, IInitiableUI
    {
        #region Variables
        VisualTreeAsset statAsset;
        [UxmlAttribute] VisualTreeAsset StatAsset { get => statAsset; set { statAsset = value;} }

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
        }

        public void Init()
        {
            CreateStats(0, UIRefs.trading.colonyLocation.passiveProductions);
            CreateStats(1, UIRefs.trading.colonyLocation.stats);
        }

        void CreateStats(int i, List<ColonyStat> stats)
        {
            if (statAsset == null)
                return;
            VisualElement statGroup = ElementAt(i).ElementAt(1);
            foreach (ColonyStat stat in stats)
            {
                VisualElement statElem = statAsset.CloneTree();
                statElem = statElem.ElementAt(0);
                ((Label)statElem.ElementAt(0)).text = stat.GetText(false); 
                ((Label)statElem.ElementAt(2)).text = stat.name;
                statElem = ElementAt(0);
                int maxAffordable = stat.CanAfford();

                for (int j = 4; j >= ColonyStat.MAX_STAT_LEVEL; j--)
                {
                    if (j >= stat.maxState)
                        statElem.ElementAt(j).AddToClassList("locked");
                    else if (i <= stat.currentState)
                        statElem.ElementAt(j).AddToClassList("completed");
                    else if (j <= maxAffordable)
                        statElem.ElementAt(j).AddToClassList("available");
                }
                statGroup.Add(statElem.parent);
            }
        }


        public string Open()
        {
            return UIRefs.trading.colonyLocation.name;
        }

        public void Hide()
        {

        }

    }
}
