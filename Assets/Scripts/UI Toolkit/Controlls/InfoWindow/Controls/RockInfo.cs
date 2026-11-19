using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Grid.New;
using InfoWindowElements;
using JobFind;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LightTransport;
using UnityEngine.UIElements;

[UxmlElement]
public partial class RockInfo : InfoWindowControl
{
    struct PrevData
    {
        public float durability;
        public Entity assigned;
    }

    Label assigned, durability;
    ResourceList yeild;

    int levelIndex;
    int localIndex;

    int globalPosition;

    PrevData prevData;

    public override bool Update()
    {
        var level = SceneRefs.World.levelArray[levelIndex];
        LevelGridTile tile = level.renderGrid[localIndex];
        if (tile.TType != TileType.Wall)
            return false;

        float newDurability = tile.Durability;
        if(newDurability != prevData.durability)
        {
            prevData.durability = newDurability;
            durability.text = $"{newDurability:0.##}";
        }

        

        if(JobOfferLinker.instance.GetAssignedHuman(globalPosition, out Entity assignedHuman))
        {
            if(prevData.assigned == Entity.Null || assignedHuman.Index != prevData.assigned.Index)
            {
                prevData.assigned = assignedHuman;
                assigned.text = assignedHuman.ToString();
            }
        }
        else
        {
            if(prevData.assigned != Entity.Null)
            {
                prevData.assigned = Entity.Null;
                assigned.text = "Empty";
            }
        }

        return true;
    }

    EntityManager manager;
    public override void Select(SelectionData data)
    {
        var world = SceneRefs.World;
        localIndex = data.localPosition;
        levelIndex = world.CurrentLevelIndex;

        var level = world.levelArray[levelIndex];
        globalPosition = Recalculation.GlobalIndexFromLocalIndex(localIndex, level.offsetsScaled, level.dimensionsScaled, world.LargestLevelDimensionScaled, levelIndex);
        /* Recalculation.LocalIndexFromGlobalIndex(
            data.localPosition, 
            world.levelArray, 
            world.GridSizeScaled, 
            out levelIndex);*/

        prevData = new() { durability = float.MinValue, assigned = Entity.Null};
        assigned.text = "Empty";

        Update();
    }

    public override void Open(object data)
    {
        /*
        LevelGridTile gridTiles = (LevelGridTile)data;
        integrity.text = $"{gridTiles.Durability:0.##}";
     */   

        /*
        // Assigned Binding
        DataBinding binding = BindingUtil.CreateBinding(nameof(Rock.Assigned));
        binding.sourceToUiConverters.AddConverter((ref Human human) => $"{(human ? human.Name : "None")}");
        InfoWindow.Window.RegisterTempBinding(new(assigned, "text"), binding, dataSource);

        // Integrity Binding
        binding = BindingUtil.CreateBinding(nameof(Rock.Integrity));
        binding.sourceToUiConverters.AddConverter((ref float integrity) => $"{integrity:0.#}");
        InfoWindow.Window.RegisterTempBinding(new(integrity, "text"), binding, dataSource);
        yeild.Open(data);
        */
    }

    public RockInfo()
    {
        // Assigned
        VisualElement element = new() { name = "Group" };

        VisualElement secElement = new() { name = "Line-Container" }; ;
        secElement.Add(new Label("Assigned"));
        secElement.Add(assigned = new Label("Unknown"));
        element.Add(secElement);

        secElement = new() { name = "Line-Container" };
        secElement.Add(new Label("Integrity"));
        secElement.Add(durability = new Label("#.#"));
        element.Add(secElement);
        Add(element);

        //inventory
        element = new() { name = "Group" };
        element.Add(new Label("Yeild") { name = "Resource-Header" });
        element.Add(yeild = new ResourceList());
        Add(element);
        yeild.verticalPadding = 2;
    }
}
