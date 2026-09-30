using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
#if UNITY_EDITOR
    public class SceneLoadingForTests : IPrebuildSetup, IPostBuildCleanup
    {
        public void Setup()
        {
            SessionState.SetBool(SceneLoadingShortucts.DISABLE_SCENELOADER, true);
        }

        [TearDown]
        public void Cleanup()
        {
            SessionState.EraseBool(SceneLoadingShortucts.DISABLE_SCENELOADER);
        }

#else
        public class SceneLoadingForTests
        {
#endif
        public async Awaitable LoadMainMenu()
        {
            var loadOp = SceneManager.LoadSceneAsync("Splash Screen", LoadSceneMode.Additive);
            if (loadOp == null)
            {
                Debug.LogError("Scene missing. Check Build Settings or ITestPlayerBuildModifier.");
                return;
            }
            if(!loadOp.isDone)
                await loadOp;


            bool loaded = false;
            SplashScreen splash = GameObject.FindAnyObjectByType<SplashScreen>();
            if (splash == null)
            {
                Debug.LogError("SplashScreen not found in the loaded scene!");
                return;
            }

            splash.afterLoad += () => loaded = true;
            
            while (!loaded)
            {
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
            return;
        }

        public async Awaitable ContinueGame()
        {
            LoadGameMenu menu = GameObject.FindAnyObjectByType<LoadGameMenu>();
            await menu.Continue();

            Debug.Log(GameObject.FindAnyObjectByType<Rock>());
            Debug.Log("loaded");

            ClickOnLoadingScreen();
        }

        public async Awaitable CreatePredefinedWorld()
        {
            await LoadMainMenu();
            LoadingScreen loadingScreen = GameObject.FindAnyObjectByType<LoadingScreen>();
            await loadingScreen.StartNewGame("_test_predefined");

            Debug.Log(GameObject.FindAnyObjectByType<Rock>());
            Debug.Log("loaded");

            ClickOnLoadingScreen();
        }

        public async Awaitable CreateRandomWorld(string name, string seed)
        {
            await LoadMainMenu();
            LoadingScreen loadingScreen = GameObject.FindAnyObjectByType<LoadingScreen>();
            await loadingScreen.StartNewGame(name, seed);

            Debug.Log(GameObject.FindAnyObjectByType<Rock>());
            Debug.Log("loaded");

            ClickOnLoadingScreen();
        }

        public static void ClickOnLoadingScreen()
        {
            InputTestFixture input = new InputTestFixture();
            var mouse = InputSystem.AddDevice<Mouse>();

            input.Set(mouse.position, new Vector2(Screen.width / 2, Screen.height / 2));
            input.Click(mouse.leftButton);
        }
    }
}
