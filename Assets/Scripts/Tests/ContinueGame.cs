using Assets.Scripts.Saving___Loading;
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

        [Test]
        public async Task GenerateMap()
        {
            await LoadMainMenu();
            /*
            LoadingScreenNew loadingScreen = GameObject.FindAnyObjectByType<LoadingScreenNew>();
            await loadingScreen.StartNewGame("_test_predefined", SplashScreen.SEED);
*/

            ClickOnLoadingScreen();

            await Awaitable.WaitForSecondsAsync(2);// WaitForSeconds(2);
        }
    }
}