using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans
{
    public class PathfindingTest : MonoBehaviour
    {
        [SerializeField]Vector3 offset = new(0.5f, 0, 0.5f);
        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(start + offset, 0.5f);
            Gizmos.DrawSphere(end + offset, 0.5f);


            if (!positions.IsCreated)
                return;

            for (int i = 0; i < positions.Length - 1; i++)
            {
                Gizmos.DrawLine(positions[i], positions[i + 1]);
            }
        }

        [SerializeField] Vector3Int start;
        [SerializeField] Vector3Int end;

        [SerializeField] GridWorld world;

        NativeArray<Vector3> positions;
        [ContextMenu("TestPath")]
        void TestPath()
        {
            int width = world.CurrentLevel.levelDimensionsScaled;

            NativeList<int> tempPos = new(Allocator.TempJob);

            NativeArray<int> offsets = new NativeArray<int>(4, Allocator.TempJob);
            offsets[0] = 1;
            offsets[1] = width;
            offsets[2] = -1;
            offsets[3] = -width;

            int2 offsetStart = new(
                    start.x - world.CurrentLevel.levelOffsetScaled, 
                    start.z - world.CurrentLevel.levelOffsetScaled);

            int2 offsetEnd = new(
                    end.x - world.CurrentLevel.levelOffsetScaled,
                    end.z - world.CurrentLevel.levelOffsetScaled);

            PathfindingNew pathfindingNew = new()
            {
                start = offsetStart.y * width + offsetStart.x,
                destination = offsetEnd.y * width + offsetEnd.x,
                grids = world.CurrentLevel.renderGrid,
                gridWidth = world.CurrentLevel.levelDimensionsScaled,
                offsetTable = offsets,
                positions = tempPos
            };

            pathfindingNew.Run();
            if (positions.IsCreated)
                positions.Dispose();
            positions = new(tempPos.Count, Allocator.Persistent);


            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = new Vector3(
                    tempPos[i] % width + world.CurrentLevel.levelOffsetScaled, 
                    1, 
                    tempPos[i] / width + world.CurrentLevel.levelOffsetScaled) + offset;
            }

            tempPos.Dispose();
            offsets.Dispose();
        }

        private void OnDestroy()
        {
            if(positions.IsCreated)
                positions.Dispose();
        }
    }
}
