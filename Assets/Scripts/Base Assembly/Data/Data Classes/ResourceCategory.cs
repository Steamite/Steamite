using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(fileName = "ResourceCategory", menuName = "Resources/ResourceCategory")]
public class ResourceCategory : ScriptableObject
{
    public string Name;
    public Texture2D icon;
}
