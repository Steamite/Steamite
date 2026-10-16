using System;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Humans
{
    public struct Human
    {
        public uint posAndRot;
        /// <summary>Specialization of the worker, provides bonuses in certain areas. (WIP)</summary>
        public Specializations specialization;
        
        /// <summary>If it's time to go to sleep.</summary>
        public bool nightTime;
        /// <summary>Fail-save to prevent looking for a job multiple times at once, Remove after testing.</summary>
        public bool lookingForAJob;


        public Human(int index, uint _posAndRot)
        {
            posAndRot = _posAndRot;
            specialization = Specializations.Worker;
            nightTime = false;
            lookingForAJob = false;
        }
    }
}
