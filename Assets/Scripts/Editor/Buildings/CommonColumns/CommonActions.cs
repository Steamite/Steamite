using Assets.Scripts.Editor.Columns;
using EditorWindows.Windows;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.CommonColumns
{
    public class CommonActions : ColumnActions
    {
        public BuildingRegister register;

        public void AssetChange(ChangeEvent<UnityEngine.Object> ev)
        {
            int i = ev.target.GetRowIndex();
            if (register.ChangedType || ((BuildingData)register.Holder).ContainsBuilding((Building)ev.newValue) == false)
            {
                Building b = ev.newValue as Building;
                if (b != null && !register.ChangedType)
                {
                    string path = $"{BuildingRegister.BUILDING_PATH}{register.SelectedCategory.Name}";
                    string _name = b.Name.Length > 0 ? b.Name : UnityEngine.Random.Range(0, int.MaxValue).ToString();
                    if (!Directory.Exists(path + "/" + _name))
                    {
                        string GUID = AssetDatabase.CreateFolder(path, $"{_name}");
                        if ((path = AssetDatabase.GUIDToAssetPath(GUID)) != "")
                        {
                            string oldPath = AssetDatabase.GetAssetPath(ev.newValue);
                            AssetDatabase.MoveAsset(oldPath, $"{path}{BuildingRegister.BUILD_NAME}");
                            ((BuildingWrapper)view.itemsSource[i]).preview = GetPrefabPreview(path);

                            if (oldPath.Contains("/BCK/"))
                                AssetDatabase.DeleteAsset(Path.GetDirectoryName(oldPath));
                            AssetDatabase.Refresh();

                            AddressableAssetEntry entry = register.Settings.CreateOrMoveEntry(GUID, register.Group);
                            entry.SetAddress(_name);
                            register.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, register.Group, true);
                        }
                        else
                            Debug.LogError($"Cannot create {BuildingRegister.BUILDING_PATH}{register.SelectedCategory.Name}!");
                    }
                    else
                    {
                        Debug.LogError("Already exists!\n" + path + "/" + _name);
                    }
                }
                else if (b == null)
                {
                    register.RemoveEntryPublic(view.itemsSource[i] as BuildingWrapper, false);
                }

                ((BuildingWrapper)view.itemsSource[i]).SetBuilding(
                    b,
                    (byte)register.Holder.Categories.FindIndex(q => q.Name == register.SelectedCategory.Name));
                register.ChangedType = false;
                view.RefreshItem(i);
                EditorUtility.SetDirty((BuildingData)register.Holder);
            }
            else
            {
                ((ObjectField)ev.target).SetValueWithoutNotify(((BuildingWrapper)view.itemsSource[i]).building);
            }
        }

        public Sprite GetPrefabPreview(string folderPath)
        {
            Debug.Log("Generate preview for " + folderPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folderPath}{BuildingRegister.BUILD_NAME}");
            var editor = UnityEditor.Editor.CreateEditor(prefab);
            Texture2D tex = editor.RenderStaticPreview($"{folderPath}{BuildingRegister.BUILD_NAME}", null, 200, 200);

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

            File.WriteAllBytes($"{folderPath}{BuildingRegister.TEX_NAME}", b);
            AssetDatabase.Refresh();

            TextureImporter importer = TextureImporter.GetAtPath($"{folderPath}{BuildingRegister.TEX_NAME}") as TextureImporter;
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
            Building prev = ((BuildingWrapper)view.itemsSource[i]).building;
            if (prev != null)
            {
                Type t = register.BuildingTypes.FirstOrDefault(q => q.Name == ev.newValue);
                if (t != null && prev.GetType() != t)
                {
                    Building building = (Building)
                        ((BuildingWrapper)view.itemsSource[i]).building.gameObject
                        .AddComponent(t);

                    building.Clone(prev);
                    UnityEditor.Editor.DestroyImmediate(prev, true);
                    EditorUtility.SetDirty(building.gameObject);
                    ((BuildingWrapper)view.itemsSource[i]).SetBuilding(building, ((byte)register.Holder.Categories.FindIndex(q => q.Name == register.SelectedCategory.Name)));
                    register.ChangedType = true;
                    view.RefreshItem(i);
                }
            }
        }

        public void BlueprintEvent(ClickEvent ev)
        {
            int i = ev.target.GetRowIndex();
            BuildEditor.ShowWindow(((BuildingWrapper)view.itemsSource[i]).building);
        }

        public void PreviewClick(ClickEvent ev)
        {
            if (ev.clickCount == 2)
            {
                int i = ev.target.GetRowIndex();
                BuildingWrapper wrapper = view.itemsSource[i] as BuildingWrapper;
                if (wrapper.building)
                {
                    wrapper.preview =
                        GetPrefabPreview(Path.GetDirectoryName(AssetDatabase.GetAssetPath(wrapper.building)));
                    EditorUtility.SetDirty(register.Holder);
                    view.RefreshItem(i);
                }
            }
        }

    }
}
