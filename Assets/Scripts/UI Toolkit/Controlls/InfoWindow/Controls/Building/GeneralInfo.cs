using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Systems.Buildings;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UIElements;

namespace InfoWindowViews
{
    [UxmlElement] 
    public partial class GeneralInfo : InfoWindowControl
    {
        Label title;
        Label secText;

        public GeneralInfo() 
        {
            Add(title = new() { name = "title"});
            Add(secText = new() { name = "secText"});
        }


        public override void Open(object data)
        {
            switch(data)
            {
                case Pub pub:
                    title.text = "Increses happines of workers who live in the range.";
                    secText.text = $"Range: {pub.Range}";
                    break;
            }
        }


        int selectedIndex;
        Entity buildingEntity;

        NativeArray<BuildingRenderData> buildingRenderDatas;
        public override void Select(SelectionData data)
        {
            base.Select(data);

            selectedIndex = SceneRefs.World.currentLevel.renderGrid[data.localPosition].Amount;
            var buildings = SceneRefs.World.WorldBuildings;

            title.text = $"Building ID: {selectedIndex}";

            buildingEntity = buildings.buildingEntities[selectedIndex];
            var buildComponent = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<BuildingComponent>(buildingEntity);
            secText.text = $"{buildComponent.state}";

            buildingRenderDatas = buildings.buildings;
        }

        public override bool Update()
        {
            return buildingRenderDatas[selectedIndex].IsValid;
        }
    }
}
