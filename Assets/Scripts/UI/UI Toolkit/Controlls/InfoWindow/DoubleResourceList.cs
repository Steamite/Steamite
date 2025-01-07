using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    public class DoubleUIResource : UIResource
    {
        public int secondAmmount;
        public DoubleUIResource(int _ammount, int _secondAmmount, ResourceType _type) : base(_ammount, _type)
        {
            secondAmmount = _secondAmmount;
        }
    }

    [UxmlElement]
    public partial class DoubleResourceList : ResourceList
    {
        [UxmlAttribute] bool cost;

        ///<summary> Do not use from code, this is only for adding the resource list from UI Builder.</summary>
        public DoubleResourceList() : base()
        {
            style.marginTop = new(new Length(9, LengthUnit.Percent));
            style.maxWidth = new(new Length(35, LengthUnit.Percent));
            style.alignContent = Align.Center;
            cost = false;
        }


        public DoubleResourceList(bool _cost, string _name) : base()
        {
            style.marginTop = new(new Length(9, LengthUnit.Percent));
            style.maxWidth = new(new Length(35, LengthUnit.Percent));
            style.alignContent = Align.Center;
            cost = _cost;
            name = _name;
        }

        public override void Fill(object data)
        {
            DataBinding binding;
            switch (data)
            {
                case ProductionBuilding:
                    ProductionBuilding building = (ProductionBuilding)data;
                    resources = new();

                    if (cost)
                    {
                        for (int i = 0; i < building.productionCost.type.Count; i++)
                            resources.Add(new DoubleUIResource(
                                    0,
                                    building.productionCost.ammount[i],
                                    building.productionCost.type[i]));
                        binding = Util.CreateBinding(nameof(ProductionBuilding.InputResource));
                    }
                    else
                    {
                        for (int i = 0; i < building.production.type.Count; i++)
                            resources.Add(new DoubleUIResource(
                                    0,
                                    building.production.ammount[i],
                                    building.production.type[i]));
                        binding = Util.CreateBinding(nameof(ProductionBuilding.LocalRes));
                    }

                    binding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage.stored));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case LevelsTab:
                    LevelsTab tab = (LevelsTab)data;
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];
                    resources = new();

                    if (cost)
                    {
                        for (int i = 0; i < resource.type.Count; i++)
                            resources.Add(new DoubleUIResource(
                                    0,
                                    resource.ammount[i],
                                    resource.type[i]));
                        binding = Util.CreateBinding(nameof(ResourceDisplay.GlobalResources));
                    }
                    else
                        throw new NotImplementedException();


                    binding.sourceToUiConverters.AddConverter((ref Resource globalStorage) =>
                    {
                        tab.UpdateState();
                        return ToUIRes(globalStorage);
                    });
                    dataSource = SceneRefs.stats.GetComponent<ResourceDisplay>();
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, dataSource);
                    break;
                default:
                    throw new NotImplementedException(data.ToString());
            }
        }

        protected override List<UIResource> ToUIRes(Resource storage)
        {
            for (int i = 0; i < storage.type.Count; i++)
            {
                int j = resources.FindIndex(q => q.type == storage.type[i]);
                if (j > -1)
                    resources[j].ammount = storage.ammount[i];
            }
            return resources;
        }

        protected override string ConvertString(UIResource resource)
        {
            if (cost)
                return $"{resource.ammount}/{((DoubleUIResource)resource).secondAmmount}";
            else
                return $"{((DoubleUIResource)resource).secondAmmount}({resource.ammount})";
        }
    }
}

