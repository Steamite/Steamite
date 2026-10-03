using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public struct LevelGridTile
    {
        public int hiddenTypeStyleAmount;
        public int durability;
        public static readonly LevelGridTile OutsideWall = new LevelGridTile(0, int.MaxValue);

        public static LevelGridTile Wall(int durability) => new LevelGridTile(1 << 1, durability);
        public static readonly LevelGridTile Road = new LevelGridTile(2 << 1, 0);

        public readonly bool Hidden => (hiddenTypeStyleAmount & 1) == 1; // one bit
        /// <summary>
        /// 0 - outside wall
        /// 1 - wall
        /// 2 - road
        /// 3 - vein
        /// 4 - water
        /// 5 - building
        /// 6 - elevator
        /// 7 - empty (space for moving and building)
        /// 15 - cavein ???
        /// </summary>
        public readonly int Type => (hiddenTypeStyleAmount >> 1) & 0xF; // 4 bits => 16 types

        public readonly int Style => (hiddenTypeStyleAmount >> 5) & 0x7F; // 7 bits => 128 styles
        public readonly int Amount => (hiddenTypeStyleAmount >> 12) & 0xFFFFF; // 20 bits => 1048576 maxAmmount

        // maybe will be changed
        public readonly int Durability => durability & 0xFFFF;
        public readonly int OriginalDurability => (durability >> 16) & 0xFFFF;


        public LevelGridTile(int _hidden, int _durability)
        {
            hiddenTypeStyleAmount = _hidden;
            durability = _durability;
        }
    }
}
