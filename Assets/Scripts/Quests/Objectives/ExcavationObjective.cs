using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Objectives
{
    [Serializable]
    public class ExcavationObjective : Objective
    {
        public override string Descr => "Dig out marked rocks:";
        public List<GridPos> needToRemove = new();

        #region Constructors
        public ExcavationObjective() { }
        public ExcavationObjective(List<GridPos> toRemove, Quest _quest)
        {
            needToRemove = toRemove.ToList();
            maxProgress = toRemove.Count;
        }
        #endregion

        public override bool UpdateProgress(object data, QuestController controller)
        {
            bool res = false;
            if (data is Rock rock && needToRemove.Remove(rock.GetPos()))
            {
                res = base.UpdateProgress(data, controller);
                if (res)
                    Cancel(controller);
            }
            return res; 
        }

        public override void Load(int _currentProgressGlobal, Quest _quest, QuestController controller)
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
            base.Load(_currentProgressGlobal, _quest, controller);
        }

        public override void Cancel(QuestController controller)
        {
            controller.ExcavationObjectives.Remove(this);
            foreach (var item in needToRemove)
            {
                if(MyGrid.GetGridItem(item) is Rock rock)
                {
                    rock.isQuest = false;
                    GameObject.Destroy(rock.transform.GetChild(0).gameObject);
                }
            }
        }

    }

    [Serializable]
    public class AnyExcavationObjective : Objective
    {
        public override string Descr => "Dig out rocks:";
        #region Constructors
        public AnyExcavationObjective() { }
        public AnyExcavationObjective(int toRemove)
        {
            currentProgress = 0;
            maxProgress = toRemove;
        }
        #endregion

        public override bool UpdateProgress(object data, QuestController controller)
        {
            bool res = false;
            if (data is Rock rock)
            {
                res = base.UpdateProgress(data, controller);
                if (res)
                    Cancel(controller);
            }
            return res;
        }

        public override void Load(int _currentProgressGlobal, Quest _quest, QuestController controller)
        {
            base.Load(_currentProgressGlobal, _quest, controller);
            controller.AnyExcavationObjectives.Add(this);
        }

        public override void Cancel(QuestController controller)
        {
            controller.AnyExcavationObjectives.Remove(this);
        }
    }
}