

using System;

public enum StorageRequestType
{
    Take,
    Store,
    Action,
}
[Serializable]
public class StorageRequest
{
    public Resource resource;
    public Human carrier;
    public StorageRequestType mod;

    public StorageRequest()
    {
    }

    public StorageRequest(Resource resource, Human carrier, StorageRequestType mod)
    {
        this.resource = resource;
        this.carrier = carrier;
        this.mod = mod;
    }
    public StorageRequest(StorageRequestSave save)
    {
        resource = new(save.request);
        mod = save.mod;
    }

    public void SetRequestToAction(JobState newState)
    {
        resource.Clear();
        mod = StorageRequestType.Action;
        carrier.SetJob(newState);
    }
}