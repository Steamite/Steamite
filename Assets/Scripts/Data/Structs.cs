using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class GridPos
{
    [SerializeField]
    public float x;
    [SerializeField]
    public float z;
    [SerializeField]
    public float level;

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        GridPos pos = (GridPos)obj;
        if (pos.x == x && pos.z == z && pos.level == level)
        {
            return true;
        }
        return false;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
    public GridPos()
    {

    }
    public GridPos(float _x, float _level,float _z)
    {
        x = _x;
        level = _level;
        z = _z;
    }
    public GridPos(float _x, float _z)
    {
        x = _x;
        z = _z;
    }
    public GridPos(Vector3 vec, bool round = true) 
    {
        level = 0;
        if (round)
        {
            x = Mathf.RoundToInt(vec.x);
            z = Mathf.RoundToInt(vec.z);
        }
        else
        {
            x = Mathf.FloorToInt(vec.x);
            z = Mathf.FloorToInt(vec.z);
        }
    }
    public GridPos(GameObject g)
    {
        level = 0;
        x = Mathf.RoundToInt(g.transform.position.x);
        z = Mathf.RoundToInt(g.transform.position.z);
    }
    public Vector3 ToVec()
    {
        return new(x, level * 2, z);
    }
}
public enum GridItemType
{
    None, // doesn't matter
    Road, // must be free
    Water, // must be on a water tile
    Entrance, //here's an entry point - instantiate the entry points there
    Anchor // where the cursor is when moving
}

[Serializable]
public class NeededGridItem
{
    [SerializeField]
    public GridPos pos;
    [SerializeField]
    public GridItemType itemType;
    public NeededGridItem(GridPos _pos, GridItemType _itemType)
    {
        pos = _pos;
        itemType = _itemType;
    }
}

[Serializable]
public class BuildingGrid
{
    [SerializeField]
    public GridPos size;
    [SerializeField]
    public GridPos moveBy;
    [SerializeField]
    public GridPos anchor;
    [SerializeField]
    public List<NeededGridItem> itemList;
}

[Serializable]
public class Build
{
    [Header("Size")]
    //public float sizeX;
    //public float sizeZ;
    public BuildingGrid blueprint;
    public int heigth = 0;
    [Header("Storage")]
    public Resource cost;
    [Header("States")]
    public bool constructed = false;
    public bool deconstructing = false;
    public int constructionProgress = 0;
    public int maximalProgress = 0;
    public Build()
    {
    }
}
[Serializable]
public class Production
{
    public StorageResource inputResource = new();
    public Resource productionCost = new();
    public Resource production = new();
    public Production()
    {

    }
}
[Serializable]
public class ProductionTime
{
    public float prodTime = 20;
    public float currentTime = 0;
    public int modifier = 1;
    public ProductionTime(int _prodTime, float _currentTime, int _modifier)
    {
        this.prodTime = _prodTime;
        this.currentTime = _currentTime;
        this.modifier = _modifier;
    }
    public ProductionTime()
    {

    }
}
[Serializable]
public class ProductionStates
{
    public bool supplied = false;
    public bool supply = true; //
    public bool space = true;
    public bool stoped = false;
    public bool running = false;
    public ProductionStates(bool _supplied, bool _supply, bool _space, bool _stoped, bool _running, bool _isResearch)
    {
        this.supplied = _supplied;
        this.supply = _supply;
        this.space = _space;
        this.stoped = _stoped;
        this.running = _running;
    }
    public ProductionStates()
    {

    }
}
[Serializable]
public class Plan
{
    public List<GridPos> path = new();
    public int index = -1; // index in objects
    
    public Plan(List<GridPos> _path, int _index)
    {
        path = _path;
        index = _index;
    }
    public Plan()
    {

    }
}

[Serializable]
public class JobObjects
{
    public Rock r;
    public Building building;
    public JobObjects(Rock _r)
    {
        this.r = _r;
    }
    public JobObjects(Building _building)
    {
        this.building = _building;
    }
    public JobObjects()
    {

    }
}

[Serializable]
public struct JobData
{
    public JobState job;
    public List<GridPos> path;
    public ClickableObject interest;
    public JobData(JobSave jobSave, Human human)
    {
        job = jobSave.job;
        path = jobSave.path != null ? jobSave.path : new();
        interest = null;
        if(jobSave.destinationID > -1)
        {
            human.destination = MyGrid.buildings.Single(q => q.id == jobSave.destinationID);
            human.destination.TryLink(human);
        }
        if(typeof(Building) == jobSave.objectType)
        {
            interest = MyGrid.buildings.Single(q => q.id == jobSave.objectId);
        }
        else if(typeof(Rock) == jobSave.objectType)
        {
            interest = MyGrid.gridTiles.toBeDigged.FirstOrDefault(q => q.id == jobSave.objectId);
            interest.GetComponent<Rock>().assigned = human;
        }
        else if(typeof(Chunk) == jobSave.objectType)
        {
            interest = MyGrid.chunks.FirstOrDefault(q => q.id == jobSave.objectId);
        }
        if (interest)
        {
            if(!interest.Equals(human.destination))
                interest.GetComponent<StorageObject>()?.TryLink(human);
        }
    }
    public JobData(List<GridPos> _path, ClickableObject _interest)
    {
        path = _path;
        interest = _interest;
        job = JobState.Free;
    }
}

/// <summary>
/// Struct used for Resources.
/// </summary>
[Serializable]
public class Resource
{
    // (https://steamite.atlassian.net/wiki/x/AYCl)
    public int capacity = -1; // -1 = no limit
    public List<ResourceType> type = new(); // stores all resource types
    public List<int> ammount = new();
    public Resource()
    {

    }
    /// <returns>a new identical resource</returns>
    public Resource Clone()
    {
        Resource clone = new();
        for (int i = 0; i < type.Count; i++)
        {
            clone.type.Add(type[i]);
            if (ammount.Count <= i)
                clone.ammount.Add(0);
            else
                clone.ammount.Add(ammount[i]);
        }
        return clone;
    }
    // override object.Equals
    public override bool Equals(object resource)
    {
        if (resource == null || GetType() != resource.GetType())
        {
            return false;
        }

        Resource cmpRes = (Resource)resource;
        if (cmpRes.type.Count != type.Count)
            return false;
        for(int i = 0; i < type.Count; i++)
        {
            int x = cmpRes.type.IndexOf(type[i]);
            if (x == -1)
                return false;
            if (ammount[i] != cmpRes.ammount[x])
                return false;
        }
        return true;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}
public enum FluidType
{
    water,
    steam
}
[Serializable]
public class Fluid
{
    public List<FluidType> type = new();
    public List<int> ammount = new();
    public List<int> capacity = new();
}
/// <summary>
/// manages storage, present in all storageObjects
/// </summary>
[Serializable]
public class StorageResource
{
    public Resource stored;
    public List<Resource> requests;
    public List<Human> carriers;
    private List<int> carrierIDs;
    public List<int> mods;
    public StorageResource()
    {
        stored = new();
        requests = new();
        carriers = new();
        mods = new();
    }
    public StorageResource(StorageResSave resSave)
    {
        stored = resSave.stored;
        requests = resSave.requests;
        mods = resSave.mod;
        carriers = new();
        carrierIDs = resSave.carriers;
    }

    /// <summary>
    /// adds a reguest to the storage objects
    /// </summary>
    /// <param name="resource">requested resource</param>
    /// <param name="human">human who requested it</param>
    /// <param name="mod">add(1) or remove(-1)</param>
    public void AddRequest(Resource resource, Human human, int mod)
    {
        requests.Add(resource);
        carriers.Add(human);
        mods.Add(mod);
    }

    /// <summary>
    /// cancels a request
    /// </summary>
    /// <param name="human">human who requested it</param>
    public void RemoveRequest(Human human)
    {
        int index = carriers.IndexOf(human);
        requests.RemoveAt(index);
        carriers.RemoveAt(index);
        mods.RemoveAt(index);
    }

    public void ReassignCarriers(bool assign = true)
    {
        if (carriers.Count > 0)
        {
            if (assign)
            {
                carriers[0].jData.job = JobState.Deconstructing;
                carriers[0].ChangeAction(HumanActions.Demolish);
            }
            for (int i = carriers.Count - 1; i > 0; i++)
            {
                HumanActions.LookForNew(carriers[i]);
                RemoveRequest(carriers[i]);
            }
        }
    }
    /// <summary>
    /// returns future resources
    /// </summary>
    /// <param name="_stored"> true = return only resource available right now (removes the reserved ones)</param>
    /// <returns></returns>
    public Resource Future(bool _stored = false)
    {
        Resource futureRes = stored.Clone();
        for (int i = 0; i < requests.Count; i++)
        {
            int mod = mods[i];
            Resource r = requests[i];
            for (int j = 0; j < r.type.Count; j++)
            {
                int index;
                int toAdd = _stored ? (mods[i] == 1 ? 0 : r.ammount[j] * mod) : r.ammount[j]* mod;
                if ((index = futureRes.type.IndexOf(r.type[j])) == -1)
                {
                    futureRes.type.Add(r.type[j]);
                    futureRes.ammount.Add(toAdd);
                }
                else
                {
                    futureRes.ammount[index] += toAdd;
                }
            }
        }
        return futureRes;
    }
    public void LinkHuman(Human h) 
    {
        int index = carrierIDs.FindIndex(q => q == h.id);
        if (index > -1)
        {
            if(carriers.Count <= index)
            {
                carriers.Add(h);
            }
            else
            {
                carriers.Insert(index, h);
            }
        }
    }
}

[Serializable]
public struct ResearchStruct
{
    // General
    public bool completed; // Is the research completed
    public Transform button; // The transform of the research button
    public bool is_loaded; // Is the research loaded
    
    // Pre-researtch start
    public int research_needed; // Research points needed to unlock
    public Resource cost; // Cost of starting the research
    public bool was_ever_researched; // Has the research been researched before(Used to determine if the research start should cost smth)
    
    // Research in progress
    public bool is_being_researched; // Is the research being researched at the moment
    public int research_progress; // The progress of the research
    
    // Research finish
    public List<Transform> unlocks; //Buttons that the research unlocks
}
/*
[Serializable]
public class GridItem
{
    public int id;
    public virtual string PrintText()
    {
        return "";
    }
}
public class GridWater : GridItem
{
    public int ammount = 50;
    public override string PrintText()
    {
        return "w";
    }
}
[Serializable]
public class GridOre : GridItem
{
    public Resource resource;
    public int hardness;
    public int integrity;
    public string oreName;
    public override string PrintText()
    {
        return "o";
    }
}
[Serializable]
public class GridBuild : GridItem
{
    public GridBuild(int _id)
    {
        id = _id;
    }
    public override string PrintText()
    {
        return "p";
    }
}*/
[Serializable]
public class BuildObject
{
    public List<Vector2Int> parts;
    public List<Vector2Int> entryPoints;
    public BuildObject()
    {
        this.parts = new();
        this.entryPoints = new();
    }
}