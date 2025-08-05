using AbstractControls;
using UnityEngine.UIElements;

namespace TradeWindowElements
{

    [UxmlElement]
    public partial class TradeButtonGroup : CustomRadioButtonGroup
    {
        enum ViewType
        {
            None,
            Colony,
            Trade,
            Outpost
        }
        VisualElement leftBar;
        ColonyView colonyView;
        TradeView tradeView;
        OutpostView outpostView;

        ViewType prevView = ViewType.None;
        public int tradeLocationCount;

        public TradeButtonGroup() : base()
        {
        }

        public TradeButtonGroup(VisualElement elem, int _tradeLocationCount) : base()
        {
            leftBar = ToolkitUtils.GetRoot(elem).Q<VisualElement>("LeftBar");
            if (leftBar != null)
            {
                colonyView = (ColonyView)leftBar.ElementAt(1);
                tradeView = (TradeView)leftBar.ElementAt(2);
                outpostView = (OutpostView)leftBar.ElementAt(3);

                colonyView.Hide();
                tradeView.Hide();
                SetChangeCallback(SwitchViews);
            }
            tradeLocationCount = _tradeLocationCount;
        }


        void SwitchViews(int index)
        {
            switch (prevView)
            {
                case ViewType.Colony:
                    colonyView.Hide();
                    break;
                case ViewType.Trade:
                    tradeView.Hide();
                    break;
                case ViewType.Outpost:
                    outpostView.Hide();
                    break;
            }
            string headerText = "";
            if(index == 0)
            {
                headerText = colonyView.Open();
                prevView = ViewType.Colony;
            }
            else if(index <= tradeLocationCount)
            {
                headerText = tradeView.Open(index - 1);
                prevView = ViewType.Trade;
            }
            else
            {
                headerText = outpostView.Open(index - tradeLocationCount);
                prevView = ViewType.Outpost;
            }
            ((Label)leftBar.ElementAt(0).ElementAt(0)).text = headerText;
        }
    }
}
