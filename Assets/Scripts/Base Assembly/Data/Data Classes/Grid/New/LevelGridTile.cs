using Buildings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Mathematics;

namespace Grid.New
{
    public enum TileType
    {
        Border,
        Wall,
        Road,
        Empty,
        Vein,
        Water,
        Building,
        PassableBuilding,
        Elevator
    }

    /// <summary>
    /// Rocks, veins and water can store only one resource per tile.
    /// </summary>
    [BurstCompile]
    public struct LevelGridTile
    {
        /// <summary>
        /// 0       - hidden    (0x1)
        /// 1 - 4   - type      (0xF)
        /// 5 - 12  - style     (0x7F)     
        /// 13 - 32 - ammount   (0xFFFFF)
        /// </summary>
        public int hiddenTypeStyleAmount;
        /// <summary>
        /// 0-1 entryPoint Counts
        /// </summary>
        public uint otherData;
        public static readonly LevelGridTile OutsideWall = new LevelGridTile((int)TileType.Border, int.MaxValue);

        public static LevelGridTile Wall(half durability, bool hidden = true) => 
            new LevelGridTile(
                (hidden ? 1 : 0) | (int)TileType.Wall << 1, 
                (uint)((durability.value << 16) | durability.value));
        
        public static LevelGridTile Wall(int style, int ammount, half durability, bool hidden = true) 
            => new LevelGridTile(
                (hidden ? 1 : 0) | (((int)TileType.Wall & 0xF) << 1) | ((style & 0x7F) << 5) | ((ammount & 0xFFFFF) << 12),
                (uint)((durability.value << 16) | durability.value));
        
        public static readonly LevelGridTile Road = new LevelGridTile((int)TileType.Road << 1, 0);

        public readonly bool Hidden => (hiddenTypeStyleAmount & 1) == 1; // one bit

        public readonly int Type => (hiddenTypeStyleAmount >> 1) & 0xF; // 4 bits => 16 types
        public readonly TileType TType => (TileType)((hiddenTypeStyleAmount >> 1) & 0xF); // 4 bits => 16 types
        
        public readonly int Style => (hiddenTypeStyleAmount >> 5) & 0xFF; // 8 bits => 256 styles 
        /// <summary>
        /// points to a building index in the <see cref="WorldBuildings"/>
        /// </summary>
        public readonly int Amount => (hiddenTypeStyleAmount >> 13) & 0xFFFFF; // (20) bits => x^20

        public readonly float Durability => math.f16tof32(otherData & 0xFFFFu);
        public readonly float OriginalDurability => math.f16tof32((otherData >> 16) & 0xFFFFu);


        public LevelGridTile(int _hiddenStats, uint _durability)
        {
            hiddenTypeStyleAmount = _hiddenStats;

            otherData = _durability;
        }

        #region Overrides
        public override string ToString()
        {
            return hiddenTypeStyleAmount.ToString();
        }

        public override int GetHashCode()
        {
            return hiddenTypeStyleAmount.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }
        #endregion
        #region Constructors
        public static LevelGridTile Elevator(int index)
        {
            LevelGridTile tile = new(
                0 | (((int)TileType.Elevator & 0xF) << 1) | ((index & 0xFFFFF) << 13), 
                0);
            return tile;
        }

        public static LevelGridTile PassableRoad(int index)
        {
            LevelGridTile tile = new(
                0 | (((int)TileType.PassableBuilding & 0xF) << 1) | ((index & 0x3FFFF) << 13),
                0);
            return tile;
        }

        public static LevelGridTile Building(int index)
        {
            LevelGridTile tile = new((((int)TileType.Building & 0xF) << 1) | (index & 0x3FFFF) << 13, 0);
            return tile;
        }
        #endregion


        [BurstCompile]
        public float RemoveDurability(float change)
        {
            float dur = Durability - change;
            otherData = (otherData & 0xFFFF0000u) | ((half)dur).value;
            if (dur < 0)
            {
                hiddenTypeStyleAmount = (hiddenTypeStyleAmount & ~(0xF << 0x1)) | (((int)TileType.Road & 0xF) << 0x1);
            }
            return dur;
        }
    }
}
