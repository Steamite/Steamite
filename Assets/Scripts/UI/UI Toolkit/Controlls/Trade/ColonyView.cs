using InfoWindowElements;
using System.Collections.Generic;
using TradeData.Locations;
using TradeData.Stats;
using UnityEngine;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
	[UxmlElement]
	public partial class ColonyView : VisualElement, IInitiableUI
	{
		#region Variables
		VisualTreeAsset statAsset;
		[UxmlAttribute] VisualTreeAsset StatAsset { get => statAsset; set { statAsset = value; } }

		#endregion

		public ColonyView() : base()
		{
			VisualElement element;
			Label label;
			for (int i = 0; i < 2; i++)
			{
				element = new();
				label = new(i == 1 ? "Passive Production" : "Stats");
				label.name = "Header";
				element.Add(label);
				element.Add(new());
				element.ElementAt(1).name = "Stats";
				Add(element);
			}
			Debug.Log("Constructing colony view");
		}

		public void Init()
		{
			CreateStats(0, UIRefs.trading.colonyLocation.stats);
			CreateStats(1, UIRefs.trading.colonyLocation.production);
			style.display = DisplayStyle.None;
		}

		void CreateStats(int i, List<ColonyStat> stats)
		{
			if (statAsset == null)
				return;
			VisualElement statGroup = ElementAt(i).ElementAt(1);
			statGroup.AddToClassList("stat-group");

			foreach (ColonyStat stat in stats)
			{
				VisualElement statElem = statAsset.CloneTree();
				statElem.AddToClassList("stat");
				statElem = statElem.ElementAt(0);
				((Label)statElem.ElementAt(0)).text = stat.GetText(false);
				((Label)statElem.ElementAt(2)).text = stat.name;
				((Label)statElem.ElementAt(2)).
					RegisterCallback<MouseEnterEvent>((eve) => ShowMenu(eve, stat));
                ((Label)statElem.ElementAt(2)).
                    RegisterCallback<MouseLeaveEvent>(HideMenu);

                statElem = statElem.ElementAt(1);

				statGroup.Add(statElem.parent.parent);
			}
		}

		void RefreshStat(ColonyStat stat, VisualElement group, bool canAffordNew)
		{
			VisualElement el;
			for (int stateLevel = ColonyStat.MAX_STAT_LEVEL; stateLevel > 0; stateLevel--)
			{
				el = group.ElementAt(stateLevel-1);
				if (!el.ClassListContains("locked") && !el.ClassListContains("completed"))
				{
					el.UnregisterCallback<MouseEnterEvent, ColonyStat>(ShowMenu); 
					el.UnregisterCallback<MouseLeaveEvent>(HideMenu);
					if (el.ClassListContains("available"))
						el.UnregisterCallback<MouseDownEvent, ColonyStat>(UpgradeStat);
				}

				el.ClearClassList();
				el.AddToClassList("state-button");

				if (stateLevel > stat.MaxState)
				{
					el.AddToClassList("locked");
					continue;
				}
				else if (stateLevel <= stat.CurrentState)
				{
					el.AddToClassList("completed");
					continue;
				}
				else if (canAffordNew && stateLevel == stat.CurrentState + 1)
				{
					el.AddToClassList("available");
					el.RegisterCallback<MouseDownEvent, ColonyStat>(UpgradeStat, stat);
				}

				el.RegisterCallback<MouseEnterEvent, ColonyStat>(ShowMenu, stat);
				el.RegisterCallback<MouseLeaveEvent>(HideMenu);
			}
			((Label)group.parent.ElementAt(0)).text = stat.GetText(false);
		}

		void ShowMenu(MouseEnterEvent eve, ColonyStat stat) =>
			ToolkitUtils.localMenu.Open(stat, (VisualElement)eve.target);
		void HideMenu(MouseLeaveEvent _) =>
			ToolkitUtils.localMenu.Close();
		void UpgradeStat(MouseDownEvent _, ColonyStat stat)
		{
			HideMenu(null);
			stat.Upgrade();
			RefreshStates();
		}

		public string Open()
		{
			style.display = DisplayStyle.Flex;
			RefreshStates();

			return UIRefs.trading.colonyLocation.name;
		}

		void RefreshStates()
		{
			ColonyLocation location = UIRefs.trading.colonyLocation;

			VisualElement statGroup = ElementAt(0).ElementAt(1);

			int i = 0;
            location.stats.ForEach(q => RefreshStat(q, statGroup.ElementAt(i++).ElementAt(0).ElementAt(1), q.CanAfford()));

			statGroup = ElementAt(1).ElementAt(1);
			i = 0;
            location.production.ForEach(q => RefreshStat(q, statGroup.ElementAt(i++).ElementAt(0).ElementAt(1), q.CanAfford()));
        }

		public void Hide()
		{
			style.display = DisplayStyle.None;
		}
	}
}
