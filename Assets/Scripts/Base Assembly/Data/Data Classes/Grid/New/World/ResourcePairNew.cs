using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    class ResourceNew
    {
        /// <summary>
        /// 0 - 23 - ammount
        /// 24 - 27 - type - category
        /// 28 - 31 - type - id
        /// </summary>
        List<int> resources;
    }

    public static partial class ResourceExtensions
    {
        public static bool IsSameType(this int a, int b)
            => (((a ^ b) << 24) & 0xFF) == 0;

        public static int CreateResource(int ammount, int categ, int id)
            => ammount & 0xFFFFFF | ((categ & 0xF) >> 24) | ((id & 0xF) >> 28);
    }

    public static partial class PositionExtansions
    {
        /// <inheritdoc cref="BuildingNew.position"/>
        public static int CreatePosionRotation(int x, int y, int z, int rot, int meshId)
            => x & 0x3FF | ((z & 0x3FF) >> 10) | ((y & 0x7) >> 20) | ((rot & 0x3) >> 22) | ((meshId & 0x7F) >> 25);
    }
}
