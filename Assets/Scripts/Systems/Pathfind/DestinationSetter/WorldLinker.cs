using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.Pathfind.DestinationSetter
{
    public class WorldLinker : MonoBehaviour, IWorldInit
    {
        public void Init(ref WorldComponent worldComponent, ref NativeArray<LevelComponent> levelArray)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ent = manager.CreateEntity();

            manager.AddComponentData(ent, worldComponent);

            var buffer = manager.AddBuffer<LevelComponent>(ent);
            buffer.AddRange(levelArray);
        }
    }
}
