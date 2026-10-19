using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildModeNew : GridTilesModeNew
{
    BuildingWrapper wrapper;

    [SerializeField] LevelRenderer renderer;
    BuildingRenderer buildingRenderer;

    event Action<BuildingWrapper> onWrapperChange;




    public override void InitMode(MouseController _mouseController)
    {
        base.InitMode(_mouseController);
        buildingRenderer = renderer.GetCustomRenderer(0) as BuildingRenderer;
    }

    public void AttachToWrapperChange(Action<BuildingWrapper> action) => onWrapperChange += action;

    protected override bool CheckTopObstruction(int2 pos, int index, LevelGridTile tile)
    {
        TileType tileType = (TileType)tile.Type;
        if (tileType < TileType.Road)
        {
            controller.SetMaterial(pos, 0, hoverColor, tileType, index, 0.0001f);
            return true;
        }
        return false;
    }


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);
        if (!lastPos.Equals(controller.hoverWorldPosition))
        {
            BuildPositionUpdate(controller.hoverWorldPosition);

            buildingRenderer.MoveBlueprint(buildingPos);
        }
    }


    public override void UpObject()
    {/*
        Building blueprintPrefab = gridTiles.BlueprintPrefab;
        Building blueprintInstance = gridTiles.BlueprintInstance;
        if (blueprintPrefab is Pipe && (blueprintInstance == null || blueprintInstance.CanPlace()))
        {
            if (gridTiles.Drag == false)
            {

                gridTiles.InitPipes(
                    gridTiles.ActivePos,// new GridPos(activePos.x, activePos.y, activePos.z),
                    blueprintInstance as Pipe);
                SceneRefs.Overlays.blueprintIndicator.MovePlacePipeOverlay(gridTiles.ActivePos, true);
                gridTiles.Drag = true;

                //GridPos gridPos = gridTiles.ActiveObject.GetPos();
                MyGrid.SetGridItem(gridTiles.ActivePos, blueprintInstance, true);
                blueprintInstance = null;
            }
            else
            {
                if (gridTiles.ClickPipes(gridTiles.ActivePos))
                    gridTiles.BlueprintInstance = null;
            }

        }
        else if (blueprintInstance.CanPlace())
        {
            blueprintInstance.PlaceBuilding();
            if (gridTiles.shiftKey.IsInProgress() && MyRes.CanAfford(blueprintPrefab.Cost))
            {
                //SceneRefs.Overlays.blueprintIndicator?.DestroyBuilingTiles();
                gridTiles.Blueprint();
            }
            else
            {
                gridTiles.BlueprintInstance = null;
            }
        }
        else
        {
            Debug.LogWarning("Can't place here!!");
        }*/
    }

    /*public override bool ToggleMod()
    {

        if (gridTiles.BlueprintPrefab.Name != gridTiles.BlueprintInstance.Name)
        {
            gridTiles.DestroyBlueprint(false);
            gridTiles.Blueprint();
            return false;
        }
        return true;
    }*/


    readonly static int[] cosTable = { 1, 0, -1, 0 };
    readonly static int[] sinTable = { 0, 1, 0, -1 };
    public override void DownObject()
    {
        int2 size = wrapper.blueprint.size;
        size.y -= 1;

        uint rot = (buildingPos >> 23) & 0x3;

        int x = size.x * cosTable[rot] - size.y * sinTable[rot];
        int y = size.x * sinTable[rot] + size.y * cosTable[rot];
        size.x = x;
        size.y = y;

        int2 world = controller.hoverWorldPosition;
        
        if(size.x < 0)
        {
            world.x += size.x + 1;
            size.x = -size.x;
        }
        if (size.y < 0)
        {
            world.y += size.y + 1;
            size.y = -size.y;
        }
        int originalX = world.x;

        for (int j = 0; j < size.y; j++)
        {
            for (int i = 0; i < size.x; i++)
            {
                var a = SceneRefs.World.GetActiveTile(world.x, world.y).TType;
                if (a != TileType.Empty && a != TileType.Road)
                    return;

                world.x++;
            }
            world.x = originalX;
            world.y++;
        }

        SceneRefs.World.WorldBuildings.PlaceBuilding(
            controller.hoverWorldPosition, 
            wrapper,
            buildingBlueprint, 
            buildingPos);

        bool clearBlueprint = !controller.isShiftPressed;
        buildingBlueprint.renderIndex = buildingRenderer.PlaceBlueprint(clearBlueprint);
        
        if(clearBlueprint)
            controller.ChangeSelMode(ControlMode.Nothing);
        else
        {
            buildingBlueprint = wrapper.Building.Clone();
        }
    }

    public override void ExitMod()
    {
        wrapper = null;
        categoryIndex = -1;
        wrapperIndex = -1;

        buildingRenderer.DestroyBlueprint();
        onWrapperChange?.Invoke(wrapper);
    }

    BuildingNew buildingBlueprint;
    public int categoryIndex;
    public int wrapperIndex;

    public void SetBlueprint(BuildingWrapper _wrapper)
    {
        if (_wrapper == null || _wrapper == wrapper)
        {
            controller.ChangeSelMode(ControlMode.Nothing);
            return;
        }
        else if(controller.activeModeIndex != (int)ControlMode.Build)
        {
            controller.ChangeSelMode(ControlMode.Build);
        }

        // new wrapper set
        wrapper = _wrapper;

        buildingBlueprint = wrapper.Building.Clone();
        BuildPositionUpdate(controller.hoverWorldPosition, true);


        wrapperIndex = buildingBlueprint.wrapperIndex;
        categoryIndex = buildingBlueprint.categoryIndex;


        buildingBlueprint.renderData.position = buildingPos;
        buildingRenderer.CreateBlueprint(buildingBlueprint.renderData);

        onWrapperChange?.Invoke(wrapper);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWorldPosition"></param>
    /// <param name="restOfPosition"></param>
    /// <returns>Packed position, should keep rotation.</returns>
    void BuildPositionUpdate(int2 newWorldPosition, bool discardRot = false)
    {
        int offset = SceneRefs.World.CurrentLevel.levelOffsetScaled;
        int2 fixedPos = newWorldPosition - offset;

        uint maskedPosition = (uint)(buildingPos & ~0x3FFFFF);
        if (discardRot)
            maskedPosition = 0;
        buildingPos = (uint)((fixedPos.x & 0x3FF) | ((fixedPos.y & 0x3FF) << 10)) | maskedPosition;
    }


    uint buildingPos;
    public void Rotate(bool clockwise)
    {
        uint rot = (buildingPos >> 23) & 0x3;
        if (clockwise)
        {
            rot++;
            if (rot == 4)
                rot = 0;
        }
        else
        {
            if (rot == 0)
                rot = 4;
            rot--;
        }

        rot = (rot & 0x3) << 23;
        buildingPos = (uint)((buildingPos & ~(0x3 << 23)) | rot);
        buildingRenderer.MoveBlueprint(buildingPos);
    }
}
