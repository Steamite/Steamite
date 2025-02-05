using AbstractControls;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class TradeMap : Map, IToolkitController, IUIElement
    {
        #region Textures
        Texture2D locationImg;
        [UxmlAttribute]
        Texture2D locationImage
        {
            get => locationImg;
            set
            {
                locationImg = value;
                if (mapElem != null)
                {
                    mapElem.ElementAt(1).Children().ToList().ForEach(q => q.style.backgroundImage = value);
                }
            }
        }
        #endregion
#if UNITY_EDITOR_WIN
        public TradeMap() : base()
        {
            UIRefs.SetTrade();
            Init(null);
            ((Slider)ElementAt(0).ElementAt(0).ElementAt(2).ElementAt(0)).value = 300;
        }
#endif
        List<LocationButton> locationButtons = new();
        VisualElement sliderGroup;

        #region Init
        /// <summary>
        /// Initialization for the map, creates needed elements on the map.
        /// </summary>
        public void Init(VisualElement root)
        {
            CreateSliders();
            CreateLocations();
        }

        /// <summary>Creates and alligns all sliders.</summary>
        void CreateSliders()
        {
            sliderGroup = new();
            sliderGroup.name = "Sliders";
            mapElem.Add(sliderGroup);
            Slider slider;
            Vector2 basePos = UIRefs.trading.colonyLocation.pos.ToVecUI();
            for (int i = 0; i < UIRefs.trading.tradeLocations.Count; i++)
            {
                Vector2 locationPos = UIRefs.trading.tradeLocations[i].pos.ToVecUI();

                float distance = Vector2.Distance(basePos, locationPos);
                slider = new("", 0, distance);
                slider.style.width = distance;
                slider.style.height = 50;
                slider.focusable = false;
                slider.fill = true;


                Vector2 dif = locationPos - basePos;
                float f = Mathf.Atan(dif.y / dif.x);
                slider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, dif.x > 0 ? (180 * f / Mathf.PI) : -180 + (180 * f / Mathf.PI)));

                VisualElement el = new();
                sliderGroup.Add(el);
                el.Add(slider);
            }
        }

        /// <summary>Creates a location button for all <see cref="TradeLocation"/>s and the <see cref="ColonyLocation"/>.</summary>
        void CreateLocations()
        {
            TradeButtonGroup locationGroup = new(this);
            mapElem.Add(locationGroup);

            LocationButton locationButton;
            for (int i = -1; i < UIRefs.trading.tradeLocations.Count; i++)
            {
                if (i == -1)
                {
                    locationButton = new(UIRefs.trading.colonyLocation.pos.ToVecUI(), i);
                    locationButton.style.unityBackgroundImageTintColor = Color.blue;
                }
                else
                    locationButton = new TradeLocationButton(
                        UIRefs.trading.tradeLocations[i].pos.ToVecUI(), 
                        i+1, 
                        (Slider)sliderGroup.ElementAt(i).ElementAt(0),
                        UIRefs.trading.colonyLocation.pos.ToVecUI());


                locationButton.style.backgroundImage = locationImg;

                locationGroup.Add(locationButton);
                locationButtons.Add(locationButton);

                locationButton.RecalculateLayout(zoom);
            }
        }
        #endregion

        #region Updates

        public void Open(object data)
        {
            //TODO: NEED TO MOVE TRADE SLIDERS
            ((TradeLocationButton)ElementAt(0).ElementAt(1).ElementAt(1)).SelectWithoutTransition(true);
            ((Label)parent.ElementAt(1).ElementAt(0)).text = $"{UIRefs.trading.AvailableConvoy}/{UIRefs.trading.maxConvoy} Convoyes";
            foreach(TradeConvoy tradeConvoy in (List<TradeConvoy>)data)
            {
                ((Slider)ElementAt(0).ElementAt(0).ElementAt(tradeConvoy.tradeLocation).ElementAt(0)).value = tradeConvoy.currentprogress;
            }
            
            EnableInput();
        }

        protected override bool ZoomMap(WheelEvent wheelEvent)
        {
            if (base.ZoomMap(wheelEvent))
            {
                for (int i = 0; i < locationButtons.Count; i++)
                {
                    locationButtons[i].RecalculateLayout(zoom);
                }
                return true;
            }
            return false;
        }

        #endregion

    }
}