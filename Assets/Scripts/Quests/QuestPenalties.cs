using System;
using UnityEngine;

[Serializable]
public class QuestPenalty : IQuestCompositor
{
    /// <summary>Make it positive, this value is going to be subtracted.</summary>
    [SerializeField] public int penaltyAmmount;
    public virtual void GetPenalty()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class QuestMoneyPenalty : QuestPenalty
{
    public override void GetPenalty()
    {
        MyRes.ManageMoneyGlobal(-penaltyAmmount);
    }

    public override string ToString()
    {
        return $"Money: {penaltyAmmount}";
    }
}

[Serializable]
public class TrustPenalty : QuestPenalty
{
    public override void GetPenalty()
    {
        MyRes.resDataSource.Trust -= penaltyAmmount;
    }

    public override string ToString()
    {
        return $"Lose trust: {penaltyAmmount}";
    }
}