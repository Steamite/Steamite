using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests.Start
{
    public class ContinueGame : SceneLoadingForTests
    {
        [Test]
        public async Task Continue()
        {
            await ContinueGame();

            await Awaitable.WaitForSecondsAsync(2);
        }

        [Test]
        public async Task NewGame()
        {
            await CreatePredefinedWorld();

            await Awaitable.WaitForSecondsAsync(2);// WaitForSeconds(2);
        }

        /*[Test]
        public async Task GenerateMap()
        {
            await LoadMainMenu();
            LoadingScreen loadingScreen = GameObject.FindAnyObjectByType<LoadingScreen>();
            await loadingScreen.StartNewGame("_test_predefined", );

            Debug.Log(GameObject.FindAnyObjectByType<Rock>());
            Debug.Log("loaded");

            ClickOnLoadingScreen();

            await Awaitable.WaitForSecondsAsync(2);// WaitForSeconds(2);
        }*/
    }
}