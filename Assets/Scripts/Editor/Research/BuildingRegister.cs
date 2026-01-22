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
using Object = UnityEngine.Object;

namespace EditorWindows.Windows
{
    public class BuildingRegister : DataGridWindow<BuildCategWrapper, BuildingWrapper>
    {
        const string BUILDING_PATH = "Assets/Game Data/Buildings/";
        const string BUILD_NAME = "/building.prefab";
        const string TEX_NAME = "/texture.png";

        Button rebindButton;

        List<Type> buildingTypes;

        bool changedType;

        AddressableAssetSettings settings;
        AddressableAssetGroup group;

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
            buildingTypes = TypeCache.GetTypesDerivedFrom(typeof(Building)).ToList();
            holder = AssetDatabase.LoadAssetAtPath<BuildingData>(BuildingData.EDITOR_PATH);

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
                        for (int i = 0; i < selectedCategory.Objects.Count; i++)
                        {
                            Building building = selectedCategory.Objects[i].building;
                            if (building != null)
                            {
                                byte categID = (byte)holder.Categories.FirstOrDefault(q => q.Name == selectedCategory.Name).id;
                                ResearchNode node = nodes.FirstOrDefault(q => q.objectConnection.categoryId == building.prefabConnection.categoryId && q.id == building.prefabConnection.objectId);
                                if (node != null)
                                    node.objectConnection.categoryId = categID;

                                building.prefabConnection = new(categID, selectedCategory.Objects[i].id);
                                selectedCategory.Objects[i].SetBuilding(selectedCategory.Objects[i].building, categID);
                                EditorUtility.SetDirty(selectedCategory.Objects[i].building);
                            }
                        }
                        EditorUtility.SetDirty(researchData);
                        EditorUtility.SetDirty(holder);
                    }
                };
            categorySelector.index = 0;
        }
        protected override void RenameCateg()
        {
            AssetDatabase.MoveAsset($"{BUILDING_PATH}{selectedCategory.Name}", $"{BUILDING_PATH}{categoryNameField.text}");
            if (group != null)
            {
                group.Name = categoryNameField.text;
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRenamed, group, true, false);
            }
            base.RenameCateg();
        }
        protected override void CreateCateg()
        {
            base.CreateCateg();
            AssetDatabase.CreateFolder($"Buildings", selectedCategory.Name);
            group = settings.CreateGroup(selectedCategory.Name, false, false, true, new() { }, new Type[0]);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, group, true, false);
        }

        protected override bool RemoveCateg()
        {
            if (base.RemoveCateg())
            {
                AssetDatabase.MoveAsset($"{BUILDING_PATH}{selectedCategory.Name}", $"{BUILDING_PATH}BCK/{selectedCategory.Name}");
                settings.RemoveGroup(settings.FindGroup(selectedCategory.Name));
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, group, true, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnDestroy()
        {
            if (selectedCategory != null)
            {
                selectedCategory.columnStates = new();
                for (int i = 0; i < dataGrid.columns.Count; i++)
                {
                    selectedCategory.columnStates.Add(dataGrid.columns[i].visible);
                }
                EditorUtility.SetDirty((BuildingData)holder);
            }
        }

        #region Category Switching
        protected override bool LoadCategData(int index)
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            bool boo = base.LoadCategData(index);
            if (boo)
            {
                group = settings.FindGroup(selectedCategory.Name);
                for (int i = 0; i < selectedCategory.columnStates?.Count; i++)
                    dataGrid.columns[i].visible = selectedCategory.columnStates[i];
                rebindButton.enabledSelf = true;
            }
            else
            {
                selectedCategory = new BuildCategWrapper();
                selectedCategory.Objects = new();
                rebindButton.enabledSelf = false;
            }
            return boo;
        }

        #endregion
        #endregion


        #region Entry managment
        protected override void AddEntry(BaseListView _, bool add = false)
        {
            BuildingWrapper wrapper = new(holder.UniqueID());
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
                    folderName =  $"Dummy{i}";
                    path = $"{BUILDING_PATH}{selectedCategory.Name}/{folderName}";
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        i++;
                        continue; 
                    }
                    AssetDatabase.CreateFolder($"{BUILDING_PATH}{selectedCategory.Name}", folderName);
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
                    (byte)holder.Categories.First(q => q.Name == selectedCategory.Name).id,
                    folderName);
                wrapper.preview = GetPrefabPreview($"{path}");
                DestroyImmediate(gameObj);

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.GUIDFromAssetPath(path).ToString(), group);
                entry.SetAddress(wrapper.building.objectName);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, group, true);
            }
            else if (choice == 1)
                return;
            selectedCategory.Objects.Add(wrapper);
            base.AddEntry(_, false);
        }

        protected override void RemoveEntry(BuildingWrapper wrapper, bool removeFromGrid)
        {
            base.RemoveEntry(wrapper, removeFromGrid);
            if (wrapper.building)
            {
                AssetDatabase.MoveAsset($"{BUILDING_PATH}{selectedCategory.Name}/{wrapper.building?.objectName}", $"{BUILDING_PATH}BCK/{wrapper.building?.objectName}");
                settings.RemoveAssetEntry(AssetDatabase.GUIDFromAssetPath($"{BUILDING_PATH}BCK/{wrapper.building?.objectName}").ToString(), group);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, group, true);
            }
        }
        #endregion


        #region Columns
        protected override void CreateColumns()
        {
            #region Base
            base.CreateColumns();
            #region Asset
            dataGrid.columns["asset"].makeCell =
                () => new ObjectField();
            dataGrid.columns["asset"].bindCell =
                (el, i) =>
                {
                    ObjectField field = (ObjectField)el;
                    field.allowSceneObjects = false;
                    field.objectType = typeof(Building);
                    field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    field.RegisterValueChangedCallback(AssetChange);
                };
            dataGrid.columns["asset"].unbindCell =
                (el, i) =>
                {
                    ObjectField field = (ObjectField)el;
                    field.UnregisterValueChangedCallback(AssetChange);
                };
            #endregion

            #region Type
            dataGrid.columns["type"].makeCell =
                () => new DropdownField();
            dataGrid.columns["type"].bindCell =
                (el, i) =>
                {
                    DropdownField field = (DropdownField)el;
                    field.choices = buildingTypes.Select(q => q.Name).ToList();//.Select(q => q.Name).Where(q => !q.Contains("Pipe")).ToList();
                    field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).building
                        ? ((BuildingWrapper)dataGrid.itemsSource[i]).building.GetType().ToString()
                        : "None";
                    field.RegisterValueChangedCallback(TypeChange);
                };
            dataGrid.columns["type"].unbindCell =
                (el, i) =>
                {
                    DropdownField field = (DropdownField)el;
                    field.UnregisterValueChangedCallback(TypeChange);
                };
            #endregion

            #region Cost
            dataGrid.columns["cost"].makeCell =
                () => new ResourceCell();
            dataGrid.columns["cost"].bindCell =
                (el, i) =>
                {
                    el.parent.focusable = true;
                    ResourceCell cell = el.Q<ResourceCell>();
                    cell.Open(((BuildingWrapper)dataGrid.itemsSource[i]).building?.Cost, ((BuildingWrapper)dataGrid.itemsSource[i]).building, true);
                };
            #endregion

            #region Blueprint
            dataGrid.columns["blueprint"].makeCell =
                () =>
                {
                    Button b = new Button();
                    b.style.alignSelf = Align.Center;
                    b.style.justifyContent = Justify.Center;
                    return b;
                };
            dataGrid.columns["blueprint"].bindCell =
                (el, i) =>
                {
                    Button button = el.Q<Button>();
                    button.text = "Manage";
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (building)
                    {
                        if (building.blueprint.itemList == null ||
                            building.blueprint.itemList.Count == 0 ||
                            (building is not Pipe &&
                                (building.blueprint.itemList.Count(q => q.itemType == GridItemType.Anchor) == 0 ||
                                 building.blueprint.itemList.Count(q => q.itemType == GridItemType.Entrance) == 0))
                            || (building is Pipe && building.blueprint.itemList.Count(q => q.itemType == GridItemType.Pipe) == 0))
                        {
                            button.style.color = Color.red;
                        }
                        else
                        {
                            button.style.color = Color.white;
                        }

                        button.RegisterCallback<ClickEvent>(BlueprintEvent);
                        button.SetEnabled(true);
                    }
                    else
                    {
                        button.SetEnabled(false);
                    }
                };
            dataGrid.columns["blueprint"].unbindCell =
                (el, i) =>
                {
                    Button button = el.Q<Button>();
                    button.UnregisterCallback<ClickEvent>(BlueprintEvent);
                };
            #endregion

            #region Preview
            dataGrid.columns.Add(new()
            {
                title = "Preview",
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    /*
					BuildingWrapper wrapper = dataGrid.itemsSource[i] as BuildingWrapper;
					if (wrapper.preview == null && wrapper.b != null)
					{
						wrapper.preview = GetPrefabPreview(AssetDatabase.GetAssetPath(wrapper.b));
						EditorUtility.SetDirty((BuildButtonHolder)data);
					}*/
                    el.RegisterCallback<ClickEvent>(PreviewClick);
                    el.style.backgroundImage = new StyleBackground(((BuildingWrapper)dataGrid.itemsSource[i]).preview);

                    el.style.width = 50;
                    el.style.height = 50;
                },
                unbindCell = (el, i) =>
                {
                    el.UnregisterCallback<ClickEvent>(PreviewClick);
                },
                width = 50,
                resizable = false
            });
            #endregion

            #endregion

            #region Assign Limit
            // Needs to have a field in the class, so it can be serialized.
            dataGrid.columns.Add(new()
            {
                name = "limit",
                title = "Assign",
                width = 75,
                resizable = false,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    Building buildingWrapper = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (buildingWrapper is IAssign)
                    {
                        if (((IAssign)buildingWrapper).AssignLimit == null)
                        {
                            ((IAssign)buildingWrapper).AssignLimit = new();
                            EditorUtility.SetDirty(buildingWrapper);
                        }
                        field.value = ((IAssign)buildingWrapper).AssignLimit.BaseValue;
                        field.SetEnabled(true);
                        field.RegisterValueChangedCallback(AssignChange);
                    }
                    else
                    {
                        field.SetEnabled(false);
                    }
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    field.UnregisterValueChangedCallback(AssignChange);
                }
            });
            #endregion

            #region Production

            ProductionRecipeHolder holder = AssetDatabase.LoadAssetAtPath<ProductionRecipeHolder>(ProductionRecipeHolder.PATH);
            dataGrid.columns.Add(new()
            {
                name = "recipes",
                title = "Recipes",
                width = 150,
                resizable = false,
                makeCell = () => new RecipeCell(),
                bindCell = (el, i) =>
                {
                    RecipeCell cell = el as RecipeCell;
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (building is IResourceProduction production)
                    {
                        cell.userData = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                        cell.Open(production);
                    }
                    else
                        cell.Open(null);
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    field.UnregisterValueChangedCallback(ProdTimeChange);
                }
            });
            /*
            #region Time
            dataGrid.columns.Add(new()
            {
                name = "prodTime",
                title = "Prod. time",
                width = 75,
                resizable = false,
                makeCell = () => new IntegerField(),
                bindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    if (((BuildingWrapper)dataGrid.itemsSource[i]).building is IProduction)
                    {
                        field.SetEnabled(true);
                        field.value = Convert.ToInt32(((IProduction)((BuildingWrapper)dataGrid.itemsSource[i]).building).ProdTime);
                        field.RegisterValueChangedCallback(ProdTimeChange);
                    }
                    else
                        field.SetEnabled(false);
                },
                unbindCell = (el, i) =>
                {
                    IntegerField field = el.Q<IntegerField>();
                    field.UnregisterValueChangedCallback(ProdTimeChange);
                }
            });
            #endregion

            #region Input

            dataGrid.columns.Add(new()
            {
                name = "prodCost",
                title = "Input",
                minWidth = dataGrid.columns["cost"].minWidth,
                maxWidth = dataGrid.columns["cost"].maxWidth,
                resizable = true,

                makeCell = () =>
                {
                    VisualElement element = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    element.Add(new ResourceCell());
                    element.Add(new Label("Fluid:") { name = "fluidLabel"});
                    element.Add(new ResourceCell());
                    return element;
                },
                bindCell = (el, i) =>
                {
                    el.parent.focusable = true;
                    ResourceCell rCell = el[0] as ResourceCell;
                    Building b = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (b is IResourceProduction resProd)
                        rCell.Open(
                            resProd.ResourceCost.EditorResource,
                            b, false);
                    else
                        rCell.Open(null, null, false);
                    
                    ResourceCell fCell = el[2] as ResourceCell;
                    if (b is FluidResProductionBuilding fluidProd)
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.Flex;
                        fCell.Open(
                            fluidProd.FluidCost,
                            b, false);
                    }
                    else
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.None;
                        fCell.Open(null, null, false);
                    }
                }
            });
            #endregion

            #region Yeild
            dataGrid.columns.Add(new()
            {
                name = "prod",
                title = "Yield",
                minWidth = dataGrid.columns["cost"].minWidth,
                maxWidth = dataGrid.columns["cost"].maxWidth,
                resizable = true,

                makeCell = () => 
                {
                    VisualElement element = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    element.Add(new ResourceCell());
                    element.Add(new Label("Fluid:") { name = "fluidLabel" });
                    element.Add(new ResourceCell());
                    return element;
                },
                bindCell = (el, i) =>
                {
                    el.parent.focusable = true;
                    ResourceCell cell = el[0] as ResourceCell;
                    Building b = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (b is IResourceProduction)
                        cell.Open(
                            ((IResourceProduction)b)?.ResourceYield.EditorResource,
                            ((BuildingWrapper)dataGrid.itemsSource[i]).building, false);
                    else
                        cell.Open(null, null, false);

                    ResourceCell fCell = el[2] as ResourceCell;
                    if (b is FluidResProductionBuilding fluidProd)
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.Flex;
                        fCell.Open(
                            fluidProd.FluidYeild,
                            b, false);
                    }
                    else
                    {
                        el.Q<Label>("fluidLabel").style.display = DisplayStyle.None;
                        fCell.Open(null, null, false);
                    }
                }
            });
            
            #endregion
            */
            #endregion

            #region Storing
            dataGrid.columns.Add(new()
            {
                name = "canStore",
                title = "Can store",
                resizable = true,
                width = 150,
                makeCell = () => new Mask64Field(),
                bindCell = (el, i) =>
                {
                    Mask64Field field = (Mask64Field)el;
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
                    if (building is IStorage storage)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetResNamesList();
                        field.choices = choices;
                        field.value = storage.CanStoreMask;
                        field.RegisterValueChangedCallback(CanStoreFluidsChange);
                    }
                    else if (building is FluidTank tank)
                    {
                        el.style.display = DisplayStyle.Flex;
                        List<string> choices = ResFluidTypes.GetFluidNames();
                        field.choices = choices;
                        field.value = tank.TypesToStore;
                        field.RegisterValueChangedCallback(CanStoreFluidsChange);
                    }
                    else
                    {
                        el.style.display = DisplayStyle.None;
                    }
                },
                unbindCell = (el, i) =>
                {
                    Mask64Field field = (Mask64Field)el;
                    field.UnregisterValueChangedCallback(CanStoreFluidsChange);
                },
            });
            #endregion

            #region Mask
            dataGrid.columns.Add(new()
            {
                name = "CategMask",
                title = "Category Mask",
                resizable = true,
                width = 150,
                makeCell = () => new MaskField(),
                bindCell = (el, i) =>
                {
                    MaskField field = (MaskField)el;
                    field.choices = Enum.GetNames(typeof(BuildingCategType)).ToList();
                    field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).building
                        ? ((BuildingWrapper)dataGrid.itemsSource[i]).building.BuildingCateg
                        : 0;
                    field.RegisterValueChangedCallback(CategoryChange);
                },
                unbindCell = (el, i) =>
                {
                    MaskField field = (MaskField)el;
                    field.UnregisterValueChangedCallback(CategoryChange);
                },
            });
            #endregion

            #region Capacity
            dataGrid.columns.Add(new()
            {
                name = "Capacity",
                title = "Capacity",
                resizable = true,
                width = 75,
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    el.Clear();
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;

                    IntegerField field = new();
                    if (building != null)
                    {
                        el.Add(field);
                        if(building is FluidTank tank)
                        {
                            if (tank.StoredFluids.capacity == null)
                                tank.StoredFluids.capacity = new(-1);
                            field.value = tank.StoredFluids.capacity.BaseValue;
                            field.RegisterValueChangedCallback(FluidCapacityChanged);
                        }
                        else
                        {
                            if (building.LocalRes.capacity == null)
                                building.LocalRes.capacity = new(-1);
                            field.value = building.LocalRes.capacity.BaseValue;
                            field.RegisterValueChangedCallback(StorageCapacityChanged);
                        }
                    }

                    if (building is FluidResProductionBuilding fluidRes)
                    {
                        field = new IntegerField();
                        if (fluidRes.StoredFluids.capacity == null)
                            fluidRes.StoredFluids.capacity = new(-1);
                        field.value = fluidRes.StoredFluids.capacity.BaseValue;

                        el.Add(field);
                        field.RegisterValueChangedCallback(FluidCapacityChanged);
                    }

                },
                unbindCell = (el, i) =>
                {
                    try
                    {
                        (el[0] as IntegerField).UnregisterValueChangedCallback(StorageCapacityChanged);
                        if (el.childCount > 1)
                            (el[1] as IntegerField).UnregisterValueChangedCallback(FluidCapacityChanged);
                        el.Clear();
                    }
                    catch { }
                },
            });
            #endregion

            #region
            dataGrid.columns.Add(new()
            {
                name = "Range",
                title = "Range",
                resizable = true,
                width = 75,
                makeCell = () => new VisualElement(),
                bindCell = (el, i) =>
                {
                    el.Clear();
                    Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;

                    IntegerField field = new();
                    if (building != null)
                    {
                        el.Add(field);
                        if (building is Pub pub)
                        {
                            pub.Range ??= new(-1);
                            field.value = pub.Range.BaseValue;
                            field.RegisterValueChangedCallback(RangeChange);
                        }
                    }
                },
                unbindCell = (el, i) =>
                {
                    try
                    {
                        (el[0] as IntegerField).UnregisterValueChangedCallback(RangeChange);
                        el.Clear();
                    }
                    catch { }
                },
            });
            #endregion
        }


        #region Base
        void AssetChange(ChangeEvent<Object> ev)
        {
            int i = ev.target.GetRowIndex();
            if (changedType || ((BuildingData)holder).ContainsBuilding((Building)ev.newValue) == false)
            {
                Building b = ev.newValue as Building;
                if (b != null && !changedType)
                {
                    string path = $"{BUILDING_PATH}{selectedCategory.Name}";
                    string _name = b.objectName.Length > 0 ? b.objectName : UnityEngine.Random.Range(0, int.MaxValue).ToString();
                    if (!Directory.Exists(path + "/" + _name))
                    {
                        string GUID = AssetDatabase.CreateFolder(path, $"{_name}");
                        if ((path = AssetDatabase.GUIDToAssetPath(GUID)) != "")
                        {
                            string oldPath = AssetDatabase.GetAssetPath(ev.newValue);
                            AssetDatabase.MoveAsset(oldPath, $"{path}{BUILD_NAME}");
                            ((BuildingWrapper)dataGrid.itemsSource[i]).preview = GetPrefabPreview(path);

                            if (oldPath.Contains("/BCK/"))
                                AssetDatabase.DeleteAsset(Path.GetDirectoryName(oldPath));
                            AssetDatabase.Refresh();

                            AddressableAssetEntry entry = settings.CreateOrMoveEntry(GUID, group);
                            entry.SetAddress(_name);
                            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, group, true);
                        }
                        else
                            Debug.LogError($"Cannot create {BUILDING_PATH}{selectedCategory.Name}!");
                    }
                    else
                    {
                        Debug.LogError("Already exists!\n" + path + "/" + _name);
                    }
                }
                else if (b == null)
                {
                    RemoveEntry(dataGrid.itemsSource[i] as BuildingWrapper, false);
                }

                //AssetDatabase.create


                ((BuildingWrapper)dataGrid.itemsSource[i]).SetBuilding(
                    b,
                    (byte)holder.Categories.FindIndex(q => q.Name == selectedCategory.Name));
                changedType = false;
                dataGrid.RefreshItem(i);
                EditorUtility.SetDirty((BuildingData)holder);
            }
            else
            {
                ((ObjectField)ev.target).SetValueWithoutNotify(((BuildingWrapper)dataGrid.itemsSource[i]).building);
            }
        }

        Sprite GetPrefabPreview(string folderPath)
        {
            Debug.Log("Generate preview for " + folderPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folderPath}{BUILD_NAME}");
            var editor = UnityEditor.Editor.CreateEditor(prefab);
            Texture2D tex = editor.RenderStaticPreview($"{folderPath}{BUILD_NAME}", null, 200, 200);

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

            File.WriteAllBytes($"{folderPath}{TEX_NAME}", b);
            AssetDatabase.Refresh();

            TextureImporter importer = TextureImporter.GetAtPath($"{folderPath}{TEX_NAME}") as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.spriteImportMode = SpriteImportMode.Single;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            DestroyImmediate(editor);
            DestroyImmediate(tex);
            return AssetDatabase.LoadAssetAtPath<Sprite>(importer.assetPath);
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
                    = group.GetAssetEntry(
                        AssetDatabase.GUIDFromAssetPath(oldPath).ToString());

                string result = AssetDatabase.MoveAsset(
                        $"{oldPath}",
                        $"{BUILDING_PATH}{selectedCategory.Name}/{value}");
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
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, value, true);
                ((BuildingWrapper)dataGrid.itemsSource[i]).building.objectName = value;
                EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).building);
            }
        }

        void TypeChange(ChangeEvent<string> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
            if (prev != null)
            {
                Type t = buildingTypes.FirstOrDefault(q => q.Name == ev.newValue);
                if (t != null && prev.GetType() != t)
                {
                    Building building = (Building)
                        ((BuildingWrapper)dataGrid.itemsSource[i]).building.gameObject
                        .AddComponent(t);

                    building.Clone(prev);
                    DestroyImmediate(prev, true);
                    EditorUtility.SetDirty(building.gameObject);
                    ((BuildingWrapper)dataGrid.itemsSource[i]).SetBuilding(building, ((byte)holder.Categories.FindIndex(q => q.Name == selectedCategory.Name)));
                    changedType = true;
                    dataGrid.RefreshItem(i);
                }
            }
        }

        void BlueprintEvent(ClickEvent ev)
        {
            int i = ev.target.GetRowIndex();
            BuildEditor.ShowWindow(((BuildingWrapper)dataGrid.itemsSource[i]).building);
        }

        void PreviewClick(ClickEvent ev)
        {
            if (ev.clickCount == 2)
            {
                int i = ev.target.GetRowIndex();
                BuildingWrapper wrapper = dataGrid.itemsSource[i] as BuildingWrapper;
                if (wrapper.building)
                {
                    wrapper.preview =
                        GetPrefabPreview(Path.GetDirectoryName(AssetDatabase.GetAssetPath(wrapper.building)));
                    EditorUtility.SetDirty(holder);
                    dataGrid.RefreshItem(i);
                }
            }
        }

        void CategoryChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
            if (prev != null)
            {
                int categ = prev.BuildingCateg;
                if (categ != ev.newValue)
                {
                    prev.BuildingCateg = ev.newValue;
                }
            }
        }
        void StorageCapacityChanged(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            if (((BuildingWrapper)dataGrid.itemsSource[i]).building != null)
            {
                ((BuildingWrapper)dataGrid.itemsSource[i]).building.LocalRes.capacity.BaseValue = ev.newValue;
                EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).building);
            }
        }

        void FluidCapacityChanged(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
            if (building != null && building is IFluidWork fluidRes)
            {
                fluidRes.StoredFluids.capacity.BaseValue = ev.newValue;
                EditorUtility.SetDirty(building);
            }
        }
        #endregion

        #region Special
        void AssignChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            ((IAssign)((BuildingWrapper)dataGrid.itemsSource[i]).building).AssignLimit.BaseValue = ev.newValue;
            EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).building);
        }

        void ProdTimeChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            ((IProduction)((BuildingWrapper)dataGrid.itemsSource[i]).building).ProdTime = ev.newValue;
            EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).building);
        }


        void CanStoreFluidsChange(ChangeEvent<ulong> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
            if (prev != null)
            {
                if (prev is FluidTank tank)
                {
                    if (tank.TypesToStore != ev.newValue)
                        tank.TypesToStore = ev.newValue;
                }
                else if (prev is IStorage storage)
                {
                    if (storage.CanStoreMask != ev.newValue)
                        storage.CanStoreMask = ev.newValue;
                }
                else
                    return;
            }
            EditorUtility.SetDirty(prev);
        }

        void RangeChange(ChangeEvent<int> ev)
        {
            int i = ev.target.GetRowIndex();
            Building prev = ((BuildingWrapper)dataGrid.itemsSource[i]).building;
            if (prev != null)
            {
                if (prev is Pub pub)
                {
                    pub.Range = new(ev.newValue);
                }
            }
            EditorUtility.SetDirty(prev);
        }


        #endregion

        #endregion
    }
}