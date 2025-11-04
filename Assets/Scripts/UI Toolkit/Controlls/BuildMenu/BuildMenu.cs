using ResearchUI;
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

        /// <summary>
        /// Inits the
        /// </summary>
        public void Init()
        {
            buildingData = SceneRefs.ObjectFactory.buildPrefabs;
            foreach (var item in buildingData.Categories)
            {
                foreach (var wrapper in item.Objects)
                {
                    wrapper.building.Cost.Init();
                    wrapper.unlocked = true;
                }
            }

            ResearchData researchData = UIRefs.ResearchWindow.researchData;
            foreach (var categ in researchData.Categories)
            {
                foreach (var node in categ.Objects)
                {
                    if (node.nodeType == NodeType.Building && node.objectConnection.categoryId > 0 && node.objectConnection.objectId > 0 && node.researched == false)
                    {
                        BuildCategWrapper buildCateg = buildingData.GetCategByID(node.objectConnection.categoryId);
                        int i = buildCateg.Objects.FindIndex(q => q.id == node.objectConnection.objectId);
                        if (i != -1)
                        {
                            BuildingWrapper wrapper = buildCateg.Objects[i];
                            if (wrapper != null)
                            {
                                node.RegisterFinishCallback(() =>
                                {
                                    wrapper.unlocked = true;
                                    if (buildingData.Categories[categGroup.SelectedChoice].id == node.objectConnection.categoryId)
                                    {
                                        buildingList.UnlockActiveButton(i);
                                    }
                                });
                                wrapper.unlocked = false;
                            }
                        }
                    }
                }
            }


            buildingList = new(BlueprintChange);
            Add(buildingList);

            categGroup = new();
            categGroup.SetChangeCallback(CategChange);
            Add(categGroup);

            SceneRefs.GridTiles.DeselectBuildingButton = () => buildingList.Select(-1);
            style.display = DisplayStyle.None;
            opened = false;
        }

        void CategChange(int i)
        {
            if (i == -1)
                buildingList.SetItemSource(null);
            else
                buildingList.SetItemSource(buildingData.Categories[i].Objects);
        }

        void BlueprintChange(int i)
        {
            if (i > -1)
            {
                BuildingWrapper wrapper = buildingData.Categories[categGroup.SelectedChoice].Objects[i];
                if (wrapper.unlocked && MyRes.CanAfford(wrapper.building.Cost))
                    SceneRefs.GridTiles.BuildPrefab = wrapper.building;
            }
            else
                SceneRefs.GridTiles.BuildPrefab = null;
        }

        public void Open()
        {
            opened = true;
            style.display = DisplayStyle.Flex;
        }

        void Close()
        {
            opened = false;
            style.display = DisplayStyle.None;
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
