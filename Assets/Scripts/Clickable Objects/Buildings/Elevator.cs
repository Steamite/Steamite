using System;
using UnityEngine;

public class Elevator : Storage
{
    public bool main = false;

    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new StorageBSave();
        (clickable as StorageBSave).canStore = canStore;
        (clickable as StorageBSave).main = main;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        canStore = (save as StorageBSave).canStore;
        main = (save as StorageBSave).main;
        base.Load(save);
    }

    public override void Store(Human human, int transferPerTick)
    {
        base.Store(human, transferPerTick);
    }

    public override void OrderDeconstruct()
    {
        if(main)
            print("can't order destroy");
    }
    public override Chunk Deconstruct(GridPos instantPos)
    {
        if (main)
            print("can't destroy");
        return null;
    }

    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            if (setUp)
            {
                info.currentTab = 0;
            }
            info.cTransform.GetChild(1).GetChild(0).gameObject.SetActive(true); // storage
            info.cTransform.GetChild(1).GetChild(1).gameObject.SetActive(false); // storage
        }
        return info;
    }
}
