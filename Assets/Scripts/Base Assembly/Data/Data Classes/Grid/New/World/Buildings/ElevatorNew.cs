using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings
{
    [Serializable]
    public class ElevatorNew : BuildingNew
    {
        const int BASE_TEST_STORAGE = 50;
        public List<bool> canStore;
        public bool isMain;

        // Resource

        public void CreateMainElevator(int centerPos)
        {
            renderData.position = (uint)(centerPos & 0x3FF | ((centerPos & 0x3FF) << 10)); 
            IsConstructing = false;
            IsDeconstructing = false;
            isMain = true;
            int i = 1;
            canStore = new();
            resources = new();

            foreach (var categ in ResFluidTypes.FullRes.Skip(1).SkipLast(1))
            {
                foreach (var obj in categ.Objects)
                {
                    resources.Add(ResourceExtensions.CreateResource(50, categ.id, obj.id));
                    canStore.Add(true);
                    i++;
                }
            }
        }
    }
}
