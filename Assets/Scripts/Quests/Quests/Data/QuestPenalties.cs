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
        return $"Lose: {penaltyAmmount} Money";
    }
}

[Serializable]
public class TrustPenalty : QuestPenalty
{
    public TrustPenalty() : base() { }
    public TrustPenalty(int penalty) : base()
    {
        penaltyAmmount = penalty;
    }

    public override void GetPenalty()
    {
        SceneRefs.QuestController.Trust -= penaltyAmmount;
    }

    public override string ToString()
    {
        return $"- {penaltyAmmount} trust";
    }
}