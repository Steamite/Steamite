using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public class BuildModeNew : GridTilesModeNew
{
    BuildingWrapper wrapper;

    BuildingRenderer buildingRenderer;
    public override void InitMode(MouseController _mouseController)
    {
        base.InitMode(_mouseController);
    }


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);
        if (!lastPos.Equals(controller.hoverWorldPosition))
        {

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


    public override void ExitMod()
    {
        //SceneRefs.CameraSceneMover.SetRaycastMask(gridTiles.defaultMask);
        /*if (gridTiles.Drag)
        {
            gridTiles.ClearPipes();
            gridTiles.DeselectBuildingButton?.Invoke();
            gridTiles.Drag = false;
        }
        else if (gridTiles.BlueprintInstance)
            gridTiles.DestroyBlueprint(true);
        gridTiles.shiftKey.Disable();*/
    }

    public override bool EnterMod()
    {
        base.EnterMod();
/*
        if (gridTiles.BlueprintPrefab is Pipe)
            SceneRefs.CameraSceneMover.SetRaycastMask(gridTiles.pipeMask);
        else
            SceneRefs.CameraSceneMover.SetRaycastMask(gridTiles.buildingMask);
        gridTiles.Blueprint();
        gridTiles.shiftKey.Enable();*/

        return false;
    }

    BuildingNew newBuilding;
    
    public void SetBlueprint(BuildingWrapper _wrapper)
    {
        if(wrapper != _wrapper)
        {
            wrapper = _wrapper;
            newBuilding = wrapper.Building.Clone();
            int offset = SceneRefs.World.CurrentLevel.levelOffsetScaled;
            int2 fixedPos = controller.hoverWorldPosition - offset;
            uint pos = (uint)((fixedPos.x & 0x3FF) | ((fixedPos.y&0xFF) << 10));

            newBuilding.renderData.position =  pos;
            buildingRenderer.CreateBlueprint(newBuilding.renderData);
        }
    }
}
