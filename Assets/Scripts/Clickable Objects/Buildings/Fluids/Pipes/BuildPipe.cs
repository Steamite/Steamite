using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildPipe : Pipe
{
    public Building connectedBuilding;
    bool pipesSaved = false;
    public void FillData(Pipe p, Building _connectedBuild)
    {
        network = p.network;
        id = p.id;
        build = p.build;
        connectedBuilding = _connectedBuild;
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        connectedBuilding.OnPointerEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        connectedBuilding.OnPointerExit(eventData);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        connectedBuilding.OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        connectedBuilding.OnPointerUp(eventData);
    }
    public override void OrderDeconstruct()
    {
        Debug.LogError("Can't deconstructed!");
       // connectedBuilding.OrderDeconstruct();
    }
    public override void FinishBuild()
    {
        base.FinishBuild();
        if (network.buildings.IndexOf(connectedBuilding) == -1)
        {
            network.buildings.Add(transform.parent.parent.GetComponent<Building>());
        }
    }
    public override void PlacePipe()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        base.PlacePipe();
        connectedBuilding = transform.parent.parent.GetComponent<Building>();
    }
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
    }
}
