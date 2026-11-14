using Unity.Collections;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public interface IWorldInit
    {
        void Init(ref WorldComponent worldComponent, ref NativeArray<LevelComponent> levelArray);
    }
}