using UnityEngine;
using UnityEngine.UI;

public class UIbuttons : MonoBehaviour
{
    public void ToggleAction(int i)
    {
        GridTiles gridTiles = MyGrid.gridTiles;
        if(gridTiles.selMode == (SelectionMode)i)
        {
            gridTiles.activeObject = null;
            gridTiles.ChangeSelMode(SelectionMode.nothing);
        }
        else
            gridTiles.ChangeSelMode((SelectionMode)i);
    }
}
