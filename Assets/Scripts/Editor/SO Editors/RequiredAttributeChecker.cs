#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;

[InitializeOnLoad]
public static class RequiredAttributeChecker
{
    static RequiredAttributeChecker()
    {
        // Subscribe to the play mode state change event
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Run the check right before the game actually starts playing
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            CheckForMissingReferences();
        }
    }


    [MenuItem("Custom Editors/Validate Missing Fields _F10")]
    public static void CheckForMissingReferences()
    {
        // Find all MonoBehaviours in the current scene
        MonoBehaviour[] scripts = Object.FindObjectsByType<MonoBehaviour>();

        foreach (var script in scripts)
        {
            if (script == null) continue;

            // Use reflection to find fields with the [Required] attribute
            var fields = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<RequiredAttribute>() != null);

            foreach (var field in fields)
            {
                object value = field.GetValue(script);
                bool isMissing = false;

                // Check if it's a null Unity Object or an empty string
                if (value == null || value.Equals(null))
                {
                    isMissing = true;
                }
                else if (value is string str && string.IsNullOrEmpty(str))
                {
                    isMissing = true;
                }

                if (isMissing)
                {
                    RequiredAttribute attr = field.GetCustomAttribute<RequiredAttribute>();

                    // Log the error. Passing 'script.gameObject' allows you to click 
                    // the console error to highlight the broken object in the hierarchy!
                    Debug.LogError($"[Required Field Missing] '{field.Name}' on '{script.gameObject.name}' is empty! {attr.Message}", script.gameObject);

                    // Optional: Automatically pause the editor if a required field is missing
                    EditorApplication.isPaused = true;
                }
            }
        }
    }
}
#endif