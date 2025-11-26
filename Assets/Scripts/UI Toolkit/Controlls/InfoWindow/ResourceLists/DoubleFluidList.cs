using System.Net.Sockets;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleFluidList : DoubleResourceList
    {
        public DoubleFluidList() : base() { }
        public DoubleFluidList(bool _cost, string _name, bool _useBindings = false, bool center = false)
            : base(_cost, _name, _useBindings, center)
        { }

        public override void Open(object data)
        {
            DataBinding mainBinding = null;
            switch (data)
            {
                case FluidResProductionBuilding fluidRes:
                    if (cost)
                    {
                        if(fluidRes is NeedSourceProduction source && source.Source is Water)
                        {
                            mainBinding = SetupResTypes(source.FluidYeild, nameof(NeedSourceProduction.Source) + "."+ nameof(NeedSourceProduction.Source.Storing));
                            mainBinding.sourceToUiConverters.AddConverter((ref Resource fluid) => ToUIRes(fluid));
                            SceneRefs.InfoWindow.RegisterTempBinding(new(this, nameof(resources)), mainBinding, data);
                            return;
                        }
                        else
                        {
                            mainBinding = SetupResTypes(
                                fluidRes.FluidCost,
                                nameof(FluidResProductionBuilding.FluidCost),
                                nameof(FluidResProductionBuilding.InputFluid),
                                data);
                        }
                    }
                    else
                    {
                        if(fluidRes.FluidYeild.Sum() > 0)
                        {
                            mainBinding = SetupResTypes(
                                fluidRes.FluidYeild,
                                nameof(FluidResProductionBuilding.FluidYeild),
                                nameof(FluidResProductionBuilding.StoredFluids),
                                data);
                        }
                        else
                        {
                            style.display = DisplayStyle.None;
                            return;
                        }
                    }
                    mainBinding.sourceToUiConverters.AddConverter((ref CapacityResource fluid) => ToUIRes(fluid));
                    break;
                case Water water:
                    noneLabel.dataSource = water;
                    noneLabel.style.height = new Length(50, LengthUnit.Pixel);
                    DataBinding binding = BindingUtil.CreateBinding(nameof(Water.Storing));
                    binding.sourceToUiConverters.AddConverter((ref int amm) => $"Water Source:\n {amm}/1");
                    SceneRefs.InfoWindow.RegisterTempBinding(new BindingContext(noneLabel, "text"), binding, water);
                    return;
                default:
                    style.display = DisplayStyle.None;
                    return;
            }
            SceneRefs.InfoWindow.RegisterTempBinding(new(this, nameof(resources)), mainBinding, data);

        }
    }
}