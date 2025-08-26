using UnityEngine;

namespace Objectives
{
    public class ResourceObjective : Objective
    {
        [SerializeField]public MoneyResource resource;

        public override string Descr => "";
        public override void Cancel(QuestController controller) { }
        public override void Load(int _currentProgressGlobal, Quest _quest, QuestController controller)
        {
            base.Load(_currentProgressGlobal, _quest, controller);
            resource.Init();
        }
    }
}