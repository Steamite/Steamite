using System;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Objectives
{
    [Serializable]
    public class Objective : IUpdatable
    {
        [SerializeField, InspectorName("max")] protected int maxProgress;
        [NonSerialized] protected int currentProgress;
        [NonSerialized] protected Quest quest;


        [CreateProperty]
        public int CurrentProgress
        {
            get => currentProgress;
            set
            {
                currentProgress = value;
                UIUpdate(nameof(CurrentProgress));
                if (currentProgress == maxProgress)
                {
                    if (quest.objectives.All(q => q.maxProgress == q.currentProgress))
                        quest.Complete(true);
                }
            }
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

        public virtual void UpdateProgress(object data) { throw new NotImplementedException(); }
    }
}