using Mono.Cecil;
using System;

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