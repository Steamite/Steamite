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
    public void SetupStorage()
    {
        int i = 1;
        LocalResources.types = new();
        LocalResources.ammounts = new();
        CanStore = new();
        foreach (var item in Enum.GetNames(typeof(ResourceType)).Skip(1))
        {
            LocalResources.types.Add((ResourceType)i);
            LocalResources.ammounts.Add(100);
            CanStore.Add(true);
            i++;
        }
    }

    public void FinishStorageConstruction()
    {
        LocalResources.types = MyRes.resourceTemplate.types;
        while (LocalResources.ammounts.Count < MyRes.resourceTemplate.types.Count)
            LocalResources.ammounts.Add(0);
        for (int i = 0; i < LocalResources.ammounts.Count; i++)
            CanStore.Add(true);
        SceneRefs.jobQueue.storages.Add(this);
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
