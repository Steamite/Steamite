using InfoWindowElements;
using Objectives;
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleResList : DoubleResourceList<Resource, ResourceType>
    {
        public DoubleResList() : base() { }
        public DoubleResList(bool _cost, string _name, bool _useBindings = false, bool center = false)
            : base(_cost, _name, _useBindings, center)
        {
            style.marginTop = new Length(9, LengthUnit.Percent);
        }

        /// <inheritdoc/>
        public override void Open(object data)
        {
            DataBinding mainBinding = null;
            switch (data)
            {
                case Building building:
                    if (building.id == -1)
                    {
                        mainBinding = SetupResTypes(building.Cost, nameof(ResourceDisplay.GlobalResources));
                        mainBinding.sourceToUiConverters.AddConverter((ref MoneyResource storage) => ToUIRes(storage));
                        data = SceneRefs.Stats.GetComponent<ResourceDisplay>();
                        dataSource = data;
                        SetBinding(nameof(resources), mainBinding);
                        ((IUpdatable)data).UIUpdate(nameof(ResourceDisplay.GlobalResources));
                        return;
                    }
                    else if (data is IResourceProduction prod)
                    {
                        // DEBUG_Binding example binding
                        // Creates a list that's used as itemSource, containg a static resouce and a dynamic binded resource.
                        if (cost && prod.ResourceCost.Sum() > 0)
                        {
                            mainBinding = SetupResTypes(
                                prod.ResourceCost,
                                nameof(IResourceProduction.ResourceCost),
                                nameof(IResourceProduction.InputResource),
                                data);
                        }
                        else if (!cost && prod.ResourceYield.Sum() > 0)
                        {
                            mainBinding = SetupResTypes(
                                prod.ResourceYield,
                                nameof(IResourceProduction.ResourceYield),
                                nameof(Building.LocalRes),
                                data);
                        }
                        else
                        {
                            style.display = DisplayStyle.None;
                            return;
                        }
                        mainBinding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage));
                    }
                    else
                    {
                        style.display = DisplayStyle.None;
                        return;
                    }
                    break;

                case LevelInfo tab:
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];

                    if (cost)
                        mainBinding = SetupResTypes(resource, nameof(ResourceDisplay.GlobalResources));
                    else
                        throw new NotImplementedException();


                    mainBinding.sourceToUiConverters.AddConverter((ref MoneyResource globalStorage) =>
                    {
                        tab.UpdateCostView();
                        return ToUIRes(globalStorage);
                    });
                    data = SceneRefs.Stats.GetComponent<ResourceDisplay>();
                    dataSource = data;
                    break;

                case Resource res:
                    if (cost && !useBindings)
                    {
                        SetResWithoutBinding(res);
                    }
                    return;
                case Order order:
                    SetResWithoutBinding((order.objectives[0] as ResourceObjective).resource);
                    return;
                default:
                    style.display = DisplayStyle.None;
                    return;
            }
            SceneRefs.InfoWindow.RegisterTempBinding(new(this, nameof(resources)), mainBinding, data);
        }
    }
}