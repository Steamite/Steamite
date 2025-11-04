using AbstractControls;
using Outposts;
using System.Collections.Generic;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class TradeMap : Map, IInitiableUI, IFullScreenWindowElem
    {
        const float DISTANCE_MOD = 2;
        List<LocationButton> locationButtons = new();
        VisualElement sliderGroup;

        Button closeButton;
        public Label convoyLabel;
        TradeButtonGroup locationGroup;
        VisualElement outpostElement;

        List<OutpostButton> outpostButtons = new();

        int index;
        public TradeMap() : base()
        {
            closeButton = new();
            closeButton.AddToClassList("close-button");
            Add(closeButton);

            VisualElement convoyElem = new() { style = { top = 0, right = new Length(7, LengthUnit.Percent), borderTopWidth = 0, borderTopLeftRadius = 0, borderTopRightRadius = 0 } };
            convoyElem.AddToClassList("small-window");

            convoyLabel = new("#/# Convoyes");
            convoyLabel.AddToClassList("convoy-label");
            convoyElem.Add(convoyLabel);
            Add(convoyElem);

            outpostElement = new();
            outpostElement.AddToClassList("outpost-group");
            Add(outpostElement);
            for (int i = 0; i < 3; i++)
            {
                outpostElement.Add(new OutpostButton());
            }
        }

        #region Init
        /// <summary>
        /// Initialization for the map, creates needed elements on the map.
        /// </summary>
        public void Init()
        {
            outpostElement.Clear();
            CreateSliders();
            CreateLocations();
            CreateOutposts();
            closeButton.clicked += UIRefs.TradingWindow.CloseWindow;
            //CreateTextAndBackButton();
        }

        /// <summary>Creates and alligns all sliders.</summary>
        void CreateSliders()
        {
            sliderGroup = new();
            sliderGroup.name = "Sliders";
            mapElem.Add(sliderGroup);
            Slider slider;
            Vector2 basePos = UIRefs.TradingWindow.colonyLocation.pos.ToVecUI();
            List<TradeConvoy> convoys = UIRefs.TradingWindow.GetConvoys();
            for (int i = 0; i < UIRefs.TradingWindow.tradeLocations.Count; i++)
            {
                Vector2 locationPos = UIRefs.TradingWindow.tradeLocations[i].pos.ToVecUI();

                float distance = Vector2.Distance(basePos, locationPos) / DISTANCE_MOD;
                UIRefs.TradingWindow.tradeLocations[i].distance = distance;

                slider = new("", 0, distance);
                slider.style.width = distance;
                slider.style.height = 50;
                slider.focusable = false;
                slider.fill = true;
                //slider.enabledSelf = false;
                int j = convoys.FindIndex(q => q.tradeLocation == i);
                if (j > -1)
                {
                    if (convoys[j].firstPhase)
                        slider.AddToClassList("trading");
                    else
                        slider.AddToClassList("retreat");
                }
                else
                    slider.AddToClassList("free");

                slider.AddToClassList("map-slider");
                Vector2 dif = locationPos - basePos;
                float f = Mathf.Atan(dif.y / dif.x);
                slider.style.rotate = Quaternion.Euler(new Vector3(0, 0, dif.x > 0 ? (180 * f / Mathf.PI) : -180 + (180 * f / Mathf.PI)));

                VisualElement el = new();
                sliderGroup.Add(el);
                el.Add(slider);
            }
        }

        /// <summary>Creates a location button for all <see cref="TradeLocation"/>s and the <see cref="ColonyLocation"/>.</summary>
        void CreateLocations()
        {
            int tradeLocationCount = UIRefs.TradingWindow.tradeLocations.Count;
            locationGroup = new(this, tradeLocationCount);
            mapElem.Add(locationGroup);

            LocationButton locationButton;
            for (index = -1; index < tradeLocationCount; index++)
            {
                Location location;
                if (index == -1)
                {
                    location = UIRefs.TradingWindow.colonyLocation;
                    locationButton = new(location.pos.ToVecUI(), 0, locationGroup);
                    locationButton.AddToClassList("colony-button");
                }
                else
                {
                    location = UIRefs.TradingWindow.tradeLocations[index];
                    locationButton = new TradeLocationButton(
                        location.pos.ToVecUI(),
                        index + 1,
                        (Slider)sliderGroup.ElementAt(index).ElementAt(0),
                        UIRefs.TradingWindow.colonyLocation.pos.ToVecUI(),
                        locationGroup);
                }
                locationButton.RegisterCallback<MouseEnterEvent>(
                    q => ToolkitUtils.localMenu.UpdateContent(location, q.target as VisualElement));
                locationButton.RegisterCallback<MouseLeaveEvent>(
                    q => ToolkitUtils.localMenu.Close());
                locationGroup.Add(locationButton);
                locationButtons.Add(locationButton);

                locationButton.RecalculateLayout(zoom);
            }
        }

        void CreateOutposts()
        {
            for (int i = 0; i < UIRefs.TradingWindow.outpostLimit; i++)
            {
                index++;
                CreateOutpost(i);
            }
        }
        #endregion

        public void CreateOutpost(int i)
        {
            OutpostButton outpostButton = new(locationGroup, index);
            outpostButtons.Add(outpostButton);
            Outpost outpost = UIRefs.TradingWindow.outposts[i];
            outpost.OnUpgrade = () => EnableOutpost(i + 1);

            if (i > 0 && !UIRefs.TradingWindow.outposts[i - 1].exists)
                outpostButton.enabledSelf = false;

            outpostButton.RegisterCallback<MouseEnterEvent>(
                q => ToolkitUtils.localMenu.UpdateContent(
                    outpost,
                    q.target as VisualElement));

            outpostButton.RegisterCallback<MouseLeaveEvent>(
                q => ToolkitUtils.localMenu.Close());

            outpostElement.Add(outpostButton);
        }

        public void EnableOutpost(int i)
        {
            outpostButtons[i].enabledSelf = true;
        }

        #region Updates

        public void Open(object data)
        {
            locationButtons[0].Select();
            convoyLabel.text = $"{UIRefs.TradingWindow.AvailableConvoy}/{UIRefs.TradingWindow.MAX_CONVOYS} Convoyes";

            Slider slider;
            foreach (TradeConvoy tradeConvoy in (List<TradeConvoy>)data)
            {
                slider = (Slider)ElementAt(0).ElementAt(0).ElementAt(tradeConvoy.tradeLocation).ElementAt(0);
                slider.value = tradeConvoy.currentprogress;
            }
            EnableInput();
        }

        protected override bool ZoomMap(WheelEvent wheelEvent)
        {
            if (base.ZoomMap(wheelEvent))
            {
                locationButtons[locationButtons.Count - 1].RegisterCallbackOnce<GeometryChangedEvent>((_) => ToolkitUtils.localMenu.Move());
                for (int i = 0; i < locationButtons.Count; i++)
                {
                    locationButtons[i].RecalculateLayout(zoom);
                }
                return true;
            }
            return false;
        }

        public void Close()
        {
            locationGroup.Select(-1);
        }

        #endregion
    }
}