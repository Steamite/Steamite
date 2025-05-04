using System;
using System.Collections.Generic;
using AbstractControls;
using UnityEngine.UIElements;

namespace BottomBar.Building
{
    [UxmlElement]
    public partial class BuildMenu : VisualElement, IInitiableUI
    {
        BuildingData buildingData;
        BuildButtonList buildingList;
        BuildCategoryGroup categGroup;
        bool opened;

        public BuildMenu()
        {
            pickingMode = PickingMode.Ignore;
        }

        public void Init()
        {
            buildingData = SceneRefs.objectFactory.buildPrefabs;
            
            buildingList = new(BlueprintChange);
            Add(buildingList);
            
            categGroup = new();
            categGroup.SetChangeCallback(CategChange);
            Add(categGroup);

            SceneRefs.gridTiles.DeselectBuildingButton = () => buildingList.Select(-1);
            style.display = DisplayStyle.None;
            opened = false;
        }

        void CategChange(int i)
        {
            if(i == -1)
                buildingList.SetItemSource(null);
            else
                buildingList.SetItemSource(buildingData.Categories[i].Objects);
        }

        void BlueprintChange(int i)
        {
            if (i > -1)
            {
                SceneRefs.gridTiles.BuildPrefab = buildingData.Categories[categGroup.SelectedChoice].Objects[i].building;
            }
            else
                SceneRefs.gridTiles.BuildPrefab = null;
        }

        public void Open()
        {
            opened = true;
            style.display = DisplayStyle.Flex;
            UIRefs.bottomBar[1].style.display = DisplayStyle.None;
        }

        void Close()
        {
            opened = false;
            style.display = DisplayStyle.None;
            UIRefs.bottomBar[1].style.display = DisplayStyle.Flex;
            categGroup.Select(-1);
            BlueprintChange(-1);
        }

        public void Toggle()
        {
            if (opened)
                Close();
            else
                Open();
        }
    }

}
