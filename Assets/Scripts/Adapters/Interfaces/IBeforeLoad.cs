using System.Threading.Tasks;
using UnityEngine;

public interface IBeforeLoadAwaitable
{
    public Awaitable BeforeInit();
}
public interface IBeforeLoad
{
    public void BeforeInit();
}