using System.Collections.Generic;
using AbstractControls;
using UnityEngine.UIElements;

namespace BuildMenu
{
    [UxmlElement]
    public partial class BuildMenu : VisualElement, IInitiableUI
    {
        BuildingData buildingData;
        BuildButtonList buildingList;
        BuildCategoryGroup categGroup;
        int prevOpen;

        public BuildMenu(){}

        public void Init()
        {
            buildingData = SceneRefs.objectFactory.buildPrefabs;
            
            buildingList = new(BlueprintChange);
            Add(buildingList);
            
            categGroup = new();
            categGroup.SetChangeCallback(CategChange);
            Add(categGroup);

            SceneRefs.gridTiles.DeselectBuildingButton = () => buildingList.Select(-1);
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
    }

}
