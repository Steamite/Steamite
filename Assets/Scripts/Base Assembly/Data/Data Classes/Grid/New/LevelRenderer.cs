using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{

    public struct OverlayData
    {
        public byte r;
        public byte g;

        public OverlayData(byte r, byte g)
        {
            this.r = r;
            this.g = g;
        }

        public OverlayData R(byte R)
        {
            return new OverlayData(R, g);
        }
        public OverlayData G(byte G)
        {
            return new OverlayData(r, G);
        }
    }



    public interface IRenderInit
    {
        public void Init(GridWorld world);
    }

    public class LevelRenderer : MonoBehaviour
    {
        [SerializeField] int2 cameraPosition = new(0, 0);
        [SerializeField] int2 cameraChunkSize;

        [SerializeField] MonoBehaviour linker;
        

        public void Awake()
        {
            enabled = false;
        }
        
        public ICustomRenderer GetCustomRenderer(int index) => customRenderers[index];

        public void Init(GridWorld world)
        {
            world.AttachChange(ChangeLevel);
            enabled = true;
        }

        public void UpdateOverlayTexture()
        {
            (linker as IOverlayUpdate).MarkDirty();
        }

        public void ChangeLevel(LevelComponent level, int oldLevel, int newLevel)
        {

            UpdateOverlayTexture();

            foreach (var item in customRenderers)
            {
                item.OnLevelChange(level, oldLevel, newLevel);
            }
        }

        private void Update()
        {
            foreach (var item in customRenderers)
            {
                item.Render(cameraPosition, cameraChunkSize);
            }
        }

        readonly List<ICustomRenderer> customRenderers = new();
        public void AddRenderer(ICustomRenderer renderer)
        {
            customRenderers.Add(renderer);
        }

    }

    public interface IOverlayUpdate
    {
        public void MarkDirty();
    }
}
