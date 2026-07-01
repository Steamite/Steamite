using Assets.Scripts.Editor.Buildings.CommonColumns;
using Assets.Scripts.Editor.Buildings.SpecialColumns;
using ResearchUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace EditorWindows.Windows
{
    public class BuildingRegister : DataGridWindow<BuildCategWrapper, BuildingWrapper>
    {
        public const string BUILDING_PATH = "Assets/Game Data/Buildings/";
        public const string BUILD_NAME = "/building.prefab";
        public const string TEX_NAME = "/texture.png";

        Button rebindButton;

        List<Type> buildingTypes;
        

        bool changedType;

        AddressableAssetSettings settings;
        AddressableAssetGroup group;

        SpecialColumns specialColumns;
        CommonColumns commonColumns;

        public List<Type> BuildingTypes { get => buildingTypes; set => buildingTypes = value; }
        public bool ChangedType { get => changedType; set => changedType = value; }
        public AddressableAssetSettings Settings { get => settings; set => settings = value; }
        public AddressableAssetGroup Group { get => group; set => group = value; }

        [MenuItem("Custom Editors/Building register %g", priority = 14)]
        public static void Open()
        {
            BuildingRegister wnd = GetWindow<BuildingRegister>();
            wnd.titleContent = new("Building Register");
            wnd.minSize = new(800, 400);
        }

        #region Overrides
        protected override void CreateGUI()
        {
            BuildingTypes = TypeCache.GetTypesDerivedFrom(typeof(Building)).ToList();
            Holder = AssetDatabase.LoadAssetAtPath<BuildingData>(BuildingData.EDITOR_PATH);

            #region Grid
            base.CreateGUI();
            #endregion

            rebindButton = rootVisualElement.Q<Button>("Rebind-Buildings");
            rebindButton.enabledSelf = false;
            rebindButton.clicked +=
                () =>
                {
                    if (EditorUtility.DisplayDialog("Building reload?", "Do you want to reload buildings", "rebuild", "cancel"))
                    {
                        ResearchData researchData = AssetDatabase.LoadAssetAtPath<ResearchData>(ResearchData.EDITOR_PATH);
                        List<ResearchNode> nodes = researchData.Categories.SelectMany(q => q.Objects).Where(q => q.nodeType == NodeType.Building).ToList();
                        for (int i = 0; i < SelectedCategory.Objects.Count; i++)
                        {
                            Building building = SelectedCategory.Objects[i].building;
                            if (building != null)
                            {
                                byte categID = (byte)Holder.Categories.FirstOrDefault(q => q.Name == SelectedCategory.Name).id;
                                ResearchNode node = nodes.FirstOrDefault(q => q.objectConnection.categoryId == building.prefabConnection.categoryId && q.id == building.prefabConnection.objectId);
                                if (node != null)
                                    node.objectConnection.categoryId = categID;

                                building.prefabConnection = new(categID, SelectedCategory.Objects[i].id);
                                SelectedCategory.Objects[i].SetBuilding(SelectedCategory.Objects[i].building, categID);
                                EditorUtility.SetDirty(SelectedCategory.Objects[i].building);
                            }
                        }
                        EditorUtility.SetDirty(researchData);
                        EditorUtility.SetDirty(Holder);
                    }
                };
            categorySelector.index = 0;
        }
        protected override void RenameCateg()
        {
            AssetDatabase.MoveAsset($"{BUILDING_PATH}{SelectedCategory.Name}", $"{BUILDING_PATH}{categoryNameField.text}");
            if (Group != null)
            {
                Group.Name = categoryNameField.text;
                Settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRenamed, Group, true, false);
            }
            base.RenameCateg();
        }
        protected override void CreateCateg()
        {
            base.CreateCateg();
            AssetDatabase.CreateFolder($"Buildings", SelectedCategory.Name);
            Group = Settings.CreateGroup(SelectedCategory.Name, false, false, true, new() { }, new Type[0]);
            Settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, Group, true, false);
        }

        protected override bool RemoveCateg()
        {
            if (base.RemoveCateg())
            {
                AssetDatabase.MoveAsset($"{BUILDING_PATH}{SelectedCategory.Name}", $"{BUILDING_PATH}BCK/{SelectedCategory.Name}");
                Settings.RemoveGroup(Settings.FindGroup(SelectedCategory.Name));
                Settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, Group, true, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnDestroy()
        {
            if (SelectedCategory != null)
            {
                SelectedCategory.columnStates = new();
                for (int i = 0; i < dataGrid.columns.Count; i++)
                {
                    SelectedCategory.columnStates.Add(dataGrid.columns[i].visible);
                }
                EditorUtility.SetDirty((BuildingData)Holder);
            }
        }

        #region Category Switching
        protected override bool LoadCategData(int index)
        {
            Settings = AddressableAssetSettingsDefaultObject.Settings;
            bool boo = base.LoadCategData(index);
            if (boo)
            {
                Group = Settings.FindGroup(SelectedCategory.Name);
                for (int i = 0; i < SelectedCategory.columnStates?.Count; i++)
                    dataGrid.columns[i].visible = SelectedCategory.columnStates[i];
                rebindButton.enabledSelf = true;
            }
            else
            {
                SelectedCategory = new BuildCategWrapper();
                SelectedCategory.Objects = new();
                rebindButton.enabledSelf = false;
            }
            return boo;
        }

        #endregion
        #endregion


        #region Entry managment
        protected override void AddEntry(BaseListView _, bool add = false)
        {
            BuildingWrapper wrapper = new(Holder.UniqueID());
            int choice = EditorUtility.DisplayDialogComplex("Register a new building",
                "Do you want to fill the new entry or create an empty one?",
                "Filled", "Cancel", "Empty");
            if (choice == 0)
            {
                int i = 0;
                string folderName;
                string path;
                while (true)
                {
                    folderName = $"Dummy{i}";
                    path = $"{BUILDING_PATH}{SelectedCategory.Name}/{folderName}";
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        i++;
                        continue;
                    }
                    AssetDatabase.CreateFolder($"{BUILDING_PATH}{SelectedCategory.Name}", folderName);
                    break;
                }

                GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                SortingGroup sortGroup = gameObj.AddComponent<SortingGroup>();
                sortGroup.sortingLayerName = "Blueprint";
                sortGroup.sortingOrder = 10;
                gameObj.layer = 2;

                gameObj.AddComponent<Building>();
                gameObj.GetComponent<BoxCollider>().isTrigger = true;
                PrefabUtility.SaveAsPrefabAsset(gameObj, $"{path}{BUILD_NAME}");
                wrapper.SetBuilding(
                    AssetDatabase.LoadAssetAtPath<Building>($"{path}{BUILD_NAME}"),
                    (byte)Holder.Categories.First(q => q.Name == SelectedCategory.Name).id,
                    folderName);
                wrapper.preview = commonColumns.actions.GetPrefabPreview($"{path}");
                DestroyImmediate(gameObj);

                AddressableAssetEntry entry = Settings.CreateOrMoveEntry(AssetDatabase.GUIDFromAssetPath(path).ToString(), Group);
                entry.SetAddress(wrapper.building.objectName);
                Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, Group, true);
            }
            else if (choice == 1)
                return;
            SelectedCategory.Objects.Add(wrapper);
            base.AddEntry(_, false);
        }

        public void RemoveEntryPublic(BuildingWrapper wrapper, bool removeFromGrid) => RemoveEntry(wrapper, removeFromGrid);
        protected override void RemoveEntry(BuildingWrapper wrapper, bool removeFromGrid)
        {
            base.RemoveEntry(wrapper, removeFromGrid);
            if (wrapper.building)
            {
                AssetDatabase.MoveAsset($"{BUILDING_PATH}{SelectedCategory.Name}/{wrapper.building?.objectName}", $"{BUILDING_PATH}BCK/{wrapper.building?.objectName}");
                Settings.RemoveAssetEntry(AssetDatabase.GUIDFromAssetPath($"{BUILDING_PATH}BCK/{wrapper.building?.objectName}").ToString(), Group);
                Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, Group, true);
            }
        }
        #endregion


        #region Columns
        protected override void CreateColumns()
        {
            base.CreateColumns();
            commonColumns = new(dataGrid, this);
            commonColumns.CreateColumns();


            specialColumns = new SpecialColumns(dataGrid);
            specialColumns.CreateColumns();
        }

        protected override void NameChange(FocusOutEvent ev)
        {
            string value;
            if (ev.target is TextElement)
            {
                TextElement field = (TextElement)ev.target;
                value = field.text.Trim(' ');
                field.text = value;
            }
            else
            {
                TextField field = (TextField)ev.target;
                value = field.value.Trim(' ');
                field.value = value;
            }

            int i = ev.target.GetRowIndex();
            if (((BuildingWrapper)dataGrid.itemsSource[i]).building.objectName != value)
            {
                string oldPath = AssetDatabase.GetAssetPath(
                    ((BuildingWrapper)dataGrid.itemsSource[i]).building)
                    .Replace("/building.prefab", "");

                AddressableAssetEntry entry
                    = Group.GetAssetEntry(
                        AssetDatabase.GUIDFromAssetPath(oldPath).ToString());

                string result = AssetDatabase.MoveAsset(
                        $"{oldPath}",
                        $"{BUILDING_PATH}{SelectedCategory.Name}/{value}");
                if (result != "")
                {
                    Debug.LogError(result);
                    if (ev.target is TextElement)
                    {
                        ((TextElement)ev.target).text = ((BuildingWrapper)dataGrid.itemsSource[i]).building.objectName;
                    }
                    else
                    {
                        ((TextField)ev.target).value = ((BuildingWrapper)dataGrid.itemsSource[i]).building.objectName;
                    }
                    return;
                }

                entry.address = value;
                Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, value, true);
                ((BuildingWrapper)dataGrid.itemsSource[i]).building.objectName = value;
                EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).building);
            }
        }

        #endregion
    }
}