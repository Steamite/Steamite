using System;
using System.Collections.Generic;
using System.Text;
//using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Editor.Shaders
{
    /*public class ShaderCapture : MonoBehaviour
    {
        [SerializeField] GraphicsStateCollection collection;

        string path;
        private void Awake()
        {
            path = AssetDatabase.GetAssetPath(collection);

            collection = new();
            collection.BeginTrace();
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            collection.EndTrace();
            collection.SaveToFile(path);
        }
    }*/
}
