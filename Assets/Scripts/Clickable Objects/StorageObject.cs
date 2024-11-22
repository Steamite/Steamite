using System;
using System.Linq;

public class StorageObject : ClickableObject
{
    public StorageResource localRes = new();
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
        MyRes.MoveRes(h.inventory, localRes.stored, localRes.requests[localRes.carriers.IndexOf(h)], transferPerTick);
        if (localRes.requests[localRes.carriers.IndexOf(h)].ammount.Sum() == 0)
        {
            localRes.RemoveRequest(h);
            h.jData = PathFinder.FindPath(new() { h.destination }, h);
            if (h.jData.interest != null)
            {
                h.jData.job = JobState.Supply;
                h.ChangeAction(HumanActions.Move);
                return;
            }
            h.Idle();
            // store maybe?
        }
    }
    #endregion Storage

    public virtual void RequestRes(Resource request, Human h, int mod)
    {
        localRes.AddRequest(request, h, mod);
    }
    public virtual void TryLink(Human h)
    {
        localRes.LinkHuman(h);
    }
}