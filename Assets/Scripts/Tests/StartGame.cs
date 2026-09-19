using NUnit.Framework;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
public class StartGame : IPrebuildSetup, IPostBuildCleanup
{
    public void Setup()
    {
        // Runs in Edit Mode BEFORE Play Mode starts
        SessionState.SetBool(SceneLoadingShortucts.DISABLE_SCENELOADER, true);
    }

    [TearDown]
    public void Cleanup()
    {
        // Runs in Edit Mode AFTER tests finish and Play Mode stops
        SessionState.EraseBool(SceneLoadingShortucts.DISABLE_SCENELOADER);
    }
#else
public class StartGame
{
#endif
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator LoadGame()
    {
        var loadOp = SceneManager.LoadSceneAsync("Splash Screen", LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError("Scene missing. Check Build Settings or ITestPlayerBuildModifier.");
            yield break;
        }

        yield return loadOp;

        // 2. Cache the references rather than searching the scene multiple times
        bool loaded = false;
        SplashScreen splash = GameObject.FindAnyObjectByType<SplashScreen>();

        if (splash == null)
        {
            Debug.LogError("SplashScreen not found in the loaded scene!");
            yield break;
        }

        splash.afterLoad += () => loaded = true;

        while (!loaded)
        {
            yield return null;
        }

        LoadGameMenu menu = GameObject.FindAnyObjectByType<LoadGameMenu>();
        Awaitable.Awaiter awaiter = menu.Continue().GetAwaiter();

        while (!awaiter.IsCompleted)
        {
            yield return null;
        }

        // 3. CRITICAL: This throws any exceptions that happened inside Continue()
        // Without this, background thread crashes go completely unnoticed.
        awaiter.GetResult();

        Debug.Log(GameObject.FindAnyObjectByType<Rock>());
        Debug.Log("loaded");

        StartGame_onFinishLoad();

        yield return new WaitForSeconds(2);
    }
    private void StartGame_onFinishLoad()
    {
        InputTestFixture input = new InputTestFixture();
        var mouse = InputSystem.AddDevice<Mouse>();

        input.Set(mouse.position, new Vector2(Screen.width / 2, Screen.height / 2));
        input.Click(mouse.leftButton);
    }
}
