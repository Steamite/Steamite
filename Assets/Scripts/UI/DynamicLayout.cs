using UnityEngine;
using UnityEngine.UI;

// Author: Matty Alan 2016
public class DynamicGridLayoutGroup : GridLayoutGroup
{
    // Recalculates The Cell Size For The Grid Layout
    // Currently; This forces the Grid Items To Be Square

    new void OnRectTransformDimensionsChange()
    {
        // Get The Total Panel Size.
        float size = 0;
        if (constraint == Constraint.FixedColumnCount)
            size = rectTransform.rect.width;
        else if (constraint == Constraint.FixedRowCount)
            size = rectTransform.rect.height;

        // Divide That Size By The Item Count
        size = size / (float)constraintCount;
        cellSize = new Vector2(size, size);

        base.OnRectTransformDimensionsChange();
    }

    public void ResizePanel(int itemCount)
    {
        var sizeDelta = rectTransform.sizeDelta;

        if (constraint == Constraint.FixedColumnCount)
            sizeDelta.y = (cellSize.y * itemCount) / (float)constraintCount;
        else if (constraint == Constraint.FixedRowCount)
            sizeDelta.x = (cellSize.x * itemCount) / (float)constraintCount;

        rectTransform.sizeDelta = sizeDelta;
    }
}