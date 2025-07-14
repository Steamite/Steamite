using InfoWindowElements;
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleResList : DoubleResourceList<Resource, ResourceType>
    {
        public DoubleResList() : base() { }
        public DoubleResList(bool _cost, string _name, bool _useBindings = false)
            : base(_cost, _name, _useBindings)
        {
            style.marginTop = new Length(9, LengthUnit.Percent);
        }

        /// <inheritdoc/>
        public override void Open(object data)
        {
            DataBinding mainBinding = null;
            switch (data)
            {
                case Building:
                    Building b = (Building)data;
                    if (b.id == -1)
                    {
                        mainBinding = SetupResTypes(b.Cost, nameof(ResourceDisplay.GlobalResources));
                        mainBinding.sourceToUiConverters.AddConverter((ref MoneyResource storage) => ToUIRes(storage));
                        data = SceneRefs.BottomBar.GetComponent<ResourceDisplay>();
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

                case LevelInfo:
                    LevelInfo tab = (LevelInfo)data;
                    Resource resource = tab.LevelData.costs[tab.SelectedLevel];

                    if (cost)
                        mainBinding = SetupResTypes(resource, nameof(ResourceDisplay.GlobalResources));
                    else
                        throw new NotImplementedException();


                    mainBinding.sourceToUiConverters.AddConverter((ref Resource globalStorage) =>
                    {
                        tab.UpdateCostView();
                        return ToUIRes(globalStorage);
                    });
                    data = SceneRefs.BottomBar.GetComponent<ResourceDisplay>();
                    dataSource = data;
                    break;

                case Resource:
                    Resource res = (Resource)data;

                    if (cost && !useBindings)
                    {
                        SetResWithoutBinding(res);
                    }
                    return;
                default:
                    style.display = DisplayStyle.None;
                    return;
            }
            SceneRefs.infoWindow.RegisterTempBinding(new(this, nameof(resources)), mainBinding, data);
        }
    }
}