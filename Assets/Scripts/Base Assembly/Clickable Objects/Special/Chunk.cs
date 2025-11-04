using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Holds resources that are laying on a Road. <br/>
/// Created by destroying <see cref="Rock"/>s and <see cref="Building"/>s.
/// </summary>
public class Chunk : StorageObject
{
    #region Basic Operations
    /// <summary>Creates a list from <see cref="MyGrid.chunks"/></summary>
    public override void UniqueID()
    {
        CreateNewId(MyGrid.chunks.Select(q => q.id).ToList());
    }
    /// <inheritdoc/>
    public override GridPos GetPos() => new(transform.position.x, (transform.position.y - 1) / 2, transform.position.z);

    #endregion

    #region Window
    /// <inheritdoc/>
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Chunk);
        return info;
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ChunkSave();
        if (gameObject)
        {
            (clickable as ChunkSave).resColor = new MyColor(transform.GetChild(1).GetComponent<MeshRenderer>().material.color);
            base.Save(clickable);
            /*ChunkSave save = clickable as ChunkSave;
            if (save.resSave.Requests.Count > 0)
            {
                Resource resource = new();
                foreach (var item in save.resSave.Requests[0].types)
                {
                    resource.ManageSimple(item, save.resSave.Requests[0][item], true);
                }
                //save.resSave.ammounts[0] -= 1;
                save.resSave.Requests[0] = new(save.resSave.Requests[0].types.ToList(), save.resSave.Requests[0].ammounts.Select(q => q).ToList());
                try
                {
                    Debug.Log(JsonConvert.SerializeObject(save.resSave.Requests, Formatting.Indented));
                }
                catch(Exception e)
                {
                    Debug.LogError("can't serialize: " + e);
                }
            }*/

            Debug.Log((clickable as StorageObjectSave).resSave.Requests.Count);
            return clickable;
        }
        return null;
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        localRes.Load((save as StorageObjectSave).resSave);
        transform.GetChild(1).GetComponent<MeshRenderer>().material.color = (save as ChunkSave).resColor.ConvertColor();
        base.Load(save);
    }
    #endregion

    #region Storage
    /// <summary>
    /// Throws an error, it's not allowed to store resources in a chunk.
    /// </summary>
    public override void Store(Human h, int transferPerTick) => throw new NotSupportedException();

    /// <summary>
    /// <inheritdoc/>
    /// If there are no resources left, destroyes the chunk.
    /// </summary>
    /// <param name="h"><inheritdoc/></param>
    /// <param name="transferPerTick"><inheritdoc/></param>
    public override void Take(Human h, int transferPerTick)
    {
        if (h.destination != null)
        {
            base.Take(h, transferPerTick);
        }
        else
        {
            int index = localRes.carriers.IndexOf(h);
            MyRes.MoveRes(h.Inventory, localRes, localRes.requests[index], transferPerTick);
            UIUpdate(nameof(LocalRes));
            if (localRes.requests[index].Sum() == 0)
            {
                if (h.Inventory.capacity.currentValue - h.Inventory.Sum() == 0)
                {
                    FindS(h);
                }
                else
                {
                    // If no other chunks were found try to find a storage.
                    // Else collect the other chunks.
                    if (!HumanActions.HandleJobTypes(SceneRefs.JobQueue, h, JobState.Cleanup))
                        FindS(h);
                }
            }
        }
        if (localRes.Sum() == 0)
        {
            SceneRefs.GridTiles.DestroyUnselect(this);
            MyGrid.chunks.Remove(this);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// Also updates UI.
    /// </summary>
    /// <param name="resource"><inheritdoc/></param>
    /// <param name="h"><inheritdoc/></param>
    /// <param name="mod"><inheritdoc/></param>
    public override void RequestRes(Resource resource, Human h, int mod)
    {
        resource = new Resource(resource);
        base.RequestRes(resource, h, mod);
        UIUpdate(nameof(LocalRes));
    }

    /// <summary>
    /// Finds storage that can store local resources.
    /// </summary>
    /// <param name="h"><see cref="Human"/> that is looking for a place to deposite resources.</param>
    void FindS(Human h)
    {
        MyRes.FindStorage(h);
        if (!h.Job.interest)
            Debug.LogError("Fuck, where do I store this?");
    }
    #endregion

    /// <summary>
    /// Assigns ID, resources and self to <see cref="MyGrid.chunks"/>.<br/> 
    /// Updates global resource counter. And fix object name.
    /// </summary>
    /// <param name="res"></param>
    /// <param name="updateGlobalResource">Do you want to add the resources to the global resource counter?</param>
    public void Init(Resource res, bool updateGlobalResource)
    {
        UniqueID();
        localRes = new(res);
        if (updateGlobalResource)
            MyRes.UpdateResource(localRes, true);
        objectName = objectName.Replace("(Clone)", " ");
        MyGrid.chunks.Add(this);
    }
}