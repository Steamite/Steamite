using System;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Objectives
{
    [Serializable]
    public class Objective : IUpdatable, IQuestCompositor
    {
        [SerializeField, InspectorName("max")] protected int maxProgress;
        [NonSerialized] protected int currentProgress;
        [NonSerialized] protected Quest quest;


        [CreateProperty]
        public int CurrentProgress
        {
            get => currentProgress;
        }

        [CreateProperty]
        public int MaxProgress
        {
            get => maxProgress;
            set
            {
                maxProgress = value;
                UIUpdate(nameof(MaxProgress));
            }
        }



        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        public virtual bool UpdateProgress(object data, QuestController controller) 
        {
            currentProgress++;
            UIUpdate(nameof(CurrentProgress));
            if(currentProgress == maxProgress)
            {
                if (quest.objectives.All(q => q.maxProgress == q.currentProgress))
                    quest.Complete(true, controller);
                return true;
            }
            return false;
        }

        public virtual void Load(int _currentProgress, Quest _quest, QuestController controller)
        {
            quest = _quest;
            currentProgress = _currentProgress;
        }
    }
}