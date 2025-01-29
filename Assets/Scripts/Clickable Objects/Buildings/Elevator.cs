using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// <inheritdoc cref="IStorage"/>
/// </summary>
public class Elevator : Building, IStorage
{
    #region Variables
    public bool main = false;
    /// <inheritdoc/>
    [CreateProperty] public List<bool> CanStore { get; set; } = new();
    public StorageResource LocalResources => localRes;
    #endregion

    #region Window
    /// <inheritdoc/>
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Storage");
        base.OpenWindowWithToggle(info, toEnable);
    }
    
    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> s = base.GetInfoText();
        s.Insert(0, $"Can store up to: {localRes.stored.capacity} resources");
        return s;
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageBSave();
        (clickable as StorageBSave).canStore = CanStore;
        (clickable as StorageBSave).main = main;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        CanStore = (save as StorageBSave).canStore;
        main = (save as StorageBSave).main;
        base.Load(save);
    }
    #endregion

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
            int spaceToStore = localRes.stored.capacity - localRes.Future().ammount.Sum();
            Resource transferRes = new();
            for (int i = 0; i < request.type.Count && spaceToStore > 0; i++)
            {
                if (CanStore[(int)request.type[i]])
                {
                    transferRes.type.Add(request.type[i]);
                    if (spaceToStore > request.ammount[i])
                    {
                        transferRes.ammount.Add(request.ammount[i]);
                        spaceToStore -= request.ammount[i];
                        MyRes.globalStorageSpace -= request.ammount[i];
                    }
                    else
                    {
                        transferRes.ammount.Add(spaceToStore);
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
        if(main)
            print("can't order destroy");
    }
    public override Chunk Deconstruct(GridPos instantPos)
    {
        if (main)
            print("can't destroy");
        return null;
    }
    #endregion
}
