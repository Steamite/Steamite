using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New.Modes;
using Grid.New;
using ModesNew;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildModeNew : GridTilesModeNew
{
    BuildingWrapper wrapper;

    BuildingRenderManager buildingRenderer;

    event Action<BuildingWrapper> onWrapperChange;

    [SerializeField] Material blueprintMaterial;
    [SerializeField] Color availableColor;
    [SerializeField] Color unavailableColor;


    public override void InitMode(MouseController _mouseController)
    {
        base.InitMode(_mouseController);
        buildingRenderer = SceneRefs.World.WorldBuildings.buildingRenderer;
        SceneRefs.World.AttachChange(OnLevelChange);
    }

    void OnLevelChange(LevelComponent oldLevel, LevelComponent level, int newLevel)
    {
        if (wrapper == null)
            return;

        uint change = 
            (uint)(oldLevel.offsetsScaled - level.offsetsScaled);

        uint x = (0x3FF & buildingPos) + change;
        uint z = ((0x3FF & (buildingPos >> 10)) + change) << 10;

        uint pos = (x | z) | (buildingPos & (0x3 << 23));
        uint y = (uint)(0x7 & newLevel) << 20;

        buildingPos = pos | y;
        
        buildingRenderer.MoveBlueprint(buildingPos);
    }

    public void AttachToWrapperChange(Action<BuildingWrapper> action) => onWrapperChange += action;

    protected override bool CheckTopObstruction(int2 pos, int index, LevelGridTile tile)
    {
        TileType tileType = (TileType)tile.Type;
        if (tileType < TileType.Road)
        {
            controller.hoverWorldPosition = pos;
            
            return true;
        }
        return false;
    }
    protected override void FallbackColor(LevelGridTile tile, int index, int2 prevPos)
    {
        controller.hoverWorldPosition = prevPos;
    }


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);
        if (!lastPos.Equals(controller.hoverWorldPosition))
        {
            BuildPositionUpdate(controller.hoverWorldPosition);

            buildingRenderer.MoveBlueprint(buildingPos);
            
            UpdateEmmissionColor();
        }
    }

    public void UpdateEmmissionColor()
    {
        blueprintMaterial.SetColor("_EmmissionColor",
                TestPlacement(wrapper.blueprint.size, controller.hoverWorldPosition)
                ? availableColor
                : unavailableColor);
    }

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
        if (!TestPlacement(size, world))
            return;

        buildingBlueprint.encodedLocalPosition = buildingPos;

        bool clearBlueprint = !controller.isShiftPressed;
        buildingBlueprint.renderIndex = buildingRenderer.PlaceBlueprint(clearBlueprint);

        SceneRefs.World.WorldBuildings.PlaceBuilding(
            wrapper,
            buildingBlueprint);


        
        if(clearBlueprint)
            controller.ChangeSelMode(ControlMode.Nothing);
        else
        {
            UpdateEmmissionColor();
        }
    }

    public bool TestPlacement(int2 size, int2 world)
    {
        int originalX = world.x;
        for (int j = 0; j < size.y; j++)
        {
            for (int i = 0; i < size.x; i++)
            {
                var a = SceneRefs.World.GetActiveTile(world).TType;
                if (a != TileType.Empty && a != TileType.Road)
                    return false;

                world.x++;
            }
            world.x = originalX;
            world.y++;
        }
        return true;
    }

    public override void ExitMod()
    {
        wrapper = null;
        wrapperIndex = -1;
        categoryIndex = -1;
        categoryIndex = -1;

        buildingRenderer.DestroyBlueprint();
        onWrapperChange?.Invoke(wrapper);
    }

    BuildingRenderData buildingBlueprint;
    public int wrapperIndex;

    // FOR UI CONTROLLS
    public int categoryIndex;
    public int objectIndex;

    public void ClearBlueprint()
    {
        controller.ChangeSelMode(ControlMode.Nothing);
    }

    public void SetBlueprint(short wrapperIndex, int catIndex, int objIndex, BuildingWrapper wrapper)
    {
        if(wrapperIndex == -1 || wrapperIndex == this.wrapperIndex)
        {
            controller.ChangeSelMode(ControlMode.Nothing);
            return;
        }

        categoryIndex = catIndex;
        objectIndex = objIndex;

        // new wrapper set
        this.wrapperIndex = wrapperIndex;
        this.wrapper = wrapper;

        BuildPositionUpdate(controller.hoverWorldPosition, true);

        buildingBlueprint = new()
        {
            encodedLocalPosition = buildingPos,
            meshIndex = wrapperIndex,
            renderIndex = -1
        };

        uint pos = (uint)(~(0x7 << 20) & buildingPos);
        uint levelPos = (uint)((SceneRefs.World.CurrentLevelIndex & 0x7) << 20);
        buildingPos = pos | levelPos;

        buildingBlueprint.encodedLocalPosition = buildingPos;

        buildingRenderer.CreateBlueprint(buildingBlueprint);


        if (controller.activeModeIndex != (int)ControlMode.Build)
        {
            controller.ChangeSelMode(ControlMode.Build);
        }

        onWrapperChange?.Invoke(wrapper);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWorldPosition"></param>
    /// <param name="discardRot"></param>
    /// <returns>Packed position</returns>
    void BuildPositionUpdate(int2 newWorldPosition, bool discardRot = false)
    {
        int offset = SceneRefs.World.currentLevel.offsetsScaled;
        int2 fixedPos = newWorldPosition - offset;

        uint maskedPosition;
        if (discardRot)
            maskedPosition = 0;
        else
            maskedPosition = (uint)(buildingPos & ~0xFFFFF);

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
