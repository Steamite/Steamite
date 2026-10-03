using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{

    [BurstCompile]
    public struct CullJob : IJobFor
    {
        public NativeArray<int> toRender;
        public NativeReference<int> toRenderCount;
        public int maxSize;
        public int width;
        public int2 lastPosition;
        public int2 cameraPosition;
        public int2 cameraSize;

        public void Execute(int index)
        {
            int y = index / width;
            int x = index - (y * width);
            /*
            float2 pos = new(positions[index].x - cameraPosition.x, positions[index].y - cameraPosition.y);
            isRendered[index] =
                cameraSize.x > math.abs(pos.x) &&
                cameraSize.y > math.abs(pos.y);// ? 1 : 0;*/

            if (math.abs(x - cameraPosition.x) < cameraSize.x && math.abs(y - cameraPosition.y) < cameraSize.y)
            {
                toRender[toRenderCount.Value] = index;
                toRenderCount.Value++;
            }
        }
    }
}
