using System.Collections.Generic;
using System.Linq;
using System;

[Serializable]
public struct JobData
{
    public JobState job;
    public List<GridPos> path;
    public ClickableObject interest;
    public JobData(JobSave jobSave, Human human)
    {
        job = jobSave.job;
        path = jobSave.path != null ? jobSave.path : new();
        interest = null;
        if (jobSave.destinationID > -1)
        {
            human.destination = MyGrid.buildings.Single(q => q.id == jobSave.destinationID);
            human.destination.TryLink(human);
        }
        if (typeof(Building) == jobSave.objectType)
        {
            interest = MyGrid.buildings.Single(q => q.id == jobSave.objectId);
        }
        else if (typeof(Rock) == jobSave.objectType)
        {
            interest = SceneRefs.gridTiles.toBeDigged.FirstOrDefault(q => q.id == jobSave.objectId);
            interest.GetComponent<Rock>().assigned = human;
        }
        else if (typeof(Chunk) == jobSave.objectType)
        {
            interest = MyGrid.chunks.FirstOrDefault(q => q.id == jobSave.objectId);
        }
        if (interest)
        {
            if (!interest.Equals(human.destination))
                interest.GetComponent<StorageObject>()?.TryLink(human);
        }
    }
    public JobData(List<GridPos> _path, ClickableObject _interest)
    {
        path = _path;
        interest = _interest;
        job = JobState.Free;
    }
}

