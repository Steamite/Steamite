using System;

[Serializable]
public class ProductionStates
{
    public bool supplied = false;
    public bool supply = true; //
    public bool space = true;
    public bool stoped = false;
    public bool running = false;
    public ProductionStates(bool _supplied, bool _supply, bool _space, bool _stoped, bool _running, bool _isResearch)
    {
        this.supplied = _supplied;
        this.supply = _supply;
        this.space = _space;
        this.stoped = _stoped;
        this.running = _running;
    }
    public ProductionStates()
    {

    }
}