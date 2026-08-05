

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(GridTiles))]
public class BuildingActions : MonoBehaviour
{
    GridTiles gridTiles;

    /// <summary>Building that's currently beeing placed.</summary>
    Building blueprintInstance;
    public Building BlueprintInstance
    {
        get => blueprintInstance;
        set
        {
            MyGrid.GetOverlay().DestroyBuilingTiles();
            blueprintInstance = value;
            if (value == null)
            {
                gridTiles.DeselectBuildingButton?.Invoke();
                gridTiles.ChangeSelMode(ControlMode.Nothing);
            }
        }
    }

    /// <summary>Currently selected building for construction.</summary>
    [Header("Tilemaps")] Building blueprintPrefab;

    /// <summary>Changed from <see cref="BuildMenu"/></summary>
    public Building BlueprintPrefab
    {
        get => blueprintPrefab;
        set
        {
            blueprintPrefab = value;
            if (value == null)
            {
                if (blueprintInstance != null)
                    gridTiles.ChangeSelMode(ControlMode.Nothing);
            }
            else
                gridTiles.ChangeSelMode(ControlMode.Build);
        }
    }

    private void Awake()
    {
        gridTiles = GetComponent<GridTiles>();
    }


    /// <summary>
    /// Instantiates and sets a copy of a building prefab.
    /// </summary>
    public void Blueprint()
    {
        Quaternion q = new();
        if (blueprintInstance)
            q = new(blueprintInstance.transform.rotation.x, blueprintInstance.transform.rotation.y, blueprintInstance.transform.rotation.z, blueprintInstance.transform.rotation.w);
        GridPos gp = blueprintPrefab.blueprint.moveBy.Rotate(blueprintPrefab.transform.eulerAngles.y);
        gp = new(
            gridTiles.ActivePos.x + gp.x,
            (MyGrid.currentLevel * ClickableObjectFactory.LEVEL_HEIGHT) +
                (blueprintPrefab is Pipe
                ? ClickableObjectFactory.PIPE_OFFSET
                : ClickableObjectFactory.BUILD_OFFSET),
            gridTiles.ActivePos.z + gp.z);

        blueprintInstance = Instantiate(
            blueprintPrefab,
            new Vector3(gp.x, gp.y, gp.z),
            q,
            blueprintPrefab is Pipe
                ? MyGrid.FindLevelPipes()
                : MyGrid.FindLevelBuildings());
        if (blueprintInstance is IFluidWork fluidWork)
            fluidWork.CreatePipes();

        blueprintInstance.AfterBlueprint();
    }

    /// <summary>
    /// Destroys the blueprint object and overlaygroup.
    /// </summary>
    /// <param name="forgetInstance">If true removes the instance.</param>
    public void DestroyBlueprint(bool forgetInstance)
    {
        if (blueprintInstance is Pipe)
        {
            Pipe pipe = blueprintInstance as Pipe;
            for (int i = 0; i < 4; i++)
                pipe.DisconnectPipe(i, true);
        }
        else if (blueprintInstance is IFluidWork fluidWork)
        {
            fluidWork.DisconnectFromNetwork();
        }
        Destroy(blueprintInstance.gameObject);
        if (forgetInstance)
            BlueprintInstance = null;
        else
            MyGrid.GetOverlay().DestroyBuilingTiles();
    }
}
