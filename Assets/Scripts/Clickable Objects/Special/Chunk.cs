using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk : StorageObject
{
    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
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

    #region Window
    protected override void SetupWindow(InfoWindow info)
    {
        base.SetupWindow(info);
        info.SwitchMods(InfoMode.Chunk);
    }

    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info); 
        info.chunk.Q<Label>("").text = $"{string.Join(',', localRes.carriers.Select(q => q.name))}";
        info.FillResourceList(info.chunk.Q<VisualElement>("Yield"), localRes.stored);
    }
    #endregion
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

    public override void RequestRes(Resource request, Human h, int mod)
    {
        base.RequestRes(request, h, mod);
        OpenWindow();
    }
    public override void Load(ClickableObjectSave save)
    {
        transform.GetChild(1).GetComponent<MeshRenderer>().material.color = (save as ChunkSave).resColor.ConvertColor();
        base.Load(save);
    }

    public override void Store(Human h, int transferPerTick)
    {
        Debug.LogError("Can't store in a chunk");
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
            MyRes.MoveRes(h.inventory, localRes.stored, localRes.requests[index], transferPerTick);
            if (localRes.requests[index].ammount.Sum() == 0)
            {
                if (h.inventory.capacity - h.inventory.ammount.Sum() == 0)
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

    ///////////////////////////////////////////////////
    ///////////////////Methods/////////////////////////
    ///////////////////////////////////////////////////    
    void FindS(Human h)
    {
        MyRes.FindStorage(h);
        if (h.jData.interest)
        {
            h.jData.job = JobState.Supply;
            h.ChangeAction(HumanActions.Move);
        }
        else
            Debug.LogError("Fuck, where do I store this?");
    }

    public void Init(Resource res)
    {
        UniqueID();
        localRes.stored = res;
        MyRes.UpdateResource(localRes.stored, 1);
        GameObject.FindWithTag("Humans").GetComponent<JobQueue>().AddJob(JobState.Cleanup, this);
        name = name.Replace("(Clone)", " ");
        MyGrid.chunks.Add(this);
    }
}