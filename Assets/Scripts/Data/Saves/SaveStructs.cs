using System;
using System.Collections.Generic;
using System.Linq;
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
        C
    }
    public JobState job;
    public List<GridPos> path;
    public int destinationID;
    public int interestID;
    public InterestType interestType; // -1 – unassigned; 0 – nothing; 1 – building; 2 – rock; 3 – chunk
    public JobSave()
    {

    }
    public JobSave(JobData jobData)
    {
        job = jobData.job;
        path = jobData.path;
        // interest assign 
        if (jobData.interest)
            jobData.interest.GetComponent<ClickableObject>().GetID(this);
        else
        {
            interestID = -1;
        }
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

    public GridSave(int _width, int _height)
    {
        width = _width;
        height = _height;
        grid = new ClickableObjectSave[_width, _height];
        pipes = new ClickableObjectSave[_width, _height];
    }
}

[Serializable]
public class BuildsAndChunksSave
{
    public BSave[] buildings;
    public ChunkSave[] chunks;

    public BuildsAndChunksSave(BSave[] _buildings, ChunkSave[] _chunks)
    {
        buildings = _buildings;
        chunks = _chunks;
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
    public ResourceType res;
    public int ammount;
    public float integrity;
    public bool toBeDug;
}
[Serializable]
public class WaterSave : ClickableObjectSave
{
    public int ammount;
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
public class BSave : StorageObjectSave
{
    ///public Build build;
    public string prefabName;
    public float rotationY;

    public BuildingGrid blueprint;
    public Resource cost;
    public bool constructed;
    public bool deconstructing;
    public float constructionProgress;
    public int maximalProgress;

    public int categoryID;
    public int wrapperID;
}

[Serializable]
public class StorageBSave : BSave
{
    public List<bool> canStore;
    public bool isMain;
}

[Serializable]
public class AssignBSave : BSave
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
    public float prodTime = 20;
    public float currentTime = 0;
    public int modifier = 1;
    public ProductionStates ProdStates;
}

public class PipeBSave : BSave
{
    public int networkID;
}
public class LightWeightPipeBSave : ClickableObjectSave
{
    public int networkID;
}
public class TankBSave : BSave
{
    public MyColor fillColor;
    public FluidWorkSave fluidSave;
}

public class FluidProdBSave : ProductionBSave
{
    public FluidWorkSave fluidSave;
}

public class FluidWorkSave
{
    public Fluid fluid;
    public List<ClickableObjectSave> pipeSaves;
}

//////////////////////////////////////////////////////////////////
//-----------------------------Humans---------------------------//
//////////////////////////////////////////////////////////////////
public class HumanSave : ClickableObjectSave
{
    // Data mainly for loading
    public MyColor color;
    public GridPos gridPos;
    // Job use
    public JobSave jobSave;
    public Resource inventory;
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
public class StorageResSave
{
    public Resource stored;
    public List<Resource> requests;
    public List<int> carriers;
    public List<int> mod;

    public StorageResSave(StorageResource storageResource)
    {
        stored = storageResource.stored;
        requests = storageResource.requests;
        carriers = storageResource.carriers.Select(q => q.id).ToList();
        mod = storageResource.mods;
    }
    public StorageResSave()
    {

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
