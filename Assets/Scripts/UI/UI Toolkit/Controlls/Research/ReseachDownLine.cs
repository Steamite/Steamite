using UnityEngine;
using UnityEngine.UIElements;

namespace Research
{
    [UxmlElement]
    public partial class ResearchDownLine : ResearchLine
    {
        public ResearchLine horizontalLine;
        public ResearchDownLine() { }

        public ResearchDownLine(Rect rect, ResearchLine _horizontalLine)
        {
            AddToClassList("line");
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
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
