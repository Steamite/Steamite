using Assets.Scripts.Systems.Pathing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class DestinationManager : MonoBehaviour, IMousePosSetter
{
    Entity entity;
    private void Start()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entity = manager.CreateEntity();
        manager.AddComponent<DestinationComponent>(entity);
    }

    public void SetPos(int2 worldPos)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var world = SceneRefs.World;

        int2 offsetStart = worldPos;

        manager.SetComponentData<DestinationComponent>(entity, new()
        {
            pos = offsetStart.y * world.LargestLevelDimensionScaled + offsetStart.x,
            level = world.CurrentLevelIndex
        });


        ref var state = ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<PathfindSystem>();
        state.Enabled = true;
    }
}