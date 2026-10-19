using System;
using System.Collections.Generic;
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
            building.renderData = renderData;

            return building;
        }

        public void SetBindingIndex(int catIndex, int wrapIndex, uint meshIndex)
        {
            categoryIndex = (byte)catIndex;
            wrapperIndex = (byte)wrapIndex;
            renderData.meshIndex = meshIndex;
        }
    }
}
