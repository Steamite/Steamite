using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class MaterialChanger : MonoBehaviour, IBeforeLoad
{
    [SerializeField] Material opaqueMaterial;
    [SerializeField] Material transparentMaterial;
    static MaterialChanger instance;

    public static Material Opaque => instance.opaqueMaterial;
    public static Material Transparent => instance.transparentMaterial;
    public Task Init()
    {
        instance = this;
        return Task.CompletedTask;
    }
}
