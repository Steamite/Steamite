using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
   // [BurstCompile]
    public struct CountJob : IJobFor
    {
        [ReadOnly] public NativeArray<LevelGridTile> gridData;
        [NativeDisableParallelForRestriction] public NativeArray<int> vertexes;
        [NativeDisableParallelForRestriction] public NativeArray<int> triangles;

        [ReadOnly] public int chunkWidth;
        [ReadOnly] public int paddedWidth;
        public void Execute(int index)
        {
            int dataIndex = index + paddedWidth + 1 + (index / (chunkWidth)) * 2;
            LevelGridTile b = gridData[dataIndex];

            int type = b.Type;
/*

            if (type == (int)TileType.Building)
            {
                vertexes[index] = 0;
                triangles[index] = 0;
                return;
            }*/

            CreateTile(index);

            if (type == 1)
                CreateOre(index, b.Style);
            else if (type == 2)
                DoDown(index, dataIndex);
        }

        void CreateTile(int index)
        {
            vertexes[index] = 4;
            triangles[index] = 6;
        }

        void DoDown(int index, int dataIndex)
        {
            CheckWall(index, dataIndex - 1);
            CheckWall(index, dataIndex + 1);
            CheckWall(index, dataIndex + paddedWidth);
            CheckWall(index, dataIndex - paddedWidth);
        }

        void CheckWall(int index, int NeighbourDataIndex)
        {
            if (gridData[NeighbourDataIndex].Type < 2)
            {
                vertexes[index] += 4;
                triangles[index] += 6;
            }
        }

        void CreateOre(int index, int style)
        {
            if (style == 0)
                return;

            TreeOre(index);
            
            //SizeOre(index, style);
        }

        void TreeOre(int index)
        {
            vertexes[index] += 9;
            triangles[index] += 9;
        }
        /*void SizeOre(int index, int count)
        {
            vertexes[index] += count * 5;// * 2 + 1;
            triangles[index] += count * 9;
        }*/
    }
}
