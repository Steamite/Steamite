using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Build : GridTilesMode
{
    public override void EnterObject(ClickableObject enterObject)
    {
        Building blueprintPrefab = gridTiles.BlueprintPrefab;
        Building blueprintInstance = gridTiles.BlueprintInstance;

        if (gridTiles.Drag)
        {
            gridTiles.CalcPipes(gridTiles.ActivePos, blueprintPrefab as Pipe);
            return;
        }

        Color c;
        GridPos grid = blueprintInstance.blueprint.moveBy.Rotate(blueprintInstance.transform.eulerAngles.y);
        blueprintInstance.transform.position = new(
            gridTiles.ActivePos.x + grid.x,
            (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) +
                (blueprintInstance is Pipe
                ? ClickableObjectFactory.PIPE_OFFSET
                : ClickableObjectFactory.BUILD_OFFSET),
            gridTiles.ActivePos.z + grid.z);
        c = blueprintInstance.CanPlace() ? Color.blue : Color.red;
        blueprintInstance.Highlight(c);
    }    

    public override void UpObject()
    {
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
        }
    }

    public override bool ToggleMod()
    {

        if (gridTiles.BlueprintPrefab.Name != gridTiles.BlueprintInstance.Name)
        {
            gridTiles.DestroyBlueprint(false);
            gridTiles.Blueprint();
            return false;
        }
        return true;
    }


    public override void ExitMod()
    {
        SceneRefs.CameraSceneMover.SetRaycastMask(gridTiles.defaultMask);
        if (gridTiles.Drag)
        {
            gridTiles.ClearPipes();
            gridTiles.DeselectBuildingButton?.Invoke();
            gridTiles.Drag = false;
        }
        else if (gridTiles.BlueprintInstance)
            gridTiles.DestroyBlueprint(true);
        gridTiles.shiftKey.Disable();
    }

    public override bool EnterMod()
    {
        base.EnterMod();

        if (gridTiles.BlueprintPrefab is Pipe)
            SceneRefs.CameraSceneMover.SetRaycastMask(gridTiles.pipeMask);
        else
            SceneRefs.CameraSceneMover.SetRaycastMask(gridTiles.buildingMask);
        gridTiles.Blueprint();
        gridTiles.shiftKey.Enable();

        return false;
    }
}
