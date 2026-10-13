using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
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
/*
        AddressableAssetSettings settings;
        AddressableAssetGroup group;*/

        SpecialColumns specialColumns;
        CommonColumns commonColumns;

        SerializedObject holderObject;
        SerializedProperty selectedCategoryProperty;
        SerializedProperty selectedCategoryBuildingsProperty;
        public SerializedProperty GetBuildingWrapperAt(int i) =>
            selectedCategoryBuildingsProperty.GetArrayElementAtIndex(i);

        public List<Type> BuildingTypes { get => buildingTypes; set => buildingTypes = value; }
        public bool ChangedType { get => changedType; set => changedType = value; }
        /*public AddressableAssetSettings Settings { get => settings; set => settings = value; }
        public AddressableAssetGroup Group { get => group; set => group = value; }*/

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
            BuildingTypes = TypeCache.GetTypesDerivedFrom(typeof(BuildingNew)).ToList();
            Holder = AssetDatabase.LoadAssetAtPath<BuildingData>(BuildingData.EDITOR_PATH);
            holderObject = new(Holder);

            #region Grid
            base.CreateGUI();
            #endregion

            /*rebindButton = rootVisualElement.Q<Button>("Rebind-Buildings");
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
                            Building building = SelectedCategory.Objects[i].Building;
                            if (building != null)
                            {
                                byte categID = (byte)Holder.Categories.FirstOrDefault(q => q.Name == SelectedCategory.Name).id;
                                ResearchNode node = nodes.FirstOrDefault(q => 
                                    q.objectConnection.categoryId == building.PrefabConnection.categoryId && 
                                    q.id == building.PrefabConnection.objectId);
                                if (node != null)
                                    node.objectConnection.categoryId = categID;

                                building.PrefabConnection = new(categID, SelectedCategory.Objects[i].id);
                                SelectedCategory.Objects[i].SetBuilding(SelectedCategory.Objects[i].Building, categID);
                                EditorUtility.SetDirty(SelectedCategory.Objects[i].Building);
                            }
                        }
                        EditorUtility.SetDirty(researchData);
                        EditorUtility.SetDirty(Holder);
                    }
                };*/
            categorySelector.index = 0;
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
            //Settings = AddressableAssetSettingsDefaultObject.Settings;
            bool boo = base.LoadCategData(index);
            if (boo)
            {
                selectedCategoryProperty = holderObject.FindProperty(nameof(BuildingData.Categories)).GetArrayElementAtIndex(index);
                selectedCategoryBuildingsProperty = selectedCategoryProperty.FindPropertyRelative(nameof(BuildCategWrapper.Objects));
                /*dataGrid.itemsSource = null;*/
                dataGrid.BindProperty(selectedCategoryBuildingsProperty);

                for (int i = 0; i < SelectedCategory.columnStates?.Count; i++)
                    dataGrid.columns[i].visible = SelectedCategory.columnStates[i];
                ///rebindButton.enabledSelf = true;
            }
            else
            {
                SelectedCategory = new BuildCategWrapper();
                SelectedCategory.Objects = new();
                //rebindButton.enabledSelf = false;
            }
            return boo;
        }

        #endregion
        #endregion


        #region Entry managment
        /*protected override void AddEntry(BaseListView _, bool add = false)
        {
            BuildingWrapper wrapper = new(Holder.UniqueID());
            wrapper.SetBuilding(new());
            SelectedCategory.Objects.Add(wrapper);
            base.AddEntry(_, false);
        }*/

        //public void RemoveEntryPublic(BuildingWrapper wrapper, bool removeFromGrid) => RemoveEntry(wrapper, removeFromGrid);
        /*protected override void RemoveEntry(BuildingWrapper wrapper, bool removeFromGrid)
        {
            base.RemoveEntry(wrapper, removeFromGrid);
            if (wrapper.Building)
            {
                AssetDatabase.MoveAsset($"{BUILDING_PATH}{SelectedCategory.Name}/{wrapper.Building?.Name}", $"{BUILDING_PATH}BCK/{wrapper.Building?.Name}");
                Settings.RemoveAssetEntry(AssetDatabase.GUIDFromAssetPath($"{BUILDING_PATH}BCK/{wrapper.Building?.Name}").ToString(), Group);
                Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, Group, true);
            }
        }*/
        #endregion


        #region Columns
        protected override void CreateColumns()
        {
            //base.CreateColumns();

            commonColumns = new(dataGrid, this);
            commonColumns.CreateColumns();


            specialColumns = new SpecialColumns(dataGrid, this);
            specialColumns.CreateColumns();
        }

        #endregion
    }
}