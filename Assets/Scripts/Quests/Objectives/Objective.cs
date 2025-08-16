using System;
using System.Linq;
using Unity.Properties;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

namespace Objectives
{
    [Serializable]
    public abstract class Objective : IUpdatable, IQuestCompositor
    {
        [SerializeField, InspectorName("max")] protected int maxProgress;
        [NonSerialized] protected int currentProgress;
        [NonSerialized] protected Quest quest;

        [CreateProperty]
        public abstract string Descr { get; }

        [CreateProperty]
        public int CurrentProgress
        {
            get => currentProgress;
            set => currentProgress = value;
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
        #region Updates
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
        #endregion
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

        public abstract void Cancel(QuestController controller);

        public override string ToString()
        {
            return $"{Descr} {currentProgress}/{maxProgress}";
        }
    }

    public class DummyObjective : Objective
    {
        public override string Descr => throw new NotImplementedException();

        public override void Cancel(QuestController controller)
        {
            throw new NotImplementedException();
        }
    }
}