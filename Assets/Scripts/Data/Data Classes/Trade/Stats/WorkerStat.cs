using UnityEngine;

namespace TradeData.Stats
{
    [CreateAssetMenu(fileName = "Workers", menuName = "UI Data/Trade/Stats/Workers", order = 0)]
    public class WorkerStat : ColonyStat
    {
        [Header("Bonus")]
        [SerializeField] int workersPerLevel = 2;

        public WorkerStat() : base()
        {

        }

        public override void DoStat()
        {
            for (int i = 0; i < CurrentState * workersPerLevel; i++)
                SceneRefs.humans.AddHuman();
        }

        public override string GetText(int state)
        {
            return $"{state * workersPerLevel} new workers arrive each week.";
        }

        public override string GetText(bool complete)
        {
            if (complete)
                return "Affects how many new workers will arrive each week.";
            else
                return $"+ {CurrentState * workersPerLevel}";
        }
    }
}
