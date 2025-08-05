using System;
using System.Collections.Generic;
using System.Linq;
namespace Objectives
{
    [Serializable]
    public class ExcavationObjective : Objective
    {
        public List<GridPos> needToRemove = new();
        public ExcavationObjective() { }
        public ExcavationObjective(List<GridPos> toRemove, Quest _quest)
        {
            needToRemove = toRemove.ToList();
            maxProgress = toRemove.Count;
            quest = _quest;
        }

        public override void UpdateProgress(object data)
        {
            if (data is Rock rock && needToRemove.Remove(rock.GetPos()))
                CurrentProgress += 1;
        }
    }

    [Serializable]
    public class ExcavationObjectiveRandom : Objective
    {
        public ExcavationObjectiveRandom() { }
        public ExcavationObjectiveRandom(int toRemove, Quest _quest)
        {
            maxProgress = toRemove;
            quest = _quest;
        }

        public override void UpdateProgress(object data)
        {
            if (data is Rock)
                CurrentProgress += 1;
        }
    }
}