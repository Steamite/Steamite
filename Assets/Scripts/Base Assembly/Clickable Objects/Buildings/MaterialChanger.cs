using System.Threading.Tasks;
using UnityEngine;

public class MaterialChanger : MonoBehaviour, IBeforeLoad
{
    [SerializeField] Material opaqueMaterial;
    [SerializeField] Material transparentMaterial;
    static MaterialChanger instance;

    public static Material Opaque => instance.opaqueMaterial;
    public static Material Transparent => instance.transparentMaterial;
    public Task BeforeInit()
    {
        instance = this;
        return Task.CompletedTask;
    }
}
