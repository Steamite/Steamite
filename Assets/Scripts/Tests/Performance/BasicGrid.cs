using AbstractControls;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tests;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Tests.Performance
{
    internal class BasicGridInteractions : SceneLoadingForTests
    {
        private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1);
        Vector2Int move = new Vector2Int(500, 200);

        [UnityTest]
        public IEnumerator ClickOnGrid()
        {
            yield return CreatePredefinedWorld();

            yield return _waitForSeconds1;

            InputTestFixture input = new InputTestFixture();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();

            yield return TestCamera(move, input, keyboard);
            yield return _waitForSeconds1;
            yield return TestCamera(-move, input, keyboard);
            InputSystem.RemoveDevice(keyboard);
        }

        [UnityTest]
        public IEnumerator ClickOnLargeGrid()
        {
            yield return CreateRandomWorld("_test_random_camera", "24D14E69");

            yield return _waitForSeconds1;

            InputTestFixture input = new InputTestFixture();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();

            yield return TestCamera(move, input, keyboard);
            yield return _waitForSeconds1;
            yield return TestCamera(-move, input, keyboard);
            InputSystem.RemoveDevice(keyboard);
        }

        IEnumerator TestCamera(Vector2Int distance, InputTestFixture input, Keyboard keyboard)
        {

            Key hKey = distance.x < 0 ? Key.A : (distance.x > 0 ? Key.D : Key.None);
            Key vKey = distance.y < 0 ? Key.S : (distance.y > 0 ? Key.W : Key.None);


            distance.x = Mathf.Abs(distance.x);
            distance.y = Mathf.Abs(distance.y);

            KeyboardState kbState = new KeyboardState();

            if (hKey != Key.None) kbState.Set(hKey, true);
            if (vKey != Key.None) kbState.Set(vKey, true);

            InputSystem.QueueStateEvent(keyboard, kbState);

            int max = Math.Max(distance.x, distance.y);

            for (int i = 0; i <= max; i++)
            {
                bool stateChanged = false;

                // When a specific axis reaches its max distance, unpress its key
                if (i == distance.x && hKey != Key.None)
                {
                    kbState.Set(hKey, false);
                    stateChanged = true;
                }
                if (i == distance.y && vKey != Key.None)
                {
                    kbState.Set(vKey, false);
                    stateChanged = true;
                }

                // If a key was released this frame, push the new updated state
                if (stateChanged)
                {
                    InputSystem.QueueStateEvent(keyboard, kbState);
                }

                yield return null; // Advance the game frame
            }

            /*Debug.Log(horizontal);
            Debug.Log(vertical);

            input.Press(horizontal);
            input.Press(vertical);

            int max = Math.Max(distance.x, distance.y);

            for (int i = 0; i <= max; i++)
            {
                if (i == distance.x)
                    input.Release(horizontal);
                if (i == distance.y)
                    input.Release(vertical);
                yield return null;
            }
            input.Release(horizontal);
            input.Release(vertical);*/
        }
    }
}
