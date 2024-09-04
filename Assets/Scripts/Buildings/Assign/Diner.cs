using System.Linq;

public class Diner : ProductionBuilding
{
    protected override void AfterProduction()
    {
        if(localRes.stored.ammount.Sum() >= localRes.stored.capacity)
        {
            pStates.space = false;
            PauseProduction();
        }
    }
}
