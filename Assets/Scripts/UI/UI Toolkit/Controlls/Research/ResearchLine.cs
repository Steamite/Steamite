using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Research
{
    [UxmlElement]
    partial class ResearchLine : VisualElement
    {
        public ResearchLine() { }
        public ResearchLine(Rect rect)
		{
			AddToClassList("line");
            style.left = rect.x;
            style.top = rect.y;
			style.width = rect.width;
            style.height = rect.height;
        }
    }
}
