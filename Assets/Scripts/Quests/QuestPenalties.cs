using System;

public abstract class QuestPenalty
{
    public abstract void GetPenalty();
}

public class QuestMoneyPenalty : QuestPenalty
{
    /// <summary>Make it positive, this value is going to be subtracted.</summary>
    public int penaltyAmmount;

    public override void GetPenalty()
    {
        MyRes.ManageMoneyGlobal(-penaltyAmmount);
    }
}