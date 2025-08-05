using UnityEngine;
using UnityEngine.EventSystems;

public class BuildPipe : Pipe
{
    public const string BUILD_PIPE_PREF_NAME = "Build pipe";
    public IFluidWork connectedBuilding;
    //bool pipesSaved = false;

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
    }
    public override void ConnectToNetwork(FluidNetwork network)
    {
        base.ConnectToNetwork(network);
        if (network.buildings.IndexOf(connectedBuilding) == -1)
        {
            network.buildings.Add(connectedBuilding);

            if (connectedBuilding is NeedSourceProduction
                || connectedBuilding is FluidTank
                || connectedBuilding is FluidResProductionBuilding)
                network.storageBuildings.Add(connectedBuilding);

            if (connectedBuilding is FluidResProductionBuilding fluidResProduction)
                network.consumptionBuildings.Add(fluidResProduction);
        }
    }

    public override void DestoyBuilding()
    {
        network.buildings.Remove(connectedBuilding);

        if (connectedBuilding is NeedSourceProduction
            || connectedBuilding is FluidTank
            || connectedBuilding is FluidResProductionBuilding)
            network.storageBuildings.Remove(connectedBuilding);

        if (connectedBuilding is FluidResProductionBuilding fluidResProduction)
            network.consumptionBuildings.Remove(fluidResProduction);
        base.DestoyBuilding();
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
