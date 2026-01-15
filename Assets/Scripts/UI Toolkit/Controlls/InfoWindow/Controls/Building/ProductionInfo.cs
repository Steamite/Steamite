using InfoWindowElements;
using System;
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
        DoubleFluidResList inputResource;
        DoubleFluidResList outputResource;

        Label capacityLabel;
        RadialFillElement radialElement;
        Button button;
        Button changeRecipe;
        Label prodSpeedLabel;

        ResourceList storageList;
        #endregion

        IProduction building;
        #endregion

        #region Constructors
        public ProductionInfo() : base()
        {
            style.flexDirection = FlexDirection.Row;

            inputResource = new(true, "Input", true);
            Add(inputResource);

            VisualElement visualElement = new()
            {
                style =
                {
                    flexGrow = 0,
                    minWidth = 124,
                    maxWidth = 124
                }
            };
            Add(visualElement);

            changeRecipe = new() { text = "Change Recipe" };
            changeRecipe.AddToClassList("main-button");
            changeRecipe.style.width = 225;
            changeRecipe.style.maxWidth = StyleKeyword.None;
            changeRecipe.style.marginTop = 5;
            changeRecipe.style.fontSize = 25;
            changeRecipe.clicked += ChangeRecipeClicked;
            visualElement.Add(changeRecipe);

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

            VisualElement bottomTextContainer = new() { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };

            capacityLabel = new("Space:\n##/##");
            capacityLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            capacityLabel.style.fontSize = 15;
            capacityLabel.AddToClassList("no-space-around");
            bottomTextContainer.Add(capacityLabel);

            prodSpeedLabel = new("Speed:\n##x");
            prodSpeedLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            prodSpeedLabel.style.fontSize = 15;
            prodSpeedLabel.AddToClassList("no-space-around");
            bottomTextContainer.Add(prodSpeedLabel);

            visualElement.Add(bottomTextContainer);

            outputResource = new(false, "Output", true);
            Add(outputResource);
            storageList = new(_name: "Storage") { style = { display = DisplayStyle.None } };
            Add(storageList);
            UpdateButton();
        }

        void ChangeRecipeClicked()
        {
            VisualElement window = SceneRefs.InfoWindow.secondWindow;
            if (window.style.display == DisplayStyle.None)
            {
                SceneRefs.InfoWindow.CreateSecondWindow("Select Recipe", changeRecipe.worldBound);
                ListView view = new()
                {
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
                };
                view.makeItem = () =>
                {
                    VisualElement element = new();

                    VisualElement row = new() { style = { flexDirection = FlexDirection.Row } };
                    element.Add(row);
                    Label label = new("ABC") { name = "title" };
                    Button button = new() { name = "button" };
                    row.Add(label);

                    row = new() { style = { flexDirection = FlexDirection.Row } };
                    element.Add(row);
                    ResourceList cost = new(0.5f, "cost");
                    row.Add(cost);

                    VisualElement column = new() { style = { flexDirection = FlexDirection.Column } };
                    VisualElement arrow = new VisualElement();
                    column.Add(arrow);
                    Label arrowText = new Label();
                    column.Add(arrowText);
                    row.Add(column);

                    ResourceList yield = new(0.5f, "yield");
                    row.Add(yield);

                    return element;
                };
                view.bindItem = (el, i) =>
                {
                    ProductionRecipe recipe = view.itemsSource[i] as ProductionRecipe;
                    el.Q<Label>("title").text = recipe.Name;
                    el.Q<ResourceList>("cost").Open(recipe.resourceCost);
                    el.Q<ResourceList>("yield").Open(recipe.resourceYield);
                    int maxCount = Math.Max(Math.Max(recipe.resourceCost.types.Count, recipe.resourceYield.types.Count), 1);
                    el.style.height = 31 + 30 * maxCount;
                };
                view.itemsSource = (building as IResourceProduction).Recipes;
                view.selectedIndex = (building as IResourceProduction).SelectedRecipe;
                window[1].Add(view);
                view.selectionChanged += (list) =>
                {
                    //ProductionRecipe recipe = list.First() as ProductionRecipe;
                    SceneRefs.InfoWindow.CloseSecondWindow();
                    (building as IResourceProduction).SetRecipe(view.selectedIndex, true);
                };
            }
            else
            {
                SceneRefs.InfoWindow.CloseSecondWindow();
            }
        }
        #endregion

        void ToggleElems(bool prod)
        {
            if (prod)
            {
                storageList.style.display = DisplayStyle.None;
                inputResource.style.display = DisplayStyle.Flex;
                radialElement.parent.style.display = DisplayStyle.Flex;
                outputResource.style.display = DisplayStyle.Flex;
            }
            else
            {
                storageList.style.display = DisplayStyle.Flex;
                inputResource.style.display = DisplayStyle.None;
                radialElement.parent.style.display = DisplayStyle.None;
                outputResource.style.display = DisplayStyle.None;
            }
        }

        /// <inheritdoc/>
        public override void Open(object data)
        {
            if(data is NeedSourceProduction needSource)
            {
                if(needSource.ResourceCost.Sum() + needSource.FluidCost.Sum() == 0)
                {
                    ToggleElems(false);
                    storageList.Open(needSource);
                    return;
                }
            }
            ToggleElems(true);

            building = (IProduction)data;
            inputResource.Open(data);
            outputResource.Open(data);
            enable = building.Stoped;
            radialElement.Open(data);

            if(data is IResourceProduction production)
            {
                if(production.Recipes.Count <= 1)
                    changeRecipe.style.visibility = Visibility.Hidden;
            }
            else
            {
                changeRecipe.style.visibility = Visibility.Hidden;
            }

            DataBinding binding;

            if((building is NeedSourceProduction source && source.Source is Vein) || building is not FluidResProductionBuilding)
            {
                binding = BindingUtil.CreateBinding(nameof(ResourceProductionBuilding.LocalRes));
                binding.sourceToUiConverters.AddConverter((ref StorageResource resource) => $"Space\n{resource.ammounts.Sum()}/{resource.capacity}");
                SceneRefs.InfoWindow.RegisterTempBinding(new(capacityLabel, "text"), binding, data);
            }
            else
            {
                binding = BindingUtil.CreateBinding(nameof(NeedSourceProduction.StoredFluids));
                binding.sourceToUiConverters.AddConverter((ref CapacityResource fluid) => $"Space\n{fluid.ammounts.Sum()}/{fluid.capacity}");
                SceneRefs.InfoWindow.RegisterTempBinding(new(capacityLabel, "text"), binding, data);
            }

            binding = BindingUtil.CreateBinding(nameof(IProduction.ProdSpeed));
            binding.sourceToUiConverters.AddConverter((ref ModifiableFloat speed) => $"Speed\n{speed}x");
            SceneRefs.InfoWindow.RegisterTempBinding(new(prodSpeedLabel, "text"), binding, data);
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