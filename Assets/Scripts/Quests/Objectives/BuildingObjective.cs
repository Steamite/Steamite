using Objectives;
using System;
using UnityEngine;

public class BuildingObjective : Objective
{
    [SerializeField] string _buildingTypeName;
    public string BuildingTypeName
    {
        get => _buildingTypeName;
        set => _buildingTypeName = value;
    }

    public override string Descr => $"Construct {_buildingTypeName}:";
    public BuildingObjective() { }

    public override bool UpdateProgress(object data, QuestController controller)
    {
        bool res = false;
        if(data.GetType().Name == _buildingTypeName)
        {
            res = base.UpdateProgress(data, controller);
            if (res)
                Cancel(controller);
        }
        return res;
    }

    public override void Load(int _currentProgressGlobal, Quest _quest, QuestController controller)
    {
        base.Load(_currentProgressGlobal, _quest, controller);
        controller.buildingObjectives.Add(this);
    }

    public override void Cancel(QuestController controller)
    {
        controller.buildingObjectives.Remove(this);
    }

}
