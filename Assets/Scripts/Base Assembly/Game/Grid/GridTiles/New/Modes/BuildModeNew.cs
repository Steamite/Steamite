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

    public override void DownObject()
    {
        var t = SceneRefs.World.CurrentLevel.renderGrid[controller.hoverObjectIndex].TType;
        if (t != TileType.Empty && t != TileType.Road)
            return;


        SceneRefs.World.WorldBuildings.PlaceBuilding(
            controller.hoverWorldPosition, 
            buildingBlueprint);

        buildingRenderer.PlaceBlueprint(controller.isShiftPressed);
        
        if(!controller.isShiftPressed)
            controller.ChangeSelMode(ControlMode.Nothing);
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

        wrapperIndex = wrapper.Building.wrapperIndex;
        categoryIndex = wrapper.Building.categoryIndex;

        buildingBlueprint = wrapper.Building.Clone();
        BuildPositionUpdate(controller.hoverWorldPosition);

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
    void BuildPositionUpdate(int2 newWorldPosition)
    {
        int offset = SceneRefs.World.CurrentLevel.levelOffsetScaled;
        int2 fixedPos = newWorldPosition - offset;

        uint maskedPosition = (uint)(buildingPos & ~0x3FFFFF);
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
