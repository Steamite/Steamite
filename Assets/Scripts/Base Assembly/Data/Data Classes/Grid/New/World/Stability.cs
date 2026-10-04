using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    public class Stability
    {
        NativeArray<MovementAndStabilityData> stability;

        public Stability(NativeArray<MovementAndStabilityData> stabilityDatas)
        {
            stability = stabilityDatas;
        }
    }
}
