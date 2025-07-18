using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Serves as a store house for the colony.<br/>
/// Each storage should have all resource types in the <see cref="StorageObject.localRes"/>
/// </summary>
public interface IStorage
{
    /// <summary>List containing which resources can be stored.</summary>
    public List<bool> CanStore { get; set; }
    public StorageResource LocalResources { get; }

    /// <summary>
    /// Assigns all resource types and setups base storage resources.
    /// </summary>
    /// <param name="jQ">Reference to register this object to global storage list.</param>
    public void SetupStorage(int ammount = 0)
    {
        int i = 1;
        LocalResources.types = new();
        LocalResources.ammounts = new();
        CanStore = new();
        foreach (var item in Enum.GetNames(typeof(ResourceType)).Skip(1))
        {
            LocalResources.types.Add((ResourceType)i);
            LocalResources.ammounts.Add(ammount);
            CanStore.Add(true);
            i++;
        }
    }

    public void FinishStorageConstruction()
    {
        SetupStorage();
        SceneRefs.JobQueue.storages.Add(this);
    }

    /// <summary>
    /// Removes the resurce from existence.
    /// </summary>
    /// <param name="type">Type of resource.</param>
    /// <param name="ammountToDestroy">Ammount of resources.</param>
    public void DestroyResource(ResourceType type, int ammountToDestroy)
    {
        LocalResources[type] -= ammountToDestroy;
        ((Building)this).UIUpdate(nameof(Building.LocalRes));
    }
}
