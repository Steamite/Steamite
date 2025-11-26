using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using UnityEngine;

[Serializable]
public class JobSave
{
    /// <summary>Helps with finding the interest, by chosing the correct list to search in.</summary>
    public enum InterestType
    {
        /// <summary>No interest.</summary>
        Nothing,
        /// <summary>Interest is a <see cref="Building"/></summary>
        B,
        /// <summary>Interest is a <see cref="Rock"/></summary>
        R,
        /// <summary>Interest is a <see cref="Chunk"/></summary>
        C,
        /// <summary>Interest is a <see cref="Pipe"/></summary>
        P
    }
    public JobState job;
    public List<GridPos> path;
    public int destinationID;
    public int interestID;
    public InterestType interestType; // -1 – unassigned; 0 – nothing; 1 – building; 2 – rock; 3 – chunk
    public InterestType destType; // -1 – unassigned; 0 – nothing; 1 – building; 2 – rock; 3 – chunk; 4 -
    public JobSave()
    {

    }
    public JobSave(JobData jobData, ClickableObject destination)
    {
        job = jobData.job;
        path = jobData.path;
        // interest assign 
        if (jobData.interest)
        {
            Tuple<int, InterestType> data = jobData.interest.GetID();
            interestID = data.Item1;
            interestType = data.Item2;
        }
        else
            interestID = -1;

        if (destination)
        {
            Tuple<int, InterestType> data = destination.GetID();
            destinationID = data.Item1;
            destType = data.Item2;
        }
        else
            destinationID = -1;
    }
}
////////////////////////////////////////////////////////////
//--------------------------Grid--------------------------//
////////////////////////////////////////////////////////////
public class WorldSave
{
    public BuildsAndChunksSave objectsSave;
    public GridSave[] gridSave;
}

[Serializable]
public class GridSave
{
    public int width;
    public int height;
    public ClickableObjectSave[,] grid;
    public ClickableObjectSave[,] pipes;
    public int elevatorID;

    public GridSave(int _width, int _height, int _elevatorID)
    {
        width = _width;
        height = _height;
        grid = new ClickableObjectSave[_width, _height];
        pipes = new ClickableObjectSave[_width, _height];
        elevatorID = _elevatorID;
    }
    public GridSave()
    {
    }
}

[Serializable]
public class BuildsAndChunksSave
{
    public BuildingSave[] buildings;
    public ChunkSave[] chunks;
    public VeinSave[] veins;

    public BuildsAndChunksSave(BuildingSave[] _buildings, ChunkSave[] _chunks, VeinSave[] _veins)
    {
        buildings = _buildings;
        chunks = _chunks;
        veins = _veins;
    }
}

[Serializable]
public class ClickableObjectSave
{
    public int id;
    public string objectName;
}

[Serializable]
public class RockSave : ClickableObjectSave
{
    public ResourceSave yeild;
    // use for loading correct integrity colors
    public float originalIntegrity;
    public float integrity;
    public bool toBeDug;
    public HiddenSave hiddenSave;

    public RockSave() { }

    public RockSave(Resource yeild, float originalIntegrity, float integrity, bool toBeDug, string _name)
    {
        this.yeild = new(yeild);
        this.originalIntegrity = originalIntegrity;
        this.integrity = integrity;
        this.toBeDug = toBeDug;
        objectName = _name;
        id = -1;
    }
}

public enum HiddenType
{
    Nothing,
    Water
}
[Serializable]
public class HiddenSave
{
    public HiddenType assignedType;
    public int ammount = 0;
}


[Serializable]
public class WaterSave : ClickableObjectSave
{
    public ResourceSave fluid;

    public WaterSave() { }
    public WaterSave(ResourceSave _fluid)
    {
        fluid = _fluid;
        id = -1;
    }

    public WaterSave(int i)
    {
        fluid = new() 
        {
            ammounts = { i}, 
            types = { ResFluidTypes.GetSaveIndex(ResFluidTypes.GetFluidByName("Water"))}
        };
    }
}

[Serializable]
public class VeinSave : ClickableObjectSave
{
    public GridPos gridPos;
    public ResourceSave resource;
    public int sizeX;
    public int sizeZ;
    public MyColor veinColor;
}

////////////////////////////////////////////////////////////
//---------------------Grid Buildings---------------------//
////////////////////////////////////////////////////////////
[Serializable]
public class StorageObjectSave : ClickableObjectSave
{
    public StorageResSave resSave;
    public GridPos gridPos;
}

[Serializable]
public class ChunkSave : StorageObjectSave
{
    public MyColor resColor;
}
[Serializable]
public class BuildingSave : StorageObjectSave
{
    ///public Build build;
    public string Name;
    public float rotationY;

    public BuildingGrid blueprint;
    public bool constructed;
    public bool deconstructing;
    public float constructionProgress;

    public DataAssign prefabConnection;
}

[Serializable]
public class StorageBSave : BuildingSave
{
    public List<bool> canStore;
    public bool isMain;

    public void SetupStorage()
    {
        constructed = true;
        isMain = true;
        int i = 1;
        canStore = new();
        foreach (var categ in ResFluidTypes.FullRes.Skip(1).SkipLast(1))
        {
            foreach (var obj in categ.Objects)
            {
                resSave.types.Add(new(categ.id, obj.id));
                resSave.ammounts.Add(100);
                canStore.Add(true);
                i++;
            }
        }
    }
}

[Serializable]
public class AssignBSave : BuildingSave
{
    public List<int> assigned;
    public int limit;
}

[Serializable]
public class ResProductionBSave : ProductionBSave
{
    public StorageResSave inputRes;
}

public class ProductionBSave : AssignBSave
{
    public float currentTime = 0;
    public ProductionStates ProdStates;
    public int selectedRecipe;
}

public class PipeBSave : BuildingSave
{
    public int networkID;
}

public class TankBSave : BuildingSave
{
    public ResourceSave fluidSave;
}

public class FluidResProductionSave : ResProductionBSave
{
    public ResourceSave inputFluid;
    public ResourceSave storedFluid;
}


[Serializable]
public class FluidProdBSave : ProductionBSave
{
    public ResourceSave fluidSave;
}

//////////////////////////////////////////////////////////////////
//-----------------------------Humans---------------------------//
//////////////////////////////////////////////////////////////////
public class HumanSave : ClickableObjectSave
{
    // Data mainly for loading
    public MyColor color;
    public GridPos gridPos;
    public float rotation;
    // Job use
    public JobSave jobSave;
    public ResourceSave inventory;
    // statuses
    public float sleep;
    // specializations
    public Specializations specs;
    public int houseID;
    public int workplaceId;
    public HumanSave()
    {

    }
}
[Serializable]
public class StorageResSave : ResourceSave
{
    [JsonProperty, JsonRequired] public List<ResourceSave> Requests { get; set; }

    public List<int> carriers;
    public List<int> mod;

    public StorageResSave(StorageResource storageResource) : base(storageResource)
    {
        Requests = storageResource.requests.Select(q => new ResourceSave(q)).ToList();
        carriers = storageResource.carriers.Select(q => q.id).ToList();
        mod = storageResource.mods.ToList();
    }
    public StorageResSave() : base()
    {

    }
    public override bool Equals(object _resource)
    {
        if (_resource is not StorageResSave)
            return false;
        return base.Equals(_resource);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), types, ammounts, Requests, carriers, mod);
    }
}
[Serializable]
public class GameStateSave
{
    public List<JobState> priorities;
    public int dayTime;
    public int numberOfDays;
    public bool autoSave;

}

[Serializable]
public class MyColor
{
    public float r;
    public float g;
    public float b;
    public float a;
    public MyColor(Color color)
    {
        r = Mathf.RoundToInt(color.r * 255);
        g = Mathf.RoundToInt(color.g * 255);
        b = Mathf.RoundToInt(color.b * 255);
        a = Mathf.RoundToInt(color.a * 255);
    }
    public Color ConvertColor()
    {
        return new(r / 255, g / 255, b / 255, a / 255);
    }
}

public class ResourceSave
{
    public List<DataAssign> types;
    public List<int> ammounts;

    public ResourceSave(Resource res)
    {
        if(res != null)
        {
            types = res.types.Select(q => ResFluidTypes.GetSaveIndex(q)).ToList();
            ammounts = res.ammounts.ToList();
        }
    }
    public ResourceSave()
    {
        types = new();
        ammounts = new();
    }
}

public class TradeDealSave
{
    public int type;
    public int cost;

    public TradeDealSave(int _type, int _cost)
    {
        type = _type;
        cost = _cost;
    }
    public TradeDealSave() { }
}

public class LocationSave
{
    public GridPos position;
    public string name;

    public LocationSave(Location location)
    {
        position = location.pos;
        name = location.Name;
    }

    public LocationSave() { }
}

public class TradeLocationSave : LocationSave
{
    public List<TradeDealSave> tradeDealsSell;
    public List<TradeDealSave> tradeDealsBuy;

    public float distance;

    public TradeLocationSave(TradeLocation location) : base(location)
    {
        tradeDealsSell = location.Sell?.Select(q => new TradeDealSave(ResFluidTypes.GetResourceIndex(q.type), q.cost)).ToList();
        tradeDealsBuy = location.Buy?.Select(q => new TradeDealSave(ResFluidTypes.GetResourceIndex(q.type), q.cost)).ToList();

        distance = location.distance;
    }

    public TradeLocationSave() : base() { }
}

