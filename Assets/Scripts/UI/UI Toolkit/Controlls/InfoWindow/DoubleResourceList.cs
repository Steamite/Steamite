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
            DataBinding binding = null;
            switch (data)
            {
                case Building:
                    Building b = (Building)data;
                    if (!b.constructed)
                    {
                        binding = SetupResTypes(b.cost, nameof(Building.LocalRes));
                    }
                    else if (data is ProductionBuilding)
                    {
                        if (cost)
                            binding = SetupResTypes(((ProductionBuilding)b).productionCost, nameof(ProductionBuilding.InputResource));
                        else
                            binding = SetupResTypes(((ProductionBuilding)b).production, nameof(ProductionBuilding.LocalRes));
                    }
                    else
                        break;
                    binding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage.stored));
                    break;

                case LevelsTab:
                    LevelsTab tab = (LevelsTab)data;
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];

                    if (cost)
                        binding = SetupResTypes(resource, nameof(ResourceDisplay.GlobalResources));
                    else
                        throw new NotImplementedException();


                    binding.sourceToUiConverters.AddConverter((ref Resource globalStorage) =>
                    {
                        tab.UpdateCostView();
                        return ToUIRes(globalStorage);
                    });
                    data = SceneRefs.stats.GetComponent<ResourceDisplay>();
                    dataSource = data;
                    break;
                default:
                    throw new NotImplementedException(data.ToString());
            }
            SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
        }

        DataBinding SetupResTypes(Resource resource, string propName)
        {
            resources = new();
            for (int i = 0; i < resource.type.Count; i++)
                resources.Add(new DoubleUIResource(
                        0,
                        resource.ammount[i],
                        resource.type[i]));
            return Util.CreateBinding(propName);
        }

        #region Convertors
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
        #endregion
    }
}

