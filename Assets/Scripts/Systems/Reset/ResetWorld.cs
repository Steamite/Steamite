using Assets.Scripts.Systems.HumanHiding;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.Reset
{
    public class ResetWorld : MonoBehaviour
    {
        private void OnDestroy()
        {
            foreach (var world in World.All)
            {
                if (world.IsCreated)
                {
                    var entityManager = world.EntityManager;

                    entityManager.DestroyEntity(entityManager.UniversalQuery);
                }
            }

            if (!World.DefaultGameObjectInjectionWorld.IsCreated)
                return;
            ref var spawner = ref World.DefaultGameObjectInjectionWorld.Unmanaged
                .GetExistingSystemState<HumanSpawner>();
            spawner.Enabled = true;

            ref var hider = ref World.DefaultGameObjectInjectionWorld.Unmanaged
                .GetExistingSystemState<SphereHiderSystem>();
            hider.Enabled = true;
        }
    }
}
