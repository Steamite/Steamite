using Mono.Cecil;
using Unity.Mathematics;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    public interface ICustomRenderer
    {
        public void OnLevelChange(int offsetScaled);
        public void Render(int2 position, int2 range);
    }
}