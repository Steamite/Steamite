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
    [CreateProperty] ModifiableResource ResourceCost { get; set; }

    /// <summary>Production cycle yeild.</summary>
    [CreateProperty] ModifiableResource ResourceYield { get; set; }
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
        CapacityResource resource = new(-1);
        // TODO: IMPROVE
        MyRes.MoveRes(
            resource,
            human.Inventory,
            InputResource.requests[index],
            transferPerTick);
        MyRes.UpdateResource(resource, false);
        InputResource.Manage(resource, true);
        // TODO END
        ((ClickableObject)this).UIUpdate(nameof(InputResource));
        if (InputResource.Diff(ResourceCost).Sum() == 0)
        {
            SceneRefs.jobQueue.CancelJob(JobState.Supply, (ClickableObject)this);
            ProdStates.supplied = true;
            RefreshStatus();
        }
        if (InputResource.requests[index].Sum() == 0)
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
                CurrentTime += ProdSpeed * progress;
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
        while (CurrentTime >= ProdTime &&
            (LocalResource.capacity.currentValue == -1 || LocalResource.Sum() < LocalResource.capacity.currentValue))
        {
            CurrentTime -= ProdTime;
            LocalResource.Manage(ResourceYield, true);
            MyRes.UpdateResource(ResourceYield, true);
            ProdStates.supplied = InputResource.Diff(ResourceCost).Sum() == 0;
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
            InputResource.Manage(ResourceCost, false);
            ((ClickableObject)this).UIUpdate("InputResource");
            //MyRes.UpdateResource(ProductionCost, -1);
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

    new public void RefreshStatus()
    {
        Building building = this as Building;

        if (building.constructed)
        {
            building.transform.GetChild(0).GetChild(0).gameObject.SetActive(Stoped);
            building.transform.GetChild(0).GetChild(1).gameObject.SetActive(!ProdStates.supplied);
            building.transform.GetChild(0).GetChild(2).gameObject.SetActive(!ProdStates.space);
        }
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

    void Init(bool constructed)
    {
        if (constructed)
        {
            RequestRestock();
            RequestPickup();
        }
    }
    #endregion
}
