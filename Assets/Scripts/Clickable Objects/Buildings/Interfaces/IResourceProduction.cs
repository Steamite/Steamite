using System.Linq;
using Unity.Properties;
using UnityEngine;

public interface IResourceProduction : IProduction
{
    #region Properties
    /// <summary>Need Production states to work.</summary>
    ProductionStates ProdStates { get; }

    /// <summary>Refence to the <see cref="StorageObject.LocalRes"/>.</summary>
    StorageResource LocalResource { get; }

    /// <summary>The Storage for paying the production cost.</summary>
    [CreateProperty] StorageResource InputResource { get; set; }

    /// <summary>Cost of one production cycle.</summary>
    Resource ProductionCost { get; }

    /// <summary>Production cycle yeild.</summary>
    Resource ProductionYield { get; }
    #endregion

    #region Storing
    /// <summary>
    /// Handles storing to <see cref="InputResource"/>. Call if storing, and constructed.
    /// </summary>
    /// <param name="human">Human that's doing the storing.</param>
    /// <param name="transferPerTick">Max transfer ammount.</param>
    void StoreProdResources(Human human, int transferPerTick)
    {
        int index = InputResource.carriers.IndexOf(human);
        if (index == -1)
            Debug.Log("");
        MyRes.MoveRes(InputResource.stored, human.Inventory, InputResource.requests[index], transferPerTick);
        ((ClickableObject)this).UIUpdate(nameof(InputResource));
        if (MyRes.DiffRes(ProductionCost, InputResource.stored).ammount.Sum() == 0)
        {
            human.transform.parent.parent.GetComponent<JobQueue>().CancelJob(JobState.Supply, (ClickableObject)this);
            ProdStates.supplied = true;
            RefreshStatus();
        }
        if (InputResource.requests[index].ammount.Sum() == 0)
        {
            InputResource.RemoveRequest(human);

            human.destination = null;
            human.SetJob(JobState.Free);
            return;
        }
    }
    #endregion

    #region Production
    void IProduction.ProgressProduction(float progress)
    {
        if (!Stoped)
        {
            if (ProdStates.running || ManageInputRes())
            {
                CurrentTime += Modifier * progress;
                ((Building)this).UIUpdate(nameof(CurrentTime));
                if (CurrentTime >= ProdTime && ProdStates.space)
                {
                    Product();
                    AfterProduction();

                    ((Building)this).UIUpdate(nameof(Building.LocalRes));
                    RefreshStatus();
                }
            }
        }
    }

    void IProduction.Product()
    {
        while (CurrentTime >= ProdTime && (LocalResource.stored.capacity == -1 || LocalResource.stored.ammount.Sum() < LocalResource.stored.capacity))
        {
            CurrentTime -= ProdTime;
            MyRes.ManageRes(LocalResource.stored, ProductionYield, 1);
            MyRes.UpdateResource(ProductionYield, 1);
            ProdStates.supplied = MyRes.DiffRes(ProductionCost, InputResource.stored).ammount.Sum() == 0;
            if (!ManageInputRes())
                return;
        }
    }

    /// <summary>
    /// Triggered before starting each production cycle. <br/>
    /// Consumes production cost or requests new resources.
    /// </summary>
    /// <returns>Returns production is running.</returns>
    public bool ManageInputRes()
    {
        if (!ProdStates.supplied)
        {
            ProdStates.running = false;
            RequestRestock();
            return false;
        }
        else
        {
            MyRes.ManageRes(InputResource.stored, ProductionCost, -1);
            ((ClickableObject)this).UIUpdate("InputResource");
            MyRes.UpdateResource(ProductionCost, -1);
            ProdStates.running = true;
            return true;
        }
    }

    /// <summary>Place for custom implementations on child classes.</summary>
    void AfterProduction()
    {
        RequestRestock();
        RequestPickup();
    }

    /// <summary>Toggles state indicators.</summary>
    public void RefreshStatus()
    {
        Building building = this as Building;

        if (building.constructed)
        {
            building.transform.GetChild(0).GetChild(0).gameObject.SetActive(Stoped);
            building.transform.GetChild(0).GetChild(1).gameObject.SetActive(!ProdStates.supplied);
            building.transform.GetChild(0).GetChild(2).gameObject.SetActive(!ProdStates.space);
        }
    }

    /// <summary>
    /// Manual production toggle.
    /// </summary>
    /// <returns>New toggle state.</returns>
    public bool StopProduction()
    {
        Stoped = !Stoped;
        RefreshStatus();
        return Stoped;
    }
    #endregion

    #region Logistics
    /// <summary>Adds a <see cref="JobState.Supply"/> job order.</summary>
    void RequestRestock()
    {
        if (InputResource.carriers.Count == 0 && !SceneRefs.jobQueue.supplyNeeded.Contains(this))
            SceneRefs.jobQueue.AddJob(JobState.Supply, (ClickableObject)this);
    }

    /// <summary>Adds a <see cref="JobState.Pickup"/> job order.</summary>
    void RequestPickup()
    {
        if (LocalResource.carriers.Count == 0 && !SceneRefs.jobQueue.pickupNeeded.Contains((StorageObject)this))
            SceneRefs.jobQueue.AddJob(JobState.Pickup, (ClickableObject)this);
    }
    #endregion
}
