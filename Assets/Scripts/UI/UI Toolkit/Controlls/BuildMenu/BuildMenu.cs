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

        }

        void CategChange(int i)
        {
            buildingList.SetItemSource(buildingData.Categories[i].Objects);
        }

        void BlueprintChange(int i)
        {

        }
    }

}
