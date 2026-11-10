using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering
{

    [BurstCompile]
    public static class DirtyGridUtils
    {
        // TODO: create a list of all changes to then iterate though and update things like paths and jobs!!!
        public static void SetDirty(NativeBitArray array, int dimScaled, int dim, int tileIndex, int range = 1)
        {
            int y = tileIndex / dimScaled;
            int x = tileIndex % dimScaled;

            int2 chunkPos = new
                int2(x, y) / GridLevel.chunkDimensions;

            int chunkIndex = (chunkPos.y * dim) + chunkPos.x;

            // neighbours 
            int2 offset;
            offset.y = CheckNeighbours(y, chunkPos.y, range, chunkIndex, dim, dim, array);
            offset.x = CheckNeighbours(x, chunkPos.x, range, chunkIndex, 1, dim, array);
            if (range > 1 && offset.x != 0 && offset.y != 0)
            {
                int tempIndex = chunkIndex + offset.x + offset.y * dim;
                MarkDirty(array, tempIndex);
            }
            MarkDirty(array, chunkIndex);
        }

        static int CheckNeighbours(int toModulo, int axisPosition, int range, int chunkIndex, int chunkIndexOffset, int dim, NativeBitArray array)
        {
            toModulo %= GridLevel.chunkDimensions;

            if (toModulo < range)
            {
                if (axisPosition > 0)
                {
                    MarkDirty(array, chunkIndex - chunkIndexOffset);
                    return -1;
                }
            }
            else if (toModulo + range >= GridLevel.chunkDimensions)
            {
                if (axisPosition < dim - 1)
                {
                    MarkDirty(array, chunkIndex + chunkIndexOffset);
                    return 1;
                }
            }
            return 0;
        }


        private static void MarkDirty(NativeBitArray dirty, int chunkIndex)
        {
            dirty.Set(chunkIndex, true);
        }
    }
}