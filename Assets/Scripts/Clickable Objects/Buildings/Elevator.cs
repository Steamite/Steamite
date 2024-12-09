using System;
using System.Collections.Generic;
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

    #region Window
    protected override void SetupWindow(InfoWindow info, List<String> strings)
    {
        base.SetupWindow(info, strings);
    }

    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        /* info.cTransform.GetChild(1).GetChild(0).gameObject.SetActive(true); // storage
            info.cTransform.GetChild(1).GetChild(1).gameObject.SetActive(false); // storage*/
    }
    #endregion
}
