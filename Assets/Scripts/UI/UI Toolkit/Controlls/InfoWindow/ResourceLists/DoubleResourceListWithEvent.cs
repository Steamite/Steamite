using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleResourceListWithEvent : DoubleResList
    {
        Action<bool> onResChange;
        public override void Open(object data)
        {
            DataBinding binding = null;
            switch (data)
            {
                case Tuple<Building, Action<bool>> tup:
                    Building building = tup.Item1;
                    if (useBindings)
                    {
                        binding = SetupResTypes(building.Cost, nameof(Building.LocalRes));
                        onResChange = tup.Item2;
                        binding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage));
                        SceneRefs.infoWindow.RegisterTempBinding(new(this, nameof(resources)), binding, building);
                    }
                    else
                    {
                        SetResWithoutBinding(building.Cost);
                        return;
                    }
                    break;

            }
        }

        protected override List<UIResource<ResourceType>> ToUIRes(Resource storage)
        {
            bool canAfford = true;
            for (int i = 0; i < resources.Count; i++)
            {
                int j = storage.types.IndexOf(resources[i].type);
                if (j > -1)
                {
                    resources[i].ammount = storage.ammounts[j];
                    if (resources[i].ammount < storage.ammounts[j])
                        canAfford = false;
                }
                else
                    canAfford = false;
            }
            onResChange(canAfford);
            return resources;
        }
    }
}