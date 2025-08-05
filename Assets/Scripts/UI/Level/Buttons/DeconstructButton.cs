using UnityEngine;

public class DeconstructButton : MonoBehaviour
{
    public void Decontruct()
    {
        Building building = GameObject.Find("Grid").GetComponent<GridTiles>().clickedObject.GetComponent<Building>();
        building.OrderDeconstruct();
        // UPDATE
    }
}
