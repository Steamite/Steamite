using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    public class TestRender : MonoBehaviour
    {
        [SerializeField] Mesh mesh;
        [SerializeField] Material material;
        RenderParams Params;

        private void Awake()
        {
            Params = new(material);
            enabled = false;
        }
        public void Update()
        {
            Graphics.RenderMesh(Params, mesh, 0, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
        }

    }
}
