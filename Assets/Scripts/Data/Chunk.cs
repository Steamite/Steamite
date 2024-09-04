using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Chunk : StorageObject
{
    public override void Store(Human h, int transferPerTick)
    {
        Debug.LogError("Can't store in a chunk");
    }
    public override void Take(Human h, int transferPerTick)
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
            int ammountStored = localRes.stored.ammount.Sum();
            if (ammountStored == 0)
            {
                transform.parent.parent.parent.parent.GetComponent<GridTiles>().Remove(this);
                MyGrid.chunks.Remove(this);
                Destroy(gameObject);
            }
        }
    }
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
    public void Create(Resource res, bool addToMainRes)
    {
        UniqueID();
        localRes.stored = res;
        if (addToMainRes)
            MyRes.UpdateResource(localRes.stored, 1);
        GameObject.FindWithTag("Humans").GetComponent<JobQueue>().AddJob(JobState.Cleanup, this);
        name = name.Replace("(Clone)", " ");
        MyGrid.chunks.Add(this);
    }
    public override void UniqueID()
    {
        CreateNewId(MyGrid.chunks.Select(q => q.id).ToList());
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Chunk);
    }
    public override InfoWindow OpenWindow(bool first)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(first).GetComponent<InfoWindow>()) != null)
        {
            // set window mod to Ore Info
            if (first)
            {
                info.SwitchMods(3, name);
            }
            info.transform.GetChild(1).GetChild(3).GetChild(0)
                .GetComponent<TMP_Text>().text 
                    = $"Human: {string.Join(",", localRes.carriers.Select(q => q.name))} \nResources: {MyRes.GetDisplayText(localRes.stored)}";
        }
        return null;
    }
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
}