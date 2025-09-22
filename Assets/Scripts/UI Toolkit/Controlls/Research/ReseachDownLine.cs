using UnityEngine;
using UnityEngine.UIElements;

namespace ResearchUI
{
    [UxmlElement]
    public partial class ResearchDownLine : ResearchLine
    {
        public ResearchHorizontalLine horizontalLine;
        public ResearchDownLine() { }

        public ResearchDownLine(Rect rect, ResearchHorizontalLine _horizontalLine) : base(rect)
        {
            AddToClassList("line");
            horizontalLine = _horizontalLine;
        }
        public override void Fill()
        {
            RemoveFromClassList("line");
            AddToClassList(FILLED_LINE_CLASS);
            if (horizontalLine != null)
                horizontalLine.Fill();
        }
    }
}
