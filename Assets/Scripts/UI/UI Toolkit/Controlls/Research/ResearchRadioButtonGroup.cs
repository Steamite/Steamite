using AbstractControls;
using InfoWindowElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Research
{
    class ResearchRadioButtonGroup : CustomRadioButtonGroup
    {
		List<VisualElement> horizonatalLines = new();
		/// <summary>Tuple<level, position in the level></summary>
		List<(int level, int position)> positions = new();
		public ResearchRadioButtonGroup() : base() { }
        public ResearchRadioButtonGroup(ResearchCategory category) : base()
        {
			VisualElement lines = new();
			VisualElement rowLines = new();
			for (int j = 1; j < 5; j++)
			{
				VisualElement l = new();
				l.style.top = j * 196;
				l.style.backgroundColor = Color.black;
				l.style.width = new Length(100, LengthUnit.Percent);
				l.style.height = 3;
				rowLines.Add(l);
			}
			lines.Add(rowLines);
			lines.Add(new());

			lines.name = "Lines";
			Add(lines);
			for (int j = 0; j < 5; j++)
			{
				VisualElement row = new();
				row.name = "Row";
				row.Add(new Label(j.ToString()));
				if (j == 0)
					row[0].style.borderBottomWidth = 0;
				row.Add(new());
				Add(row);
			}
			CreateNodes(category);
		}

		void CreateNodes(ResearchCategory category)
		{
			positions = new();
			int inLevel = 0;
			int lastLevel = -1;
			for (int i = 0; i < category.Objects.Count; i++)
			{
				ResearchRadioButton researchUIButton = new(category.Objects, i);
				ResearchNode node = category.Objects[i];
				if(lastLevel != node.level)
				{
					inLevel = 0;
					lastLevel = node.level;
				}
				else
					inLevel++;

				positions.Add(new(node.level, inLevel));
				GetRow(node.level).Add(researchUIButton);

				var index = i;
				researchUIButton.RegisterCallbackOnce<GeometryChangedEvent>(
					(_) =>
					{
						for (int k = 0; k < node.unlockedBy.Count; k++)
						{
							int eIndex = category.Objects.FindIndex(q => q.id == node.unlockedBy[k]);
							DrawLines(
								node.level,
								index - category.Objects.FindIndex(q => q.level == node.level),
								category.Objects[eIndex].level,
								eIndex - category.Objects.FindIndex(q => q.level == category.Objects[eIndex].level));
						}
					});
			}
		}

		public override void Select(int value)
		{
			if (SelectedChoice > -1 && SelectedChoice != value)
			{
				GetButtonByIndex(SelectedChoice).Deselect();
			}
			SelectedChoice = value;
			if (value > -1)
				changeEvent?.Invoke(value);
		}

		public ResearchRadioButton GetButtonByIndex(int i)
			=> GetRow(positions[SelectedChoice].level)[positions[SelectedChoice].position] as ResearchRadioButton;

		public VisualElement GetRow(int level) => this[level+1][1];

		public void DrawLines(int sLevel, int sIndexInLevel, int eLevel, int eIndexInLevel)
		{
			ResearchRadioButton topButton = (ResearchRadioButton)GetRow(sLevel)[sIndexInLevel];
			ResearchRadioButton botButton = (ResearchRadioButton)GetRow(eLevel)[eIndexInLevel];

			float height = GetRow(0).resolvedStyle.height / 2;
			float topPos = topButton.localBound.x + topButton.localBound.width / 2;
			float botPos = botButton.localBound.x + botButton.localBound.width / 2;
			float horPos = topButton.worldBound.y + topButton.worldBound.height / 2 - this[0].worldBound.y + height;

			ResearchLine line = new(
				new(
					Mathf.Min(topPos, botPos),
					horPos,
					Mathf.Abs(topPos - botPos),
					3
				));
			this[0][1].Add(line);

			#region Vertical buttons
			if (topButton.lineDown == null)
			{
				line = new(
					new(
						topPos,
						topButton.worldBound.y + topButton.worldBound.height / 2 - this[0].worldBound.y,
						3,
						height
					));
				this[0][1].Add(line);
				topButton.lineDown = line;
			}

			if (botButton.lineUp == null)
			{
				line = new(
					new(
						botPos,
						horPos,
						3,
						(sLevel - eLevel - 1) * 200 + height
					));
				this[0][1].Add(line);
				botButton.lineUp = line;
			}
			#endregion
		}
	}
}
