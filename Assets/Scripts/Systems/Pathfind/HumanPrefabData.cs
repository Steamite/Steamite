using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Pathing
{
    [CreateAssetMenu(fileName = "humanPref", menuName = "Create Menu")]
    class HumanPrefabData : ScriptableObject
    {
        public int count;
        public float speed;


        public int3 position;
    }
}
