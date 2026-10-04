using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings
{

    [Serializable]
    public class BuildingDataNew
    {
        protected int indexInList;

        /// <summary>
        /// 00 - 09 - x             <br/>
        /// 10 - 19 - z             <br/>
        /// 20 - 22 - y             <br/>
        /// 23 - 24 - y (rotation)  <br/>
        /// 25 - 31 - construction progress<br/>
        /// </summary>
        public uint position;
        public uint meshIndex;
        public bool IsConstructing;
        public bool IsDeconstructing;

        public List<int> resources;
        public int renderIndex;
    }


    [Serializable]
    public class ElevatorData : BuildingDataNew
    {
        const int BASE_TEST_STORAGE = 50;
        public List<bool> canStore;
        public bool isMain;

        public void CreateMainElevator(int x, int y)
        {
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
