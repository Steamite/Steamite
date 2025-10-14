using UnityEngine;

public class UIbuttons : MonoBehaviour
{
    public void ToggleAction(int i)
    {
        GridTiles gridTiles = SceneRefs.GridTiles;
        if (gridTiles.activeControl == (ControlMode)i)
        {
            gridTiles.activeObject = null;
            gridTiles.ChangeSelMode(ControlMode.Nothing);
        }
        else
            gridTiles.ChangeSelMode((ControlMode)i);
    }

}
