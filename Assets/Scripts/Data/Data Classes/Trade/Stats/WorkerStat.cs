using UnityEngine;

namespace TradeData.Stats
{
    [CreateAssetMenu(fileName = "Workers", menuName = "Stats/Workers", order = 0)]
    public class WorkerStat : ColonyStat
    {
        public WorkerStat() : base()
        {

        }

        public override void DoStat()
        {
            for(int i = 0; i < currentState; i++)
                SceneRefs.humans.AddHuman();
        }

        public override string GetText(int state)
        {
            return $"{state * 2} new workers arrive each week.";
        }

        public override string GetText(bool complete)
        {
            if (complete)
                return "Affects how many new workers will arrive each week.";
            else
                return "+ 2";
        }
    }
}
