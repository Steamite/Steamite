using System.Collections.Generic;
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
    /// <param name="templateRes">Clones resource types.</param>
    /// <param name="jQ">Reference to register this object to global storage list.</param>
    public void SetupStorage(JobQueue jQ, bool fill = true)
    {
        LocalResources.stored.type = MyRes.resourceTemplate.type;
        while (LocalResources.stored.ammount.Count < MyRes.resourceTemplate.type.Count)
        {
            if (fill)
                LocalResources.stored.ammount.Add(100);
            else
                LocalResources.stored.ammount.Add(0);
        }
        CanStore = new();
        for (int i = 0; i < LocalResources.stored.ammount.Count; i++)
        {
            CanStore.Add(true);
        }
        jQ.storages.Add(this);
        LocalResources.stored.capacity = 1000;
    }

    /// <summary>
    /// Removes the resurce from existence.
    /// </summary>
    /// <param name="type">Type of resource.</param>
    /// <param name="ammountToDestroy">Ammount of resources.</param>
    public void DestroyResource(ResourceType type, int ammountToDestroy)
    {
        LocalResources.stored[type] -= ammountToDestroy;
        ((Building)this).UIUpdate(nameof(Building.LocalRes));
    }
}
