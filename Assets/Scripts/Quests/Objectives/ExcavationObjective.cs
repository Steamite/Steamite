using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        }

        public override bool UpdateProgress(object data, QuestController controller)
        {
            bool res = false;
            if (data is Rock rock && needToRemove.Remove(rock.GetPos()))
            {
                res = base.UpdateProgress(data, controller);
                if (res)
                    controller.ExcavationObjectives.Remove(this);
            }
            return res; 
        }

        public override void Load(int _currentProgress, Quest _quest, QuestController controller)
        {
            maxProgress = needToRemove.Count;
            foreach (var item in needToRemove)
            {
                if (MyGrid.GetGridItem(item) is Rock rock)
                {
                    GameObject.Instantiate(controller.ExcavationIcon, item.ToVec(3), Quaternion.identity, rock.transform);
                    rock.isQuest = true;
                }
            }
            controller.ExcavationObjectives.Add(this);
            base.Load(_currentProgress, _quest, controller);
        }
    }

    [Serializable]
    public class AnyExcavationObjective : Objective
    {
        public AnyExcavationObjective() { }
        public AnyExcavationObjective(int toRemove)
        {
            maxProgress = toRemove;
        }

        public override bool UpdateProgress(object data, QuestController controller)
        {
            bool res = false;
            if (data is Rock rock)
            {
                res = base.UpdateProgress(data, controller);
                if (res)
                    controller.AnyExcavationObjectives.Remove(this);
            }
            return res;
        }
        public override void Load(int _currentProgress, Quest _quest, QuestController controller)
        {
            base.Load(_currentProgress, _quest, controller);
            controller.AnyExcavationObjectives.Add(this);
        }
    }
}