using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AssetImporters;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace EditorWindows.Windows
{
	public class BuildingRegister : CategoryWindow<BuildCategWrapper, BuildingWrapper>
	{
		const string BUILDING_PATH = "Assets/Game Data/Buildings/";
		const string BUILD_NAME = "/building.prefab";
		const string TEX_NAME = "/texture.png";
		MultiColumnListView dataGrid;

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
			data = AssetDatabase.LoadAssetAtPath<BuildButtonHolder>("Assets/Game Data/Research && Building/Build Data.asset");

			#region Grid
			base.CreateGUI();
			dataGrid = rootVisualElement.Q<MultiColumnListView>("Data");
			dataGrid.onAdd = AddEntry;
			dataGrid.onRemove = RemoveEntry;
			CreateColumns();
			#endregion
			categorySelector.index = 0;
		}
		protected override void RenameCateg()
		{
			AssetDatabase.MoveAsset($"{BUILDING_PATH}{selectedCategory.Name}", $"{BUILDING_PATH}{categoryNameField.text}");
			if(group != null)
			{
				group.Name = categoryNameField.text;
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRenamed, group, true, false);
			}
			base.RenameCateg();
		}
		protected override void CreateCateg()
		{
			base.CreateCateg();
			AssetDatabase.CreateFolder($"Assets/Game Data/Buildings", selectedCategory.Name);
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
				((BuildCategWrapper)selectedCategory).columnStates = new();
				for (int i = 0; i < dataGrid.columns.Count; i++)
				{
					((BuildCategWrapper)selectedCategory).columnStates.Add(dataGrid.columns[i].visible);
				}
				EditorUtility.SetDirty((BuildButtonHolder)data);
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
				dataGrid.style.display = DisplayStyle.Flex;

				dataGrid.itemsSource = selectedCategory.Objects;
				for (int i = 0; i < ((BuildCategWrapper)selectedCategory).columnStates?.Count; i++)
					dataGrid.columns[i].visible = ((BuildCategWrapper)selectedCategory).columnStates[i];
			}
			else
			{
				selectedCategory = new BuildCategWrapper();
				dataGrid.style.display = DisplayStyle.None;
			}
			return boo;
		}

		#endregion
		#endregion

		#region
		void AddEntry(BaseListView _)
		{
			BuildingWrapper wrapper = new(selectedCategory.UniqueID());
			int choice = EditorUtility.DisplayDialogComplex("Register a new building",
				"Do you want to fill the new entry or create an empty one?",
				"Filled", "Cancel", "Empty");
			if (choice == 0)
			{
				string s = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder($"{BUILDING_PATH}{selectedCategory.Name}", "Dummy"));

				GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				gameObj.AddComponent<Building>();
				PrefabUtility.SaveAsPrefabAsset(gameObj, $"{s}{BUILD_NAME}");
				wrapper.b = AssetDatabase.LoadAssetAtPath<GameObject>($"{s}{BUILD_NAME}").GetComponent<Building>();
				wrapper.b.objectName = s.Split('/')[^1];
				wrapper.preview = GetPrefabPreview($"{s}");
				DestroyImmediate(gameObj);

				AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.GUIDFromAssetPath(s).ToString(), group);
				entry.SetAddress(wrapper.b.objectName);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, group, true);
			}
			else if (choice == 1)
				return;
			selectedCategory.Objects.Add(wrapper);
			dataGrid.RefreshItems();
			EditorUtility.SetDirty((BuildButtonHolder)data);
		}

		void RemoveEntry(BaseListView _) => RemoveEntry(_.selectedItem as BuildingWrapper, true);
		void RemoveEntry(BuildingWrapper wrapper, bool removeFromGrid)
		{
			if (removeFromGrid)
			{
				selectedCategory.Objects.Remove(wrapper);
				dataGrid.RefreshItems();
			}
			if (wrapper.b)
			{
				AssetDatabase.MoveAsset($"{BUILDING_PATH}{selectedCategory.Name}/{wrapper.b?.objectName}", $"{BUILDING_PATH}BCK/{wrapper.b?.objectName}");
				settings.RemoveAssetEntry(AssetDatabase.GUIDFromAssetPath($"{BUILDING_PATH}BCK/{wrapper.b?.objectName}").ToString(), group);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, group, true);
			}
		}
		#endregion

		#region Columns
		int GetRowIndex(VisualElement element)
		{
			while (element.name != "unity-multi-column-view__row-container")
			{
				element = element.parent;
			}
			return element.parent.IndexOf(element);
		}

		private void CreateColumns()
		{
			#region Base
			#region ID
			dataGrid.columns["id"].makeCell =
				() =>
				{
					Label l = new Label();
					l.AddToClassList("cell");
					return l;
				};
			dataGrid.columns["id"].bindCell =
				(el, i) => ((Label)el).text = selectedCategory.Objects[i].id.ToString();
			#endregion

			#region Asset
			dataGrid.columns["asset"].makeCell =
				() => new ObjectField();
			dataGrid.columns["asset"].bindCell =
				(el, i) =>
				{
					ObjectField field = (ObjectField)el;
					field.allowSceneObjects = false;
					field.objectType = typeof(Building);
					field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).b;
					field.RegisterValueChangedCallback(AssetChange);
				};
			dataGrid.columns["asset"].unbindCell =
				(el, i) =>
				{
					ObjectField field = (ObjectField)el;
					field.UnregisterValueChangedCallback(AssetChange);
				};
			#endregion

			#region Name
			dataGrid.columns["name"].makeCell =
				() => new TextField();
			dataGrid.columns["name"].bindCell =
				(el, i) =>
				{
					TextField field = (TextField)el;
					if (((BuildingWrapper)dataGrid.itemsSource[i]).b != null)
					{
						field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName.ToString();
						field.RegisterCallback<FocusOutEvent>(NameChange);
					}
					else
					{
						field.value = "";
					}
				};
			dataGrid.columns["name"].unbindCell =
				(el, i) =>
				{
					TextField field = (TextField)el;
					field.UnregisterCallback<FocusOutEvent>(NameChange);
				};
			#endregion

			#region Type
			dataGrid.columns["type"].makeCell =
				() => new DropdownField();
			dataGrid.columns["type"].bindCell =
				(el, i) =>
				{
					DropdownField field = (DropdownField)el;
					field.choices = buildingTypes.Select(q => q.Name).Where(q => !q.Contains("Pipe")).ToList();
					field.value = ((BuildingWrapper)dataGrid.itemsSource[i]).b
						? ((BuildingWrapper)dataGrid.itemsSource[i]).b.GetType().ToString()
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
					cell.Open(((BuildingWrapper)dataGrid.itemsSource[i]).b?.cost, ((BuildingWrapper)dataGrid.itemsSource[i]).b, true);
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
					Building building = ((BuildingWrapper)dataGrid.itemsSource[i]).b;
					if (building)
					{
						if (building.blueprint.itemList == null ||
							building.blueprint.itemList.Count == 0 ||
							building.blueprint.itemList.Count(q => q.itemType == GridItemType.Anchor) == 0 ||
							building.blueprint.itemList.Count(q => q.itemType == GridItemType.Entrance) == 0)
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
					if (((BuildingWrapper)dataGrid.itemsSource[i]).b is IAssign)
					{
						field.value = ((IAssign)((BuildingWrapper)dataGrid.itemsSource[i]).b).AssignLimit;
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
					if (((BuildingWrapper)dataGrid.itemsSource[i]).b is IProduction)
					{
						field.SetEnabled(true);
						field.value = Convert.ToInt32(((IProduction)((BuildingWrapper)dataGrid.itemsSource[i]).b).ProdTime);
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

				makeCell = () => new ResourceCell(),
				bindCell = (el, i) =>
				{
					el.parent.focusable = true;
					ResourceCell cell = el.Q<ResourceCell>();
					Building b = ((BuildingWrapper)dataGrid.itemsSource[i]).b;
					if (b is IResourceProduction)
						cell.Open(
							((IResourceProduction)b)?.ProductionCost,
							((BuildingWrapper)dataGrid.itemsSource[i]).b, false);
					else
						cell.Open(null, null, false);
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

				makeCell = () => new ResourceCell(),
				bindCell = (el, i) =>
				{
					el.parent.focusable = true;
					ResourceCell cell = el.Q<ResourceCell>();
					Building b = ((BuildingWrapper)dataGrid.itemsSource[i]).b;
					if (b is IResourceProduction)
						cell.Open(
							((IResourceProduction)b)?.ProductionYield,
							((BuildingWrapper)dataGrid.itemsSource[i]).b, false);
					else
						cell.Open(null, null, false);
				}
			});
			#endregion

			#endregion

			
		}

		#region Base
		void AssetChange(ChangeEvent<Object> ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			if (changedType || ((BuildButtonHolder)data).ContainsBuilding((Building)ev.newValue) == false)
			{
				Building b = ev.newValue as Building;
				if (b != null && !changedType)
				{
					string path = $"{BUILDING_PATH}{selectedCategory.Name}";
					if (!Directory.Exists(path + "/" + b.objectName))
					{
						string GUID = AssetDatabase.CreateFolder(path, $"{b.objectName}");
						if ((path = AssetDatabase.GUIDToAssetPath(GUID)) != "")
						{
							string oldPath = AssetDatabase.GetAssetPath(ev.newValue);
							AssetDatabase.MoveAsset(oldPath, $"{path}{BUILD_NAME}");
							((BuildingWrapper)dataGrid.itemsSource[i]).preview = GetPrefabPreview(path);
							AssetDatabase.Refresh();

							AddressableAssetEntry entry = settings.CreateOrMoveEntry(GUID, group);
							entry.SetAddress(b.objectName);
							settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, group, true);
						}
						else
							Debug.LogError($"Cannot create {BUILDING_PATH}{selectedCategory.Name}!");
					}
					else
					{
						Debug.LogError("Already exists!");
					}
				}
				else if (b == null)
				{
					RemoveEntry(dataGrid.itemsSource[i] as BuildingWrapper, false);
				}

				//AssetDatabase.create


				((BuildingWrapper)dataGrid.itemsSource[i]).b = b;
				changedType = false;
				dataGrid.RefreshItem(i);
				EditorUtility.SetDirty((BuildButtonHolder)data);
			}
			else
			{
				((ObjectField)ev.target).SetValueWithoutNotify(((BuildingWrapper)dataGrid.itemsSource[i]).b);
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
			tex = new(200,200, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
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


		void NameChange(FocusOutEvent ev)
		{
			string value;
			if(ev.target is TextElement)
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

				int i = GetRowIndex((VisualElement)ev.target);
			if (((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName != value)
			{
				string result = AssetDatabase.MoveAsset(
						$"{BUILDING_PATH}{selectedCategory.Name}/{((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName}",
						$"{BUILDING_PATH}{selectedCategory.Name}/{value}");
				if (result != "")
				{
					Debug.LogError(result);
					if (ev.target is TextElement)
					{
						((TextElement)ev.target).text = ((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName;
					}
					else
					{
						((TextField)ev.target).value = ((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName;
					}
					return;
				}
				AddressableAssetEntry entry = group.GetAssetEntry(AssetDatabase.GUIDFromAssetPath($"{BUILDING_PATH}{selectedCategory.Name}/{value}").ToString());
				entry.address = value;
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, value, true);
				((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName = value;
				EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).b);


			}
		}

		void TypeChange(ChangeEvent<string> ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			Building prev = ((BuildingWrapper)dataGrid.itemsSource[i]).b;
			if (prev != null)
			{
				Type t = buildingTypes.FirstOrDefault(q => q.Name == ev.newValue);
				if (t != null && prev.GetType() != t)
				{
					Building building = (Building)
						((BuildingWrapper)dataGrid.itemsSource[i]).b.gameObject
						.AddComponent(t);

					building.Clone(prev);
					DestroyImmediate(prev, true);
					EditorUtility.SetDirty(building.gameObject);
					((BuildingWrapper)dataGrid.itemsSource[i]).b = building;
					changedType = true;
					dataGrid.RefreshItem(i);
				}
			}
		}

		void BlueprintEvent(ClickEvent ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			BuildEditor.ShowWindow(((BuildingWrapper)dataGrid.itemsSource[i]).b);
		}

		void PreviewClick(ClickEvent ev)
		{
			if (ev.clickCount == 2)
			{
				int i = GetRowIndex((VisualElement)ev.target);
				BuildingWrapper wrapper = dataGrid.itemsSource[i] as BuildingWrapper;
				if (wrapper.b)
				{
					wrapper.preview = 
						GetPrefabPreview(Path.GetDirectoryName(AssetDatabase.GetAssetPath(wrapper.b)));
					EditorUtility.SetDirty(data);
					dataGrid.RefreshItem(i);
				}
			}
		}
		#endregion

		#region Special
		void AssignChange(ChangeEvent<int> ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			((IAssign)((BuildingWrapper)dataGrid.itemsSource[i]).b).AssignLimit = ev.newValue;
			EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).b);
		}

		void ProdTimeChange(ChangeEvent<int> ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			((IProduction)((BuildingWrapper)dataGrid.itemsSource[i]).b).ProdTime = ev.newValue;
			EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).b);
		}


		#endregion

		#endregion
	}
}