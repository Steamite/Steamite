using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleResourceListWithEvent : DoubleResourceList
    {
        Action<bool> onResChange;
        public override void Open(object data)
        {
            DataBinding binding = null;
            switch (data)
            {
                case Tuple<Building, Action<bool>>:
                    Tuple<Building, Action<bool>> tup = data as Tuple<Building, Action<bool>>;
                    Building building = tup.Item1;
                    if (useBindings)
                    {
                        binding = SetupResTypes(building.Cost, nameof(Building.LocalRes));
                        onResChange = tup.Item2;
                        binding.sourceToUiConverters.AddConverter((ref StorageResource storage) => ToUIRes(storage));
                        SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, building);
                    }
                    else
                    {
                        SetResWithoutBinding(building.Cost);
                        return;
                    }
                    break;

            }
        }

        protected override List<UIResource> ToUIRes(Resource storage)
        {
            bool canAfford = true;
            for (int i = 0; i < resources.Count; i++)
            {
                int j = storage.type.IndexOf((ResourceType)resources[i].type);
                if (j > -1)
                {
                    resources[i].ammount = storage.ammount[j];
                    if (resources[i].ammount < storage.ammount[j])
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