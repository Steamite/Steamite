using InfoWindowElements;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleFluidList : DoubleResourceList<Fluid, FluidType>
    {
        public DoubleFluidList() : base() { }
        public DoubleFluidList(bool _cost, string _name, bool _useBindings = false)
            : base(_cost, _name, _useBindings)
        { }

        public override void Open(object data)
        {
            DataBinding mainBinding = null;
            switch (data)
            {
                case FluidResProductionBuilding fluidRes:
                    if (cost)
                        mainBinding = SetupResTypes(
                            fluidRes.FluidCost,
                            nameof(FluidResProductionBuilding.FluidCost),
                            nameof(FluidResProductionBuilding.StoredFluids),
                            data);
                    else
                        mainBinding = SetupResTypes(
                            fluidRes.FluidYeild,
                            nameof(FluidResProductionBuilding.FluidYeild),
                            nameof(FluidResProductionBuilding.StoredFluids),
                            data);
                    mainBinding.sourceToUiConverters.AddConverter((ref Fluid fluid) => ToUIRes(fluid));
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