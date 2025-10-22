using System.Collections.Generic;
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
    [CreateProperty] ModifiableResource ResourceCost { get; set; }

    /// <summary>Production cycle yeild.</summary>
    [CreateProperty] ModifiableResource ResourceYield { get; set; }
    List<ProductionRecipe> Recipes { get; set; }
    List<DataAssign> RecipeAsssigment { get; set; }
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
            SceneRefs.JobQueue.CancelJob(JobState.Supply, (ClickableObject)this);
            ProdStates.supplied = true;
            ProdStates.requestedSupply = false;
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
                if (CurrentTime >= ProdTime)
                {
                    Product();
                }
            }
        }
    }

    void IProduction.Product()
    {
        CurrentTime -= ProdTime;
        LocalResource.Manage(ResourceYield, true);
        ((IUpdatable)this).UIUpdate(nameof(Building.LocalRes));
        MyRes.UpdateResource(ResourceYield, true);

        if(ProdStates.needsResources)
            ProdStates.supplied = InputResource.Diff(ResourceCost).Sum() == 0;
        ProdStates.space = ResourceYield.Sum() <= LocalResource.FreeSpace;
        ManageInputRes();
    }

    /// <summary>
    /// Triggered before starting each production cycle. <br/>
    /// Consumes production cost or requests new resources.
    /// </summary>
    /// <returns>Returns production is running.</returns>
    public bool ManageInputRes()
    {
        if (ProdStates.needsResources && !ProdStates.supplied)
        {
            ProdStates.running = false;
            RequestRestock();
            return false;
        }
        else if (!ProdStates.space)
        {
            ProdStates.running = false;
            RequestPickup();
            return false;
        }
        else
        {
            InputResource.Manage(ResourceCost, false);
            ((ClickableObject)this).UIUpdate("InputResource");
            ProdStates.running = true;
            return true;
        }
    }

    new public void RefreshStatus()
    {
        Building building = this as Building;

        if (building.constructed)
        {
            /*building.transform.GetChild(0).GetChild(0).gameObject.SetActive(Stoped);
            building.transform.GetChild(0).GetChild(1).gameObject.SetActive(!ProdStates.supplied);
            building.transform.GetChild(0).GetChild(2).gameObject.SetActive(!ProdStates.space);*/
        }
    }
    #endregion

    #region Logistics
    /// <summary>Adds a <see cref="JobState.Supply"/> job order.</summary>
    void RequestRestock()
    {
        if(ProdStates.needsResources && ProdStates.requestedSupply == false)
        {
            ProdStates.requestedSupply = true;
            SceneRefs.JobQueue.AddJob(JobState.Supply, (ClickableObject)this);
        }
    }

    /// <summary>Adds a <see cref="JobState.Pickup"/> job order.</summary>
    void RequestPickup()
    {
        if (ProdStates.requestedPickup == false && LocalResource.Sum() > 0)
        {
            ProdStates.requestedPickup = true;
            SceneRefs.JobQueue.AddJob(JobState.Pickup, (ClickableObject)this);
        }
    }

    void Init(bool constructed, ProductionRecipeHolder holder)
    {
        ProdStates.needsResources = ResourceCost.Sum() > 0;
        Recipes = new();
        foreach(var recip in RecipeAsssigment)
        {
            Recipes.Add(holder.GetObjectBySaveIndex(recip));//.Categories[recip.categoryIndex].Objects.FirstOrDefault(q => q.id == recip.objectId));
        }
        if (constructed)
        {
            RequestRestock();
            RequestPickup();
        }
    }
    #endregion

    void SetRecipe(ProductionRecipe recipe)
    {
        ResourceCost = recipe.resourceCost;
        ResourceYield = recipe.resourceYield;
        ProdTime = recipe.timeInTicks;
        CurrentTime = 0;
        if (((Building)this).selected)
        {
            ((Building)this).OpenWindow();
        }
    }
}
