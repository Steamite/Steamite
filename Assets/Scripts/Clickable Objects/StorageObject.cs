using System;
using System.Linq;
using Unity.Properties;
using UnityEngine;

public class StorageObject : ClickableObject
{
    [SerializeField]
    protected StorageResource localRes = new();
    [CreateProperty]
    public StorageResource LocalRes => localRes;
    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageObjectSave();
        (clickable as StorageObjectSave).resSave = new(localRes);
        (clickable as StorageObjectSave).gridPos = GetPos(); // used for not rounded values
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        localRes = new((save as StorageObjectSave).resSave);
        base.Load(save);
    }
    #endregion Saving

    #region Storage
    public virtual void Store(Human h, int transferPerTick)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes resource from the object(chunk, building)
    /// </summary>
    /// <param name="resId"></param>
    /// <param name="ammount"></param>
    public virtual void Take(Human h, int transferPerTick)
    {
        MyRes.MoveRes(h.Inventory, localRes.stored, localRes.requests[localRes.carriers.IndexOf(h)], transferPerTick);
        UpdateWindow(nameof(LocalRes));
        if (localRes.requests[localRes.carriers.IndexOf(h)].ammount.Sum() == 0)
        {
            localRes.RemoveRequest(h);
            JobData data = PathFinder.FindPath(new() { h.destination }, h);
            if (data.interest != null)
            {
                data.job = JobState.Supply;
                h.SetJob(data);
                h.ChangeAction(HumanActions.Move);
                return;
            }
            h.Idle();
        }
    }

    public virtual void RequestRes(Resource request, Human h, int mod)
    {
        localRes.AddRequest(request, h, mod);
    }

    public virtual void TryLink(Human h)
    {
        localRes.LinkHuman(h);
    }
    #endregion Storage
}