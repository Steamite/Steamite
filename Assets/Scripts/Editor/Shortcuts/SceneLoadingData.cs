using UnityEngine;


[CreateAssetMenu(fileName = "EditorData", menuName = "Editor/SceneLoadingData")]
public class SceneLoadingData : ScriptableObject
{
    public bool load;

    public SceneLoadingData()
    {
    }
}