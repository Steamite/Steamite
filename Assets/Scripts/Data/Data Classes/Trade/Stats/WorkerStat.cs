using UnityEngine;

namespace TradeData.Stats
{
    [CreateAssetMenu(fileName = "Workers", menuName = "Stats/Workers", order = 0)]
    public class WorkerStat : ColonyStat
    {
        const int WORKER_PER_LEVEL = 2;
        public WorkerStat() : base()
        {

        }

        public override void DoStat()
        {
            for(int i = 0; i < CurrentState * WORKER_PER_LEVEL; i++)
                SceneRefs.humans.AddHuman();
        }

        public override string GetText(int state)
        {
            return $"{state * WORKER_PER_LEVEL} new workers arrive each week.";
        }

        public override string GetText(bool complete)
        {
            if (complete)
                return "Affects how many new workers will arrive each week.";
            else
                return $"+ {CurrentState * WORKER_PER_LEVEL}";
        }
    }
}
