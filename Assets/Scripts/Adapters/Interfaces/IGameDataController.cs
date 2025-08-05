using System.Threading.Tasks;

public interface IGameDataController<T>
{
    public Task LoadState(T saveData);
}