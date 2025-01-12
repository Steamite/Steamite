using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Storage : Building
{
    public List<bool> canStore = new();

    #region Window
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Storage");
        base.OpenWindowWithToggle(info, toEnable);
        ((IUIElement)SceneRefs.infoWindow.constructedElement.Q<VisualElement>("Storage")).Fill(this);
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageBSave();
        (clickable as StorageBSave).canStore = canStore;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        canStore = (save as StorageBSave).canStore;
        if (constructed)
            SceneRefs.jobQueue.storages.Add(this);
    }
    #endregion

    #region Storing
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

    public void DestroyResource(ResourceType type, int ammountToDestroy)
    {
        localRes.stored.ammount[localRes.stored.type.IndexOf(type)] -= ammountToDestroy;
        UIUpdate(nameof(LocalRes));
    }
    #endregion
    public override List<string> GetInfoText()
    {
        List<string> s = base.GetInfoText();
        s.Insert(0, $"Can store up to: {localRes.stored.capacity} resources");
        return s;
    }

    ///////////////////////////////////////////////////
    ///////////////////Methods/////////////////////////
    ///////////////////////////////////////////////////
    public virtual void SetupStorage(Resource templateRes, JobQueue jQ)
    {
        localRes.stored.type = templateRes.type;
        while(localRes.stored.ammount.Count < templateRes.type.Count)
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
}