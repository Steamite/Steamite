using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Job for humans.</summary>
[Serializable]
public struct JobData
{
    #region Variables
    /// <summary>Type of job, determines actions.</summary>
    public JobState job;
    /// <summary>Path to reach the interest, or the destination.</summary>
    public List<GridPos> path;
    /// <summary>Object to do something with.</summary>
    public ClickableObject interest;
    #endregion

    #region Constructors
    public JobData(JobSave jobSave, Human human)
    {
        job = jobSave.job;
        path = jobSave.path != null ? jobSave.path : new();
        interest = null;
        if (jobSave.destinationID > -1)
        {
            if (jobSave.destType == JobSave.InterestType.P)
                human.destination = MyGrid.GetPipes(q => q.id == jobSave.destinationID);
            else
                human.destination = MyGrid.GetBuilding(q => q.id == jobSave.destinationID);
            human.destination.TryLink(human);
        }

        switch (jobSave.interestType)
        {
            case JobSave.InterestType.B:
                interest = MyGrid.GetBuilding(q => q.id == jobSave.interestID);
                break;
            case JobSave.InterestType.R:
                interest = SceneRefs.jobQueue.toBeDug.FirstOrDefault(q => q.id == jobSave.interestID);
                interest.GetComponent<Rock>().Assigned = human;
                break;
            case JobSave.InterestType.C:
                interest = MyGrid.chunks.FirstOrDefault(q => q.id == jobSave.interestID);
                break;
            case JobSave.InterestType.P:
                interest = MyGrid.GetPipes(q => q.id == jobSave.interestID);
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
    #endregion
}