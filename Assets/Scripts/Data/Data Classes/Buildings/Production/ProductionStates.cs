using System;

[Serializable]
public class ProductionStates
{
    public bool supplied = false;
    public bool space = true;
    public bool running = false;
    public ProductionStates(bool _supplied, bool _space, bool _running)
    {
        this.supplied = _supplied;
        this.space = _space;
        this.running = _running;
    }
    public ProductionStates()
    {

    }
}