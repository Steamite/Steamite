using UnityEngine;

public class Elevator : Storage
{
    public bool main = false;
    public override void OrderDeconstruct()
    {
        if(main)
            print("can't order destroy");
    }
    public override Chunk Deconstruct(Vector3 instantPos)
    {
        if (main)
            print("can't destroy");
        return null;
    }
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
}
