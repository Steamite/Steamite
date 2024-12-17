using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class ProductionButton : VisualElement, IUIElement
    {
        #region Attributes
        bool enable;
        [UxmlAttribute] bool Play 
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
        [UxmlAttribute] Color FillColor
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

        #endregion
        #region References
        DoubleResourceList inputResource;
        DoubleResourceList outputResource;

        Label capacityLabel;
        RadialFillElement radialElement;
        Button button;
        #endregion
        ProductionBuilding building;

        public ProductionButton()
        {
            inputResource = new();
            inputResource.cost = true;
            Add(inputResource);

            VisualElement visualElement = new();
            visualElement.style.flexGrow = 1;
            visualElement.style.maxWidth = new(new Length(30, LengthUnit.Percent));
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

            outputResource = new();
            outputResource.cost = false;
            Add(outputResource);
            UpdateButton();
        }

        public void Fill(object data)
        {
            building = (ProductionBuilding)data;
            enable = building.pStates.stoped;
            inputResource.Fill(data);
            outputResource.Fill(data);
            radialElement.Fill(data);

            DataBinding binding = SceneRefs.infoWindow.CreateBinding(nameof(ProductionBuilding.LocalRes));
            binding.sourceToUiConverters.AddConverter((ref StorageResource res) => $"Space\n{res.stored.ammount.Sum()}/{res.stored.capacity}");
            SceneRefs.infoWindow.AddBinding(new(capacityLabel, "text"), binding, data);
            UpdateButton();
        }

        void ButtonClick()
        {
            enable = building.StopProduction();
            UpdateButton();
        }

        void UpdateButton()
        {
            button.iconImage = Play ? resumeTex : pauseTex; // Resources.Load<Texture2D>("Icon/Pause");
        }
    }

}