using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public enum TileType
    {
        Boarder,
        Wall,
        Road,
        Empty,
        Vein,
        Water,
        Building,
        Elevator
    }

    /// <summary>
    /// Rocks, veins and water can store only one resource per tile.
    /// </summary>
    public struct LevelGridTile
    {
        public int hiddenTypeStyleAmount;
        public int durability;
        public static readonly LevelGridTile OutsideWall = new LevelGridTile((int)TileType.Boarder, int.MaxValue);

        public static LevelGridTile Wall(int durability) => new LevelGridTile((int)TileType.Wall << 1, durability);
        
        public static LevelGridTile Wall(int style, int ammount, int durability) 
            => new LevelGridTile((((int)TileType.Wall & 0xF) << 1) | ((style & 0x7F) << 5) | ((ammount & 0xFFFFF) << 12), durability);
        
        public static readonly LevelGridTile Road = new LevelGridTile((int)TileType.Road << 1, 0);

        public readonly bool Hidden => (hiddenTypeStyleAmount & 1) == 1; // one bit

        public readonly int Type => (hiddenTypeStyleAmount >> 1) & 0xF; // 4 bits => 16 types
        
        public readonly int Style => (hiddenTypeStyleAmount >> 5) & 0xFF; // 8 bits => 256 styles 
        /// <summary>
        /// points to a building index in the <see cref="WorldBuildings"/>
        /// </summary>
        public readonly int Amount => (hiddenTypeStyleAmount >> 13) & 0x3FFFF; // (18) bits => x^18maxAmmount

        // maybe will be changed
        public readonly int Durability => durability & 0xFFFF;
        public readonly int OriginalDurability => (durability >> 16) & 0xFFFF;


        public LevelGridTile(int _hiddenStats, int _durability)
        {
            hiddenTypeStyleAmount = _hiddenStats;
            durability = _durability;
        }

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

        public static LevelGridTile Elevator(int index)
        {
            LevelGridTile tile = new(
                0 | (((int)TileType.Building & 0xF) << 4) | ((index & 0x3FFFF) << 13), 
                0);
            return tile;
        }
    }
}
