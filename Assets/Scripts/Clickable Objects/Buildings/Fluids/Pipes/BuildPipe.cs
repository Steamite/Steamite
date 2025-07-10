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

            if (connectedBuilding is WaterPump 
                || connectedBuilding is FluidTank 
                || connectedBuilding is FluidResProductionBuilding)
                network.storageBuildings.Add(connectedBuilding);

            if(connectedBuilding is FluidResProductionBuilding fluidResProduction)
                network.consumptionBuildings.Add(fluidResProduction);
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

    public override void ChangeRenderMode(bool transparent){}
    public override void GetRenderComponents() {}

    protected override void AddRenderer(Renderer _renderer)
    {
        Building connection = connectedBuilding as Building;
        connection.meshRenderers.Add(_renderer);/*
        _renderer.material = connection.meshRenderers[0].material;
        _renderer.material.color = connection.meshRenderers[0].material.color;*/
    }
    protected override void RemoveRenderer(Renderer _renderer)
    {
        ((Building)connectedBuilding).meshRenderers.Remove(_renderer);
    }


    public void RecalculatePipeTransform()
    {
        transform.rotation = Quaternion.identity;
        float f = Mathf.Abs(transform.parent.rotation.eulerAngles.y % 180);
        if (f == 0)
        {
            transform.localScale = new(
                0.5f / transform.parent.lossyScale.x,
                0.1f / transform.parent.lossyScale.y,
                0.5f / transform.parent.lossyScale.z);
        }
        else
        {
            transform.localScale = new(
                0.5f / transform.parent.lossyScale.z,
                0.1f / transform.parent.lossyScale.y,
                0.5f / transform.parent.lossyScale.x);
        }
    }
}
