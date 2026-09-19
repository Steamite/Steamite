using UnityEditor;
using UnityEditor.TestTools;
using System.Linq;

// Register this class so the Test Runner knows to use it
[assembly: TestPlayerBuildModifier(typeof(IncludeProjectScenesInTestBuild))]

public class IncludeProjectScenesInTestBuild : ITestPlayerBuildModifier
{
    public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
    {
        // 1. Get all enabled scenes from your normal File > Build Settings
        var projectScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToList();

        // 2. Get the temporary test scenes injected by the Test Runner
        var testScenes = playerOptions.scenes.ToList();

        // 3. Combine them (Test scenes must remain at the start of the list)
        // Union prevents duplicates if a scene was somehow added twice
        var combinedScenes = testScenes.Union(projectScenes).ToArray();

        // 4. Assign the combined list back to the build options
        playerOptions.scenes = combinedScenes;

        return playerOptions;
    }
}