using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Serves as a store house for the colony. <br/>
/// Each storage should have all resource types in the <see cref="StorageObject.localRes"/></summary>
public class Storage : Building
{
    #region Variables
    /// <summary>List containing which resources can be stored.</summary>
    public List<bool> canStore = new();
    #endregion

    #region Window
    /// <inheritdoc/>
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Storage");
        base.OpenWindowWithToggle(info, toEnable);
        ((IUIElement)SceneRefs.infoWindow.constructedElement.Q<VisualElement>("Storage")).Fill(this);
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageBSave();
        (clickable as StorageBSave).canStore = canStore;
        return base.Save(clickable);
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        canStore = (save as StorageBSave).canStore;
        if (constructed)
            SceneRefs.jobQueue.storages.Add(this);
    }
    #endregion

    #region Storing
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
                if (canStore[(int)request.type[i]])
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

    /// <summary>
    /// Removes the resurce from existence.
    /// </summary>
    /// <param name="type">Type of resource.</param>
    /// <param name="ammountToDestroy">Ammount of resources.</param>
    public void DestroyResource(ResourceType type, int ammountToDestroy)
    {
        localRes.stored.ammount[localRes.stored.type.IndexOf(type)] -= ammountToDestroy;
        UIUpdate(nameof(LocalRes));
    }

    /// <summary>
    /// Assigns all resource types and setups base storage resources.
    /// </summary>
    /// <param name="templateRes">Clones resource types.</param>
    /// <param name="jQ">Reference to register this object to global storage list.</param>
    public virtual void SetupStorage(Resource templateRes, JobQueue jQ)
    {
        localRes.stored.type = templateRes.type;
        while (localRes.stored.ammount.Count < templateRes.type.Count)
        {
            localRes.stored.ammount.Add(400);
        }
        canStore = new();
        for (int i = 0; i < localRes.stored.ammount.Count; i++)
        {
            canStore.Add(true);
        }
        jQ.storages.Add(this);
        localRes.stored.capacity = 5000;
    }
    #endregion

    #region Placing
    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> s = base.GetInfoText();
        s.Insert(0, $"Can store up to: {localRes.stored.capacity} resources");
        return s;
    }
    #endregion
}