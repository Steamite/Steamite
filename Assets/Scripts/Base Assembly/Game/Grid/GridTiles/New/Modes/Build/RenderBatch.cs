using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Systems.Rendering.Buildings
{

    public class RenderBatch : IDisposable
    {
        public Mesh mesh;

        public RenderParams renderParams;
        public MaterialPropertyBlock propBlock;

        public GraphicsBuffer instaceBuffer;
        public NativeArray<uint> instanceData;

        public int startIndex;
        public int size;
        /// <summary>
        /// DO NOT SET IN CONSTRUCTOR, let the init loop set this.
        /// </summary>
        public int numberOfItems;

        public void Dispose()
        {
            instaceBuffer.Dispose();
            instanceData.Dispose();
        }

        public void UpdateInstanceData(int start = 0)
        {
            instaceBuffer.SetData(instanceData, start, start, size - start);
        }

        public RenderBatch(Mesh mesh, Material material, int itemCount, int startIndex, int groupIndex)
        {
            propBlock = new();

            this.mesh = mesh;
            this.startIndex = startIndex;

            renderParams = new(new(material))
            {
                matProps = propBlock,
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000)
            };

            numberOfItems = 0;
            size = math.max(itemCount, 10);

            instanceData = new NativeArray<uint>(size, Allocator.Persistent);

            instaceBuffer = new GraphicsBuffer(Target.Structured, size, sizeof(uint));
            propBlock.SetBuffer("_BuildingInstanceBuffer", instaceBuffer);

            propBlock.SetInt("_GroupIndex", groupIndex);
            propBlock.SetInt("_Offset", startIndex);

            /*argBuffer = new (GraphicsBuffer.Target.IndirectArguments, 1, 20);

            args.indexCountPerInstance = mesh.GetIndexCount(0);
            args.instanceCount = numberOfItems;
            args.startIndex = mesh.GetIndexStart(0);
            args.baseVertexIndex = mesh.GetBaseVertex(0);

            argBuffer.SetData(args);*/
        }

        /// <summary>
        /// Blueprint init
        /// </summary>
        public RenderBatch()
        {
            size = 1;
            startIndex = 0;

            propBlock = new();

            instanceData = new NativeArray<uint>(1, Allocator.Persistent);

            instaceBuffer = new GraphicsBuffer(Target.Structured, size, sizeof(uint));
            instaceBuffer.SetData(instanceData);

            propBlock.SetBuffer("_BuildingInstanceBuffer", instaceBuffer);

            propBlock.SetInt("_GroupIndex", 0);
            propBlock.SetInt("_Offset", startIndex);
        }

        public void SetBlueprint(Mesh mesh, Material blueprintMaterial)
        {
            this.mesh = mesh;

            renderParams = new(blueprintMaterial)
            {
                matProps = propBlock,
                worldBounds = new(Vector3.zero, Vector3.one * 10000)
            };
            numberOfItems = 1;
        }

        public void ClearBlueprint()
        {
            numberOfItems = 0;
        }
    }
}
