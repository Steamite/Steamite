using System;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class TradeView : VisualElement
    {
        #region Variables
        VisualTreeAsset dealAsset;
        [UxmlAttribute] VisualTreeAsset DealAsset { get => dealAsset; set { dealAsset = value; CreateDeals(0); CreateDeals(1); } }

        VisualElement GetDeal(int categ, int deal) => ElementAt(categ).ElementAt(1).ElementAt(deal).ElementAt(0);

        TradeLocation selectedLocation;
        int selectedLocationIndex;
        Button confirmButton;

        int BuyMoney, SellMoney, BuyCount, SellCount;
        #endregion

        #region Constructors
        public TradeView()
        {
            style.flexGrow = 1;
            name = "Trade";

            CreateCateg("Buy", 0);
            CreateCateg("Sell", 1);

            #region Summary
            VisualElement summary, temp;
            Label label;
            summary = new();
            summary.name = "Summary";

            #region Numbers
            temp = new();
            temp.name = "Numbers";

            label = new("New Balance:");
            label.name = "NewBalance";
            temp.Add(label);

            label = new("###### £");
            label.name = "NewBalanceValue";
            temp.Add(label);
            summary.Add(temp);
            #endregion
            confirmButton = new();
            confirmButton.AddToClassList("main-button");
            confirmButton.clicked += TradeCommit;
            summary.Add(confirmButton);
            Add(summary);
            #endregion
        }

        VisualElement CreateCateg(string _name, int i)
        {
            VisualElement categ, temp;
            Label label;

            categ = new();
            categ.name = _name;
            categ.AddToClassList("trade-categ");

            #region Header
            temp = new();
            temp.name = "Header";

            label = new(_name);
            label.name = "Label";
            temp.Add(label);

            label = new("(###/###)");
            label.name = "Limit";
            temp.Add(label);

            label = new("##### £");
            label.name = "Summary";
            temp.Add(label);

            categ.Add(temp);
            #endregion

            #region Deals
            temp = new();
            temp.name = "Deals";
            categ.Add(temp);
            if (DealAsset != null)
                CreateDeals(i);
            #endregion
            Add(categ);
            return temp;
        }

        void CreateDeals(int categIndex)
        {
            if (DealAsset == null)
                return;
            VisualElement deals = ElementAt(categIndex).Q<VisualElement>("Deals");
            deals.Children().ToList().ForEach(q => deals.Remove(q));

            for (int i = 0; i < 3; i++)
            {
                DealAsset.CloneTree(deals);
                deals.ElementAt(i).name = "Deal";
                var x = i;
                var y = categIndex;
                deals.ElementAt(i).Q<SliderInt>().RegisterValueChangedCallback(
                    (eve) => UpdateSummary(eve.newValue, y, x));
            }
        }
        #endregion

        #region View switch
        public string Open(int index)
        {
            selectedLocation = UIRefs.trading.tradeLocations[index];
            selectedLocationIndex = index;

            BuyMoney = 0;
            SellMoney = 0;
            BuyCount = 0;
            SellCount = 0;

            SetDeals(0, selectedLocation.Buy);
            SetDeals(1, selectedLocation.Sell);
            UpdateConfirmButton();

            style.display = DisplayStyle.Flex;
            return selectedLocation.name;
        }

        void SetDeals(int categ, List<TradeDeal> tradeDeals)
        {
            VisualElement deals = ElementAt(categ).ElementAt(1);
            Resource globalResources = MyRes.resDataSource.GlobalResources;

            for (int i = 0; i < 3; i++)
            {
                VisualElement deal = deals.ElementAt(i);
                if (tradeDeals.Count <= i)
                {
                    deal.style.display = DisplayStyle.None;
                    continue;
                }
                else
                {
                    deal = deal.ElementAt(0);
                    deal.ElementAt(0).style.unityBackgroundImageTintColor = ToolkitUtils.resSkins.GetResourceColor(tradeDeals[i].type);

                    SliderInt slider = (SliderInt)deal.ElementAt(1);
                    slider.SetValueWithoutNotify(0);
                    if (categ == 0)
                        slider.highValue = Trading.CONVOY_STORAGE_LIMIT;
                    else
                        slider.highValue = Math.Min(globalResources[tradeDeals[i].type], Trading.CONVOY_STORAGE_LIMIT);

                    ((Label)deal.ElementAt(2)).text = $"* {tradeDeals[i].cost} = 0 £";

                    deal.parent.style.display = DisplayStyle.Flex;
                }
            }
            // Categ header
            deals = ElementAt(categ).ElementAt(0);
            ((Label)deals.ElementAt(1)).text = $"(0/{Trading.CONVOY_STORAGE_LIMIT})";
            ((Label)deals.ElementAt(2)).text = $"0 £";
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
        #endregion

        #region Updates
        void UpdateSummary(int value, int categoryIndex, int dealIndex)
        {
            #region Category updates
            VisualElement deal = UpdateDeal(value, categoryIndex, dealIndex);

            int _totalCost, _totalCount;
            if (categoryIndex == 0)
            {
                MakeSummary(out _totalCost, out _totalCount, selectedLocation.Buy, deal);
                for (int i = 0; i < selectedLocation.Buy.Count; i++)
                {
                    deal = GetDeal(categoryIndex, i);
                    ((SliderInt)deal.ElementAt(1)).highValue = Trading.CONVOY_STORAGE_LIMIT - (_totalCount - ((SliderInt)deal.ElementAt(1)).value);
                }
                BuyMoney = _totalCost;
                BuyCount = _totalCount;
            }
            else
            {
                MakeSummary(out _totalCost, out _totalCount, selectedLocation.Sell, deal);
                for (int i = 0; i < selectedLocation.Sell.Count; i++)
                {
                    deal = GetDeal(categoryIndex, i);
                    ((SliderInt)deal.ElementAt(1)).highValue =
                        Math.Min(
                            MyRes.resDataSource.GlobalResources[selectedLocation.Sell[i].type],
                            Trading.CONVOY_STORAGE_LIMIT - (_totalCount - ((SliderInt)deal.ElementAt(1)).value));
                }
                SellMoney = _totalCost;
                SellCount = _totalCount;
            }

            // header elem
            deal = ElementAt(categoryIndex).ElementAt(0);
            ((Label)deal.ElementAt(1)).text = $"({_totalCount}/{Trading.CONVOY_STORAGE_LIMIT})";
            ((Label)deal.ElementAt(2)).text = $"{_totalCost} £";
            #endregion

            #region Total Summary
            UpdateConfirmButton();
            #endregion
        }

        void MakeSummary(out int totalCost, out int totalCount, List<TradeDeal> deals, VisualElement deal)
        {
            totalCost = 0;
            totalCount = 0;
            for (int i = 0; i < deals.Count; i++)
            {
                deal = deal.parent.parent.ElementAt(i).ElementAt(0);
                int count = ((SliderInt)deal.ElementAt(1)).value;
                totalCost += count * deals[i].cost;
                totalCount += count;
            }
        }

        VisualElement UpdateDeal(int value, int categoryIndex, int dealIndex)
        {
            VisualElement deal = GetDeal(categoryIndex, dealIndex);
            if (categoryIndex == 0)
                ((Label)deal.ElementAt(2)).text = $"* {selectedLocation.Buy[dealIndex].cost} = {selectedLocation.Buy[dealIndex].cost * value} £";
            else
                ((Label)deal.ElementAt(2)).text = $"* {selectedLocation.Sell[dealIndex].cost} = {selectedLocation.Sell[dealIndex].cost * value} £";
            return deal;
        }

        void UpdateConfirmButton()
        {
            ((Label)ElementAt(2).ElementAt(0).ElementAt(1)).text = $"{MyRes.Money - BuyMoney + SellMoney} £";
            if (UIRefs.trading.ConvoyOnRoute(selectedLocationIndex))
            {
                confirmButton.RemoveFromClassList("main-button");
                confirmButton.AddToClassList("disabled-button");
                confirmButton.text = "<line-height=77%>Convoy already on route";
            }
            else if (UIRefs.trading.AvailableConvoy == 0)
            {
                confirmButton.RemoveFromClassList("main-button");
                confirmButton.AddToClassList("disabled-button");
                confirmButton.text = "No available convoy";
            }
            else if (MyRes.Money + BuyMoney + SellMoney < 0)
            {
                confirmButton.RemoveFromClassList("main-button");
                confirmButton.AddToClassList("disabled-button");
                confirmButton.text = "Not enough money";
            }
            else if (BuyCount + SellCount == 0)
            {
                confirmButton.RemoveFromClassList("main-button");
                confirmButton.AddToClassList("disabled-button");
                confirmButton.text = "Nothing selected";
            }
            else
            {
                confirmButton.RemoveFromClassList("disabled-button");
                confirmButton.AddToClassList("main-button");
                confirmButton.text = "Commit trade";
            }
        }
        #endregion

        #region Commit
        void TradeCommit()
        {
            if (confirmButton.ClassListContains("main-button"))
            {
                Slider slider = (Slider)ToolkitUtils.GetRoot(this).Q<TradeMap>("Map").ElementAt(0).ElementAt(0).ElementAt(selectedLocationIndex).ElementAt(0);
                UIRefs.trading.Trade(
                    new TradeConvoy(
                        GetTradeResources(selectedLocation.Buy, 0),
                        SellMoney,
                        selectedLocationIndex,
                        slider.highValue),
                    GetTradeResources(selectedLocation.Sell, 1),
                    BuyMoney);
                Open(selectedLocationIndex);

                ((Label)parent.parent.ElementAt(1).ElementAt(1).ElementAt(0)).text = $"{UIRefs.trading.AvailableConvoy}/{UIRefs.trading.maxConvoy} Convoyes";
                slider.RemoveFromClassList("free");
                slider.AddToClassList("trading");
            }
        }

        Resource GetTradeResources(List<TradeDeal> deals, int category)
        {
            Resource resource = new();
            VisualElement dealsElem = ElementAt(category).ElementAt(1);
            for (int i = 0; i < deals.Count; i++)
            {
                resource.ManageSimple(
                    deals[i].type, 
                    ((SliderInt)dealsElem.ElementAt(i).ElementAt(0).ElementAt(1)).value,
                    true);
            }
            return resource;
        }
        #endregion
    }
}
