using System.Collections.Generic;
using Unity.Properties;

public interface IQuestController
{
    [CreateProperty]public IEnumerable<DataObject> ActiveQuests { get; }
    public void DigRock(object rock);
    public void BuildBuilding(object building);
    public void ResChange();
}