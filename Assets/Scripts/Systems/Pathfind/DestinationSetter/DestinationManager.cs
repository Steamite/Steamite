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
        int width = world.CurrentLevel.levelDimensionsScaled;

        int2 offsetStart = worldPos - world.CurrentLevel.levelOffsetScaled;

        manager.SetComponentData<DestinationComponent>(entity, new()
        {
            pos = offsetStart.y * width + offsetStart.x,
            worldPos = offsetStart,
            grid = world.CurrentLevel.renderGrid,
            width = width,
            offset = world.CurrentLevel.levelOffsetScaled
        });


        ref var state = ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<PathfindSystem>();
        state.Enabled = true;
    }
}