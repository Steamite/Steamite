using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Editor.Columns;
using EditorWindows.Windows;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.CommonColumns
{
    public class CommonActions : ColumnActions
    {
        public BuildingRegister register;



        public Sprite GetPrefabPreview( string prefabPath)
        {
            string folder = Path.GetDirectoryName(prefabPath);

            // TODO use meshes
            Debug.Log("Generate preview for " + prefabPath);
            Mesh prefab = AssetDatabase.LoadAssetAtPath<Mesh>(prefabPath);
            var editor = UnityEditor.Editor.CreateEditor(prefab);
            Texture2D tex = editor.RenderStaticPreview(prefabPath, null, 200, 200);

            Color32 backgroundColor = new(82, 82, 82, 1);
            Color32[] colors = tex.GetPixels32();
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].r == backgroundColor.r &&
                    colors[i].g == backgroundColor.g &&
                    colors[i].b == backgroundColor.b)
                    colors[i] = new(0, 255, 0, 0);
            }
            tex = new(200, 200, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            tex.SetPixels32(colors);
            tex.Apply();
            byte[] b = tex.EncodeToPNG();

            string spritePath = Path.Combine(folder, BuildingRegister.TEX_NAME);
            File.WriteAllBytes(spritePath, b);
            AssetDatabase.Refresh();

            TextureImporter importer = TextureImporter.GetAtPath(spritePath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.spriteImportMode = SpriteImportMode.Single;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            UnityEditor.Editor.DestroyImmediate(editor);
            UnityEditor.Editor.DestroyImmediate(tex);
            return AssetDatabase.LoadAssetAtPath<Sprite>(importer.assetPath);
        }

        public void TypeChange(ChangeEvent<string> ev)
        {
            int i = ev.target.GetRowIndex();
            var wrapperProperty = register.GetBuildingWrapperAt(i);
            
            // Is there a building?
            if(((BuildingWrapper)wrapperProperty.boxedValue).Building == null)
            {
                wrapperProperty.FindPropertyRelative(nameof(BuildingWrapper.Building)).boxedValue = new BuildingNew();
                wrapperProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            BuildingWrapper wrapper = (BuildingWrapper)wrapperProperty.boxedValue;
            if (wrapper.Building != null)
            {
                Type t = register.BuildingTypes.FirstOrDefault(q => q.Name == ev.newValue);
                if (t != null && wrapper.Building.GetType() != t)
                {
                    BuildingNew building = (BuildingNew)Activator.CreateInstance(t);
                    building.CopyFrom(wrapper.Building);

                    wrapper.SetBuilding(building);

                    register.ChangedType = true;
                    view.RefreshItem(i);
                }
            }
        }

        public void BlueprintEvent(ClickEvent ev)
        {
            int i = ev.target.GetRowIndex();

            // TODO pass a serializedProperty
            BuildEditor.ShowWindow(register.GetBuildingWrapperAt(i));
        }

        public void PreviewClick(ClickEvent ev)
        {
            if (ev.clickCount == 2)
            {
                int i = ev.target.GetRowIndex();
                BuildingWrapper wrapper = view.itemsSource[i] as BuildingWrapper;
                /*if (wrapper.mesh)
                {
                    wrapper.preview =
                        GetPrefabPreview(AssetDatabase.GetAssetPath(wrapper.mesh));
                    EditorUtility.SetDirty(register.Holder);
                    view.RefreshItem(i);
                }*/
            }
        }

    }
}
