using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// <inheritdoc cref="IStorage"/>
/// </summary>
public class Elevator : Building, IStorage
{
    #region Variables
    /// <inheritdoc/>
    [CreateProperty] public List<bool> CanStore { get; set; } = new();
    public StorageResource LocalResources => localRes;

    public ulong CanStoreMask { get => canStoreInt; set => canStoreInt = value; }
    [SerializeField] ulong canStoreInt;
    #endregion

    public override void UniqueID()
    {
        base.UniqueID();
    }
    #region Window
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("Storage", new List<string> { "Storage Info" });
        toEnable.Add("Levels", new List<string> { "Level Info" });
        base.ToggleInfoComponents(info, toEnable);
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> s = base.GetInfoText();
        s.Insert(0, $"Can store up to: {localRes.capacity} resources");
        return s;
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageBSave();
        (clickable as StorageBSave).canStore = CanStore;
        return base.Save(clickable);
    }

    public override void Load(ClickableObjectSave save)
    {
        CanStore = (save as StorageBSave).canStore;
        SceneRefs.JobQueue.storages.Add(this);
        base.Load(save);
        if (SceneRefs.ObjectFactory.CenterElevatorIds.Contains(id))
        {
            MyGrid.UnlockLevel(this, GetPos().y);
        }
    }
    #endregion

    public override void FinishBuild()
    {
        localRes.ammounts = new();
        ((IStorage)this).FinishStorageConstruction();
        base.FinishBuild();
    }

    #region Storing
    public override void Store(Human human, int transferPerTick)
    {
        base.Store(human, transferPerTick);
    }

    /// <summary>
    /// <inheritdoc/> 
    /// And adds logic to enforce <see cref="canStore"/>.
    /// </summary>
    /// <param name="request"><inheritdoc/></param>
    /// <param name="h"><inheritdoc/></param>
    /// <param name="mod"><inheritdoc/></param>
    public override void RequestRes(Resource request, Human h, int mod)
    {
        if (constructed && mod == 1)
        {
            int spaceToStore = localRes.capacity.currentValue - localRes.Future().Sum();
            Resource transferRes = new();
            for (int i = 0; i < request.types.Count && spaceToStore > 0; i++)
            {
                if (CanStore[ResFluidTypes.GetResourceIndex(request.types[i])])
                {
                    transferRes.types.Add(request.types[i]);
                    if (spaceToStore > request.ammounts[i])
                    {
                        transferRes.ammounts.Add(request.ammounts[i]);
                        spaceToStore -= request.ammounts[i];
                        MyRes.globalStorageSpace -= request.ammounts[i];
                    }
                    else
                    {
                        transferRes.ammounts.Add(spaceToStore);
                        MyRes.globalStorageSpace -= spaceToStore;
                        break;
                    }
                }
            }
            request = transferRes;
        }
        base.RequestRes(request, h, mod);
    }

    #endregion

    #region Deconstruction
    public override void OrderDeconstruct()
    {
        /*if (main)
            print("can't order destroy");*/
    }
    public override Chunk Deconstruct(GridPos instantPos)
    {
        /*if (main)
            print("can't destroy");*/
        return null;
    }
    #endregion
}
