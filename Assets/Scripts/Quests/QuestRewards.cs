using System;

[Serializable]
public abstract class QuestReward
{
    public abstract void ObtainReward();
}

[Serializable]
public class QuestResourceReward : QuestReward
{
    public MoneyResource resource;

    public override void ObtainReward()
    {
        MyRes.DeliverToElevator(resource);
    }
}