using Pathing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class DestinationManager : MonoBehaviour, IMousePosSetter
{
    Entity entity;
    NativeArray<int> positions;

    private void Start()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entity = manager.CreateEntity();
        manager.AddComponent<DestinationComponent>(entity);
        positions = new(1, Allocator.Persistent);
    }

    public void SetPos(int2 worldPos)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var world = SceneRefs.World;

        int2 offsetStart = worldPos;

        positions[0] = 
            world.GridSizeScaled * world.CurrentLevelIndex 
            + offsetStart.y * world.LargestLevelDimensionScaled 
            + offsetStart.x;

        manager.SetComponentData<DestinationComponent>(entity, new()
        {
            positions = positions,
            level = world.CurrentLevelIndex
        });


        ref var state = ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<PathfindSystem>();
        state.Enabled = true;
    }

    private void OnDestroy()
    {
        if(positions.IsCreated)
            positions.Dispose();
    }
}