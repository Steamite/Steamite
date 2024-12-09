using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storage : Building
{
    public List<bool> canStore = new();

    #region Window
    protected override void SetupWindow(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Storage");

        base.SetupWindow(info, toEnable);
        /*StorageAssign SA = info.cTransform.GetChild(1).GetComponent<StorageAssign>();
        SA.building = this;
        SA.SetStorageButton(GetComponent<Storage>().canStore, info.cTransform);
        info.cTransform.GetChild(1).gameObject.SetActive(true); // storage
        info.cTransform.GetChild(1).GetChild(0).gameObject.SetActive(true); // storage
        info.cTransform.GetChild(1).GetChild(1).gameObject.SetActive(false); // storage*/
    }
    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        //info.constructed.Q<>("").UpdateAmmounts();
    }
    #endregion
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
        if (build.constructed)
            SceneRefs.humans.GetComponent<JobQueue>().storages.Add(this);
    }

    public override void RequestRes(Resource request, Human h, int mod)
    {
        if (build.constructed && mod == 1)
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
            localRes.stored.ammount.Add(0);
        }
        canStore = new();
        for (int i = 0; i < localRes.stored.ammount.Count; i++)
        {
            canStore.Add(true);
        }
        jQ.storages.Add(this);
    }
}