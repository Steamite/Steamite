using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk : StorageObject
{
    #region Basic Operations
    public override void UniqueID()
    {
        CreateNewId(MyGrid.chunks.Select(q => q.id).ToList());
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Chunk);
    }
    public override GridPos GetPos()
    {
        return new(transform.position.x, (transform.position.y - 1) / 2, transform.position.z);
    }

    #endregion

    #region Window
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Chunk);
        return info;
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ChunkSave();
        if (gameObject)
        {
            (clickable as ChunkSave).resColor = new MyColor(transform.GetChild(1).GetComponent<MeshRenderer>().material.color);
            return base.Save(clickable);
        }
        return null;
    }
    public override void Load(ClickableObjectSave save)
    {
        transform.GetChild(1).GetComponent<MeshRenderer>().material.color = (save as ChunkSave).resColor.ConvertColor();
        base.Load(save);
    }
    #endregion

    #region Storage
    public override void Store(Human h, int transferPerTick)
    {
        throw new NotSupportedException();
    }
    public override void Take(Human h, int transferPerTick)
    {
        if (h.destination != null)
        {
            base.Take(h, transferPerTick);
        }
        else
        {
            int index = localRes.carriers.IndexOf(h);
            MyRes.MoveRes(h.Inventory, localRes.stored, localRes.requests[index], transferPerTick);
            UIUpdate(nameof(LocalRes));
            if (localRes.requests[index].ammount.Sum() == 0)
            {
                if (h.Inventory.capacity - h.Inventory.ammount.Sum() == 0)
                {
                    FindS(h);
                }
                else
                {
                    if (HumanActions.HandleCases(h.transform.parent.parent.GetComponent<JobQueue>(), h, JobState.Cleanup))
                        FindS(h);
                }
            }
        }
        if (localRes.stored.ammount.Sum() == 0)
        {
            SceneRefs.gridTiles.DestroyUnselect(this);
            MyGrid.chunks.Remove(this);
            Destroy(gameObject);
        }
    }
    public override void RequestRes(Resource request, Human h, int mod)
    {
        base.RequestRes(request, h, mod);
        UIUpdate(nameof(LocalRes));
    }
    void FindS(Human h)
    {
        MyRes.FindStorage(h);
        if (!h.Job.interest)
            Debug.LogError("Fuck, where do I store this?");
    }
    #endregion
    public void Init(Resource res)
    {
        UniqueID();
        localRes.stored = res;
        MyRes.UpdateResource(localRes.stored, 1);
        name = name.Replace("(Clone)", " ");
        MyGrid.chunks.Add(this);
    }
}