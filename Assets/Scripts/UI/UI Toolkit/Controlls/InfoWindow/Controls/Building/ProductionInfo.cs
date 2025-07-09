using InfoWindowElements;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowViews
{
    /// <summary>Control for displaying info about production.</summary>
    [UxmlElement]
    public partial class ProductionInfo : InfoWindowControl
    {
        #region Variables
        bool enable;
        [UxmlAttribute]
        bool Play
        {
            get => enable;
            set
            {
                enable = value;
                UpdateButton();
            }
        }
        #region Styles
        Color fillColor;
        [UxmlAttribute]
        Color FillColor
        {
            get => fillColor;
            set
            {
                fillColor = value;
                if (radialElement != null)
                    radialElement.fillColor = fillColor;
            }
        }
        [UxmlAttribute] Texture2D pauseTex { get; set; }
        [UxmlAttribute] Texture2D resumeTex { get; set; }
        #endregion
        [UxmlAttribute] VisualTreeAsset resourcePrefab;


        #region References
        DoubleResList inputResource;
        DoubleResList outputResource;

        Label capacityLabel;
        RadialFillElement radialElement;
        Button button;
        Label prodSpeedLabel;
        #endregion

        IProduction building;
        #endregion

        #region Constructors
        public ProductionInfo()
        {
            style.flexDirection = FlexDirection.Row;
            style.maxHeight = new Length(37.5f, LengthUnit.Percent);

            inputResource = new(true, "Input", true);
            Add(inputResource);

            VisualElement visualElement = new()
            {
                style =
                {
                    flexGrow = 1,
                    maxWidth = new Length(30, LengthUnit.Percent),
                }
            };
            Add(visualElement);

            capacityLabel = new("Space:\n##/##");
            capacityLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            capacityLabel.style.fontSize = 15;
            capacityLabel.AddToClassList("no-space-around");
            visualElement.Add(capacityLabel);

            #region Border
            radialElement = new();
            visualElement.Add(radialElement);
            #endregion

            #region Button
            button = new(ButtonClick);
            button.AddToClassList("production-button");
            button.AddToClassList("no-space-around");
            radialElement.ElementAt(0).Add(button);
            #endregion

            prodSpeedLabel = new("Speed:\n##x");
            prodSpeedLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            prodSpeedLabel.style.fontSize = 15;
            prodSpeedLabel.AddToClassList("no-space-around");
            visualElement.Add(prodSpeedLabel);

            outputResource = new(false, "Output", true) { leftPadding = 0 };
            Add(outputResource);
            UpdateButton();
        }
        #endregion

        /// <inheritdoc/>
        public override void Open(object data)
        {
            // DEBUG_Binding
            building = (IProduction)data;
            inputResource.Open(data);
            outputResource.Open(data);
            enable = building.Stoped;
            radialElement.Open(data);

            DataBinding binding;
            if (building is WaterPump)
            {
                binding = BindingUtil.CreateBinding(nameof(WaterPump.StoredFluids));
                binding.sourceToUiConverters.AddConverter((ref Fluid fluid) => $"Space\n{fluid.ammounts.Sum()}/{fluid.capacities[0]}");
                SceneRefs.infoWindow.RegisterTempBinding(new(capacityLabel, "text"), binding, data);
            }
            else
            {
                binding = BindingUtil.CreateBinding(nameof(Building.LocalRes));
                binding.sourceToUiConverters.AddConverter((ref StorageResource res) => $"Space\n{res.Sum()}/{res.capacity}");
                SceneRefs.infoWindow.RegisterTempBinding(new(capacityLabel, "text"), binding, data);
            }

            binding = BindingUtil.CreateBinding(nameof(IProduction.ProdSpeed));
            binding.sourceToUiConverters.AddConverter((ref ModifiableFloat speed) => $"Speed\n{speed}x");
            SceneRefs.infoWindow.RegisterTempBinding(new(prodSpeedLabel, "text"), binding, data);
            UpdateButton();
        }

        /// <summary>Handles the button click.</summary>
        void ButtonClick()
        {
            enable = building.StopProduction();
            UpdateButton();
        }

        /// <summary>Changes the button texture image.</summary>
        void UpdateButton()
        {
            button.iconImage = Play ? resumeTex : pauseTex; // Resources.Load<Texture2D>("Icon/Pause");
        }
    }

}