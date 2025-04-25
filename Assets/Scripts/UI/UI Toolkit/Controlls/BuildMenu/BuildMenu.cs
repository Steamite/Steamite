using System.Collections.Generic;
using AbstractControls;
using UnityEngine.UIElements;

namespace BuildMenu
{
    [UxmlElement]
    public partial class BuildMenu : VisualElement, IInitiableUI
    {
        BuildingData buildingData;
        BuildCategoryGroup categGroup;
        BuildButtonList buildingList;
        int prevOpen;

        public BuildMenu(){}

        public void Init()
        {
            buildingData = SceneRefs.objectFactory.buildPrefabs;
            categGroup = new();
            categGroup.SetChangeCallback(CategChange);
            Add(categGroup);

            buildingList = new(BlueprintChange);
            Add(buildingList);
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
