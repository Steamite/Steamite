using Rendering;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Systems.Rendering.Buildings
{

    [UpdateInGroup(typeof(RenderUpdateGroup)), UpdateAfter(typeof(ChunkRenderSystem))]
    public partial class BuildingRenderSystem : SystemBase
    {
        public GraphicsBuffer argBuffer;
        public IndirectDrawIndexedArgs[] args;

        /// <summary>
        /// (0) - blueprint
        /// (1 - x) - normal buildings
        /// </summary>
        public RenderBatch[] batches;

        protected override void OnCreate()
        {
            Shader.SetGlobalFloat("_CellSize", 1);
            Shader.SetGlobalFloat("_HeightY", 1);

            Enabled = false;
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < batches.Length; i++)
            {
                RenderBatch batch = batches[i];
                if (batch.numberOfItems == 0)
                    continue;
                
                Graphics.RenderMeshIndirect(batch.renderParams, batch.mesh, argBuffer, startCommand: i);
            }
        }

    }
}
