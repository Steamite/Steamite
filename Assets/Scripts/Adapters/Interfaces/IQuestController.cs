public interface IQuestController
{
    int Trust { get; set; }

    //[CreateProperty]public IEnumerable<DataObject> ActiveQuests { get; }
    public void DigRock(object rock);
    public void BuildBuilding(object building);
    public void ResChange();
}