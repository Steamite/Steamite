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

            ResearchData researchData = UIRefs.research.researchData;
            foreach (var categ in researchData.Categories)
            {
                foreach (var node in categ.Objects)
                {
                    if (node.nodeType == NodeType.Building && node.nodeAssignee > -1 && node.researched == false)
                    {
                        int i = buildingData.Categories[node.nodeCategory].Objects.FindIndex(q => q.id == node.nodeAssignee);
                        if (i != -1)
                        {
                            BuildingWrapper wrapper = buildingData.Categories[node.nodeCategory].Objects[i];
                            if (wrapper != null)
                            {
                                node.RegisterFinishCallback(() =>
                                {
                                    wrapper.unlocked = true;
                                    if (categGroup.SelectedChoice == node.nodeCategory)
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
                if (buildingData.Categories[categGroup.SelectedChoice].Objects[i].unlocked)
                    SceneRefs.GridTiles.BuildPrefab = buildingData.Categories[categGroup.SelectedChoice].Objects[i].building;
            }
            else
                SceneRefs.GridTiles.BuildPrefab = null;
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
