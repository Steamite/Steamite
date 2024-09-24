using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storage : Building
{
    public List<bool> canStore = new();
    protected override void Awake()
    {
        base.Awake();
        for(int i = 0; i < localRes.stored.ammount.Count; i++)
        {
            canStore.Add(true);
        }
    }

    ///////////////////////////////////////////////////
    //---------------Saving & Loading----------------//
    ///////////////////////////////////////////////////
    
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if(clickable == null)
            clickable = new StorageBSave();
        (clickable as StorageBSave).canStore = canStore;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        canStore = (save as StorageBSave).canStore;
        if (build.constructed)
            GameObject.Find("Humans").GetComponent<JobQueue>().storages.Add(this);
    }

    ///////////////////////////////////////////////////
    /////---------------Info Window----------------////
    ///////////////////////////////////////////////////
    
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            if (setUp)
            {
                StorageAssign SA = info.cTransform.GetChild(1).GetComponent<StorageAssign>();
                SA.building = this;
                SA.SetStorageButton(GetComponent<Storage>().canStore, info.cTransform);
                info.cTransform.GetChild(1).gameObject.SetActive(true); // storage
                info.cTransform.GetChild(1).GetChild(0).gameObject.SetActive(true); // storage
                info.cTransform.GetChild(1).GetChild(1).gameObject.SetActive(false); // storage
            }
            info.cTransform.GetChild(1).GetComponent<StorageAssign>().UpdateAmmounts();
        }
        return info;
    }
    
    public override List<string> GetInfoText()
    {
        List<string> s = base.GetInfoText();
        s.Insert(0, $"Can store up to: {localRes.stored.capacity} resources");
        return s;
    }
    public override void RequestRes(Resource request, Human h, int mod)
    {
        if(build.constructed && mod == 1)
        {
            int spaceToStore = localRes.stored.capacity - localRes.Future(false).ammount.Sum();
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

    public virtual void SetupStorage(Resource templateRes, JobQueue jQ)
    {
        localRes.stored.type = templateRes.type;
        jQ.storages.Add(this);
    }
}