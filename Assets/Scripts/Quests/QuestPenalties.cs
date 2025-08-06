using System;

[Serializable]
public class QuestPenalty : IQuestCompositor
{
    public virtual void GetPenalty()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class QuestMoneyPenalty : QuestPenalty
{
    /// <summary>Make it positive, this value is going to be subtracted.</summary>
    public int penaltyAmmount;

    public override void GetPenalty()
    {
        MyRes.ManageMoneyGlobal(-penaltyAmmount);
    }
}