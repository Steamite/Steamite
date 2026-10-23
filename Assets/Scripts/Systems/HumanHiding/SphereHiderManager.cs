using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.HumanHiding
{
    public class SphereHiderManager : MonoBehaviour, IAfterLoad
    {
        Entity entity;

        void OnLevelChange(int oldLevel, int newLevel)
        {
            var man = World.DefaultGameObjectInjectionWorld.EntityManager;

            man.SetComponentData<SphereHiderComponent>(entity, new()
            {
                prevLevel = oldLevel,
                currentLevel = newLevel
            });

            ref var state = ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<SphereHiderSystem>();
            state.Enabled = true;
        }

        public void AfterLoad()
        {
            var man = World.DefaultGameObjectInjectionWorld.EntityManager;
            entity = man.CreateEntity();
            man.AddComponent<SphereHiderComponent>(entity);
            man.SetComponentData<SphereHiderComponent>(entity, new() { prevLevel = -1, currentLevel = 0 });

            SceneRefs.World.AttachChange(OnLevelChange);
        }
    
    }
}
