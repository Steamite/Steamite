using AbstractControls;
using System;
using System.Collections.Generic;
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

        ViewType prevView = ViewType.None;
        public TradeButtonGroup() : base()
        {
        }

        public TradeButtonGroup(VisualElement elem) : base()
        {
            leftBar = ToolkitUtils.GetRoot(elem).Q<VisualElement>("LeftBar");                                       
            if(leftBar != null)
            {
                colonyView = (ColonyView)leftBar.ElementAt(1);
                tradeView = (TradeView)leftBar.ElementAt(2);
                SetChangeCallback(SwitchViews);
            }
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
                    // outpostView.Hide();
                    break;
            }
            string headerText = "";
            switch (index)
            {
                case 0:
                    headerText = colonyView.Open();
                    prevView = ViewType.Colony;
                    break;
                case > 0:
                    headerText = tradeView.Open(index-1);
                    prevView = ViewType.Trade;
                    break;
                case < 0:
                    // outpostView.Hide();
                    prevView = ViewType.Outpost;
                    break;
            }
            ((Label)leftBar.ElementAt(0).ElementAt(0)).text = headerText;
        }
    }
}
