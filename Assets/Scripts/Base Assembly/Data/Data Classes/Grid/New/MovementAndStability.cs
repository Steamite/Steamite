using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public struct MovementAndStabilityData
    {
        public int value;

        public int Stability => value & 0xFF;
        public int PathCost => (value >> 8) & 0xFF;
        public int PathNodes => (value >> 16) & 0xFF;

        public int Remaining => (value >> 22) & 0x3FF;

        public MovementAndStabilityData(int _value)
        {
            value = _value;
        }
    }

}
