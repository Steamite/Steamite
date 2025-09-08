using System;
using UnityEngine;

[Serializable]
public class QuestReward : IQuestCompositor
{
    public virtual void ObtainReward()
    {
        throw new NotImplementedException();
    }

    public virtual void Init()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class QuestResourceReward : QuestReward
{
    public MoneyResource resource = new();

    public override void ObtainReward()
    {
        MyRes.DeliverToElevator(resource);
        MyRes.ManageMoneyGlobal(+resource.Money);
    }

    public override void Init()
    {
        resource.Init();
    }
    public override string ToString()
    {
        return $"Obtain: {resource}";
    }
}

public class TrustReward : QuestReward
{
    public int gainAmmount;
    public TrustReward() : base(){ }
    public TrustReward(int i) : base()
    {
        gainAmmount = i;
    }
    public override void ObtainReward()
    {
        SceneRefs.QuestController.Trust += gainAmmount;
    }

    public override void Init() {}

    public override string ToString()
    {
        return $"+ {gainAmmount} trust";
    }
}

public class GameWinReward : QuestReward
{
    public override void ObtainReward()
    {
        ((QuestController)SceneRefs.QuestController).endMenu.GetComponent<IUIElement>().Open(true);
    }
    public override void Init()
    {
        Debug.Log("This is the last Order!");
    }
}