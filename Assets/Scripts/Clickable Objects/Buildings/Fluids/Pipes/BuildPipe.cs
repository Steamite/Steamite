using UnityEngine;
using UnityEngine.EventSystems;

public class BuildPipe : Pipe
{
    public IFluidWork connectedBuilding;
    //bool pipesSaved = false;
    public void FillData(Pipe p, IFluidWork _connectedBuild)
    {
        network = p.network;
        id = p.id;
        connectedBuilding = _connectedBuild;
    }
    public override void OnPointerEnter(PointerEventData eventData) { }
    public override void OnPointerExit(PointerEventData eventData) { }
    public override void OnPointerDown(PointerEventData eventData)
    {
        ((Building)connectedBuilding).OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData) { }
    public override void OrderDeconstruct()
    {
        ((Building)connectedBuilding).OrderDeconstruct();
    }
    public override void FinishBuild()
    {
        base.FinishBuild();
        if (network.buildings.IndexOf(connectedBuilding) == -1)
        {
            network.buildings.Add(connectedBuilding);
        }
    }
    public override void PlacePipe()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        base.PlacePipe();
    }
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        return null;
    }

    
}
