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

        TextFieldLabel label;

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
                label = leftBar[0][0] as TextFieldLabel;
                colonyView = leftBar.Q<ColonyView>();
                tradeView = leftBar.Q<TradeView>();
                outpostView = leftBar.Q<OutpostView>();
                outpostView.map = elem as TradeMap;

                colonyView.Hide();
                tradeView.Hide();
                outpostView.Hide();
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
            object inspectedObject;
            if (index == -1)
                return;
            if (index == 0)
            {
                inspectedObject = colonyView.Open();
                prevView = ViewType.Colony;
            }
            else if (index <= tradeLocationCount)
            {
                inspectedObject = tradeView.Open(index - 1);
                prevView = ViewType.Trade;
            }
            else
            {
                inspectedObject = outpostView.Open(index - (tradeLocationCount + 1));
                prevView = ViewType.Outpost;
            }
            label.Open(inspectedObject);
        }
    }
}
