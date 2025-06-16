using UnityEngine;
using UnityEngine.UIElements;

namespace ResearchUI
{
    [UxmlElement]
    public partial class ResearchLine : VisualElement
    {
        public const string FILLED_LINE_CLASS = "line-filled";
        public ResearchLine() { }
        public ResearchLine(Rect rect)
        {
            AddToClassList("line");
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
        }


        public virtual void Fill()
        {
            RemoveFromClassList("line");
            AddToClassList(FILLED_LINE_CLASS);
        }
    }
}
