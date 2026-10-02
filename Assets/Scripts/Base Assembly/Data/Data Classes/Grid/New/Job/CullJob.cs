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
        public float2 lastPosition;
        public float2 cameraPosition;
        public float2 cameraSize;

        public void Execute(int index)
        {
            if(math.abs(lastPosition.x - cameraPosition.x) < cameraSize.x && math.abs(lastPosition.y - cameraPosition.y) < cameraSize.y)
            {
                toRender[toRenderCount.Value] = index;
                toRenderCount.Value++;
            }

            lastPosition.x++;
            if (lastPosition.x == width)
            {
                lastPosition.x = 0;
                lastPosition.y++;
            }
        }
    }
}
