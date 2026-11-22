using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Systems.Rendering.Buildings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New.Modes
{
    public class BuildingRenderManager : IDisposable
    {

        GraphicsBuffer argsBuffer;
        RenderBatch[] renderBatches;
        IndirectDrawIndexedArgs[] args;


        public BuildingRenderManager(RenderBatch[] batches, IndirectDrawIndexedArgs[] args, GraphicsBuffer argsBuffer)
        {
            this.renderBatches = batches;
            this.args = args;
            this.argsBuffer = argsBuffer;
        }

        public int RemoveBuilding(int index)
        {
            int batchIndex;
            for (batchIndex = 0; batchIndex < renderBatches.Length; batchIndex++)
            {
                if ((renderBatches[batchIndex].startIndex + renderBatches[batchIndex].numberOfItems) > index)
                    break;
            }

            // get batch
            var b = renderBatches[batchIndex];
            

            int start = index - b.startIndex;
            int end = b.numberOfItems;

            // move instances
            for (int i = start; i < end; i++)
            {
                b.instanceData[i] = b.instanceData[i + 1];
            }

            int numberOfChanges = end - start;
            b.instaceBuffer.SetData(b.instanceData, start, start, numberOfChanges);


            // update args
            renderBatches[batchIndex].numberOfItems--;
            args[batchIndex].instanceCount -= 1;

            argsBuffer.SetData(args, batchIndex, batchIndex, 1);
            return numberOfChanges;
        }


        int prefabIndex;
        public void CreateBlueprint(BuildingRenderData renderData)
        {
            prefabIndex = (int)renderData.meshIndex + 1;
            var prefab = renderBatches[prefabIndex];

            renderBatches[0].SetBlueprint(prefab.mesh);

            renderBatches[0].instanceData[0] = renderData.encodedLocalPosition;
            renderBatches[0].UpdateInstanceData();

            args[0].indexCountPerInstance = args[prefabIndex].indexCountPerInstance;
            args[0].instanceCount = 1;

            argsBuffer.SetData(args, 0, 0, 1);
        }

        public void MoveBlueprint(uint pos)
        {
            pos = Recalculation.BuildingPosToFixedBuildingPos(pos, SceneRefs.World.currentLevel.offsetsScaled);

            renderBatches[0].instanceData[0] = pos;
            renderBatches[0].UpdateInstanceData();
        }

        const int INCREASE_AMMOUNT = 5;
        public short PlaceBlueprint(bool clearBlueprint)
        {
            int dataIndex = renderBatches[prefabIndex].numberOfItems;
            renderBatches[prefabIndex].numberOfItems++;


            HandleInstanceRealoc(prefabIndex);
            args[prefabIndex].instanceCount++;

            if (clearBlueprint)
            {
                args[0].instanceCount = 0;
                argsBuffer.SetData(args, 0, 0, 1);
            }
            argsBuffer.SetData(args, prefabIndex, prefabIndex, 1);

            renderBatches[prefabIndex].instanceData[dataIndex] = renderBatches[0].instanceData[0];
            renderBatches[prefabIndex].UpdateInstanceData(dataIndex);


            return (short)(dataIndex + renderBatches[prefabIndex].startIndex);
        }

        void HandleInstanceRealoc(int index)
        {
            var batch = renderBatches[index];
            if (batch.numberOfItems == batch.size)
            {
                batch.size += INCREASE_AMMOUNT;

                NativeArray<uint> tempInstanceData = new NativeArray<uint>(
                    batch.size,
                    Allocator.Persistent,
                    NativeArrayOptions.ClearMemory);


                for (int i = 0; i < batch.instanceData.Length; i++)
                {
                    tempInstanceData[i] = batch.instanceData[i];
                }

                SceneRefs.World.WorldBuildings.UpdateRenderIndexes(renderBatches[index + 1].startIndex, INCREASE_AMMOUNT);
                // Move start Indexes
                for (int i = index + 1; i < renderBatches.Length; i++)
                {
                    renderBatches[i].startIndex += INCREASE_AMMOUNT;
                }

                batch.instanceData.Dispose();
                batch.instanceData = tempInstanceData;

                var tempBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    batch.size,
                    sizeof(uint));
                tempBuffer.SetData(tempInstanceData);

                // TODO move the building parts after the the data, and update batche data, reuse alloc function!
                batch.instaceBuffer.Dispose();
                batch.instaceBuffer = tempBuffer;
                batch.propBlock.SetBuffer("_BuildingInstanceBuffer", tempBuffer);
            }
        }

        public void DestroyBlueprint()
        {
            args[0].instanceCount = 0;
            argsBuffer.SetData(args, 0, 0, 1);

            renderBatches[0].numberOfItems = 0;
        }

        public void Dispose()
        {
            argsBuffer.Dispose();
            foreach (var item in renderBatches)
            {
                item.Dispose();
            }
        }
    }
}
