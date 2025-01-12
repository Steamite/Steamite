using Mono.Cecil;
using System;
using System.Linq;
using Unity.Properties;
using UnityEngine;

public abstract class StorageObject : ClickableObject
{
    /// <summary>Stored resouces, which also contain all requests.</summary>
    [SerializeField] protected StorageResource localRes = new();
    
    /// <inheritdoc/>
    [CreateProperty] public StorageResource LocalRes => localRes;
    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageObjectSave();
        (clickable as StorageObjectSave).resSave = new(localRes);
        (clickable as StorageObjectSave).gridPos = GetPos(); // used for not rounded values
        return base.Save(clickable);
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        localRes = new((save as StorageObjectSave).resSave);
        base.Load(save);
    }
    #endregion Saving

    #region Storage
    /// <summary>
    /// Implements store funcionality.
    /// </summary>
    /// <param name="h"><see cref="Human"/> that is storing resources.</param>
    /// <param name="transferPerTick">Max resources that can be transfered.</param>
    public abstract void Store(Human h, int transferPerTick);

    /// <summary>
    /// Moves resources from <see cref="localRes"/> to <see cref="Human.inventory"/>.<br/>
    /// </summary>
    /// <param name="h"><see cref="Human"/> that is taking resources.</param>
    /// <param name="transferPerTick">Max resources that can be transfered.</param>
    public virtual void Take(Human h, int transferPerTick)
    {
        MyRes.MoveRes(h.Inventory, localRes.stored, localRes.requests[localRes.carriers.IndexOf(h)], transferPerTick);
        UIUpdate(nameof(LocalRes));
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
            HumanActions.LookForNew(h);
        }
    }

    /// <inheritdoc cref="StorageResource.AddRequest(Resource, Human, int)"/>
    public virtual void RequestRes(Resource request, Human human, int mod)
    {
        localRes.AddRequest(request, human, mod);
    }
    /// <inheritdoc cref="StorageResource.LinkHuman(Human)"/>
    public virtual void TryLink(Human h)
    {
        localRes.LinkHuman(h);
    }
    #endregion Storage
}