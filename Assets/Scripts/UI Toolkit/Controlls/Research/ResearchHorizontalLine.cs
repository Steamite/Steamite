using UnityEngine;

namespace ResearchUI
{
    public class ResearchHorizontalLine : ResearchLine
    {
        float minX, maxX;

        public ResearchHorizontalLine(Rect rect) : base(rect)
        {
            minX = rect.xMin;
            maxX = rect.xMax;

            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
        }

        public void Resize(float newPos)
        {
            if (newPos < minX)
            {
                minX = newPos;
                style.left = minX;
            }
            else if (newPos > maxX)
            {
                maxX = newPos;
            }
            else
                return;
            style.width = maxX - minX + WIDTH;
        }

    }
}

