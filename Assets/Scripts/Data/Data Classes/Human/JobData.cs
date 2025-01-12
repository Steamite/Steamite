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

        switch (jobSave.interestType)
        {
            case JobSave.InterestType.B:
                interest = MyGrid.buildings.Single(q => q.id == jobSave.interestID);
                break;
            case JobSave.InterestType.R:
                interest = SceneRefs.gridTiles.toBeDigged.FirstOrDefault(q => q.id == jobSave.interestID);
                interest.GetComponent<Rock>().Assigned = human;
                break;
            case JobSave.InterestType.C:
                interest = MyGrid.chunks.FirstOrDefault(q => q.id == jobSave.interestID);
                break;
            default:
                return;
        }
        if (!interest.Equals(human.destination))
            interest.GetComponent<StorageObject>()?.TryLink(human);
    }
    public JobData(List<GridPos> _path, ClickableObject _interest)
    {
        path = _path;
        interest = _interest;
        job = JobState.Free;
    }
}

