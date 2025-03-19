using UnityEngine;
using UnityEngine.UI;

public class UIbuttons : MonoBehaviour
{
    public void ToggleAction(int i)
    {
        GridTiles gridTiles = SceneRefs.gridTiles;
        if(gridTiles.activeControl == (ControlMode)i)
        {
            gridTiles.activeObject = null;
            gridTiles.ChangeSelMode(ControlMode.nothing);
        }
        else
            gridTiles.ChangeSelMode((ControlMode)i);
    }

}
