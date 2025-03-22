using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TradeData.Stats
{
    [Serializable]
    public struct StatData
    {
        [SerializeField] int currentState;
        [SerializeField] int maxState;
    }
    /// <summary>Base class that each type of state must inherit from.</summary>
    public abstract class ColonyStat : ScriptableObject
    {
		#region Variables
		public const int MAX_STAT_LEVEL = 5;

        [Header("Base")]
        /// <summary>Name of the stat in display.</summary>
        public string displayName;
        /// <summary>Current state, starting value set whe initializing a new game.</summary>
        public int CurrentState { get; private set; }
        public int MaxState { get; private set; }
        /// <summary>Max possible state, set when initialining a new game.</summary>

        /// <summary>Resources needed for upgrading to that level.</summary>
        [SerializeField]public List<Resource> resourceUpgradeCost = new() {};
		#endregion


		/// <summary>Provides production based on the <see cref="CurrentState"/>.</summary>
		public abstract void DoStat();

        /// <summary>Summary for one stat level.</summary>
        public abstract string GetText(int state);

        /// <summary>
        /// Returns text for the small label on top, or if <paramref name="complete"/> is true the complete summary.
        /// </summary>
        /// <param name="complete">If true, returns complete summary else only number and icon</param>
        public abstract string GetText(bool complete);

		/// <summary>
		/// If the next level is affordable.
		/// </summary>
		/// <returns>If the next level is affordable</returns>
		public bool CanAfford()
        {
            if (CurrentState == MaxState)
                return false;

            return 
                resourceUpgradeCost[CurrentState].capacity <= MyRes.Money &&
                MyRes.CanAfford(resourceUpgradeCost[CurrentState]);
                 
		}

		public void Upgrade()
		{
            Debug.Assert(CanAfford(), "Cannot Afford");
            MyRes.TakeFromGlobalStorage(resourceUpgradeCost[CurrentState]);
            MyRes.UpdateMoney(-resourceUpgradeCost[CurrentState].capacity);
            CurrentState++;
		}

        /// <summary>
        /// Used when loading a save, or creating a new game.
        /// </summary>
        /// <param name="_currentState">Loaded state.</param>
		public void LoadState(int _currentState, int _maxState)
		{
			CurrentState = _currentState;
            MaxState = _maxState;
		}
	}
}
