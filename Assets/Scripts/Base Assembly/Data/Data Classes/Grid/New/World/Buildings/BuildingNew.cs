using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings
{
    [Serializable]
    public struct BuildingRenderData
    {
        /// <summary>
        /// 00 - 09 - x             0x3FF<br/>
        /// 10 - 19 - z             0x3FF<br/>
        /// 20 - 22 - y             0x7  <br/>
        /// 23 - 24 - y (rotation)  0x3  <br/>
        /// 25 - 31 - construction progress<br/>
        /// </summary>
        public uint position;
        /// <summary>
        /// 00 - 03 - x
        /// 04 - 07 - y
        /// </summary>
        public byte blueprintSize;
        public uint meshIndex;
    }

    [Serializable]
    public class BuildingNew
    {
        protected int indexInList;

        public BuildingRenderData renderData;

        public bool IsConstructing = false;
        public bool IsDeconstructing = false;

        public List<int> resources;
        public int renderIndex;



        /// <summary>
        /// In playmode use index, but when saving and loading switch for indexes. <b/>
        /// Allows reodering building in the register and keeping an optimal search time in the playmode
        /// </summary>
        public byte categoryIndex;
        public byte wrapperIndex;

        public virtual void CopyFrom(BuildingNew prev)
        {
        }

        public virtual BuildingNew Clone()
        {
            BuildingNew building = new();
            building.categoryIndex = categoryIndex;
            building.wrapperIndex = wrapperIndex;

            return building;
        }
    }


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
