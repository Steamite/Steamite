
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.HumanHiding
{
    public partial struct SphereHiderComponent : IComponentData
    {
        public int prevLevel;
        public int currentLevel;
    }
}
