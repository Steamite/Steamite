using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Serves as a store house for the colony.<br/>
/// Each storage should have all resource types in the <see cref="StorageObject.localRes"/>
/// </summary>
public interface IStorage
{
    /// <summary>List containing which resources can be stored.</summary>
    public List<bool> CanStore { get; set; }
    public StorageResource LocalResources { get; }
    public ulong CanStoreMask { get; set; }

    /// <summary>
    /// Assigns all resource types and setups base storage resources.
    /// </summary>
    public void SetupStorage(int ammount = 0)
    {
        LocalResources.types = new();
        LocalResources.ammounts = new();
        CanStore = new();

        int i = 0;
        ulong mask = CanStoreMask;
        try
        {
            while (mask != 0 && i < ResFluidTypes.GetResMax())
            {
                if ((mask & 1) == 1)
                {
                    LocalResources.types.Add(ResFluidTypes.GetResByIndex(i));
                    LocalResources.ammounts.Add(ammount);
                    CanStore.Add(true);
                }
                mask = mask >> 1;
                i++;
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"mask out of range({exception})");
        }
        
    }

    public void FinishStorageConstruction()
    {
        SetupStorage();
        SceneRefs.JobQueue.AddStorage(this);
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
