using System;

[Serializable]
public class QuestReward : IQuestCompositor
{
    public virtual void ObtainReward()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class QuestResourceReward : QuestReward
{
    public MoneyResource resource;

    public override void ObtainReward()
    {
        resource.Init();
        MyRes.DeliverToElevator(resource);
        MyRes.ManageMoneyGlobal(+resource.Money);
    }

    public override string ToString()
    {
        return $"Obtain: {resource}";
    }
}