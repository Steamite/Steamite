using System;

[Serializable]
public class ProductionStates
{
    public bool requestedSupply = false;
    public bool supplied = false;
    public bool needsResources = true;

    public bool requestedPickup = false;
    public bool space = true;

    public bool running = false;

    public ProductionStates()
    {
    }

    public ProductionStates(bool requestedSupply, bool supplied, bool needsResources, bool requestedPickup, bool space, bool running)
    {
        this.requestedSupply = requestedSupply;
        this.supplied = supplied;
        this.needsResources = needsResources;
        this.requestedPickup = requestedPickup;
        this.space = space;
        this.running = running;
    }
}