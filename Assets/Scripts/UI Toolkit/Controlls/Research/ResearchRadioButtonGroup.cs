using AbstractControls;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ResearchUI
{
    class ResearchRadioButtonGroup : CustomRadioButtonGroup
    {
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
                l.name = j.ToString();
                l.AddToClassList("background-line");
                l.style.top = j * 194;
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
                ResearchRadioButton researchUIButton = new(category, i, this);
                ResearchNode node = category.Objects[i];
                if (lastLevel != node.level)
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
                        researchUIButton.AfterLines(category);
                    });
            }
        }
        public void SetSelection(int value)
            => SelectedChoice = value;
        public override bool Select(int value)
        {
            if (SelectedChoice > -1)
                GetButtonByIndex(SelectedChoice).Deselect();
            if (SelectedChoice == value)
            {
                SelectedChoice = -1;
                changeEvent?.Invoke(-1);
                return false;
            }
            else
            {
                SelectedChoice = value;
                changeEvent?.Invoke(value);
                return true;
            }
        }

        public ResearchRadioButton GetButtonByIndex(int i)
            => GetRow(positions[SelectedChoice].level)[positions[SelectedChoice].position] as ResearchRadioButton;

        public VisualElement GetRow(int level) => this[level + 1][1];

        public void DrawLines(int sLevel, int sIndexInLevel, int eLevel, int eIndexInLevel)
        {
            ResearchRadioButton topButton = (ResearchRadioButton)GetRow(sLevel)[sIndexInLevel];
            ResearchRadioButton botButton = (ResearchRadioButton)GetRow(eLevel)[eIndexInLevel];

            float height = GetRow(0).resolvedStyle.height / 2;
            float topPos = topButton.localBound.x + topButton.localBound.width / 2;
            float botPos = botButton.localBound.x + botButton.localBound.width / 2;
            float horPos = topButton.worldBound.y + topButton.worldBound.height / 2 - this[0].worldBound.y + height;

            #region Down && horizonatal lines
            if (topButton.lineDown == null)
            {
                #region Horizontal Line
                ResearchHorizontalLine horizontalLine = new(
                    new(
                        Mathf.Min(topPos, botPos),
                        horPos,
                        Mathf.Abs(topPos - botPos + ResearchLine.WIDTH),
                        ResearchLine.WIDTH
                    ));

                this[0][1].Add(horizontalLine);
                #endregion Horizontal Line

                #region LineDown
                ResearchDownLine lineDown = new(
                    new(
                        topPos,
                        topButton.worldBound.y + topButton.worldBound.height / 2 - this[0].worldBound.y + ResearchLine.BORDER,
                        ResearchLine.WIDTH,
                        height),
                    horizontalLine);

                this[0][1].Add(lineDown);
                topButton.lineDown = lineDown;
                if (topButton.node.researched)
                    lineDown.Fill();
                #endregion LineDown
            }
            else
            {
                topButton.lineDown.horizontalLine.Resize(botPos);
            }
            #endregion

            #region Up line
            if (botButton.lineUp == null)
            {
                ResearchLine line = new(
                    new(
                        botPos,
                        horPos + ResearchLine.WIDTH-ResearchLine.BORDER,
                        ResearchLine.WIDTH,
                        (sLevel - eLevel - 1) * 200 + height));
                this[0][1].Add(line);
                if (botButton.node.researched)
                    line.Fill();
                botButton.lineUp = line;
            }
            #endregion
        }
    }
}
