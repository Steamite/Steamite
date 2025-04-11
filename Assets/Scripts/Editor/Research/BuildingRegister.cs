using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EditorWindows.Windows
{
	public class BuildingRegister : CategoryWindow<BuildCategWrapper, BuildingWrapper>
	{
		MultiColumnListView dataGrid;

		List<Type> buildingTypes;

		bool changedType;

		[MenuItem("Custom Editors/Building register %g", priority = 14)]
		public static void Open()
		{
			BuildingRegister wnd = GetWindow<BuildingRegister>();
			wnd.titleContent = new("Building Register");
			wnd.minSize = new(800, 400);
		}

		protected override void CreateGUI()
		{
			buildingTypes = TypeCache.GetTypesDerivedFrom(typeof(Building)).ToList();
			data = AssetDatabase.LoadAssetAtPath<BuildButtonHolder>("Assets/Game Data/Research && Building/Build Data.asset");

			#region Grid
			base.CreateGUI();
			dataGrid = rootVisualElement.Q<MultiColumnListView>("Data");
			dataGrid.onAdd =
				(_) =>
				{
					BuildingWrapper wrapper = new(selectedCategory.UniqueID());
					if (EditorUtility.DisplayDialog("Register a new building",
						"Do you want to fill the new entry or create an empty one?",
						"Filled", "Empty"))
					{
						GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
						gameObj.AddComponent<Building>();
						string s = AssetDatabase.GenerateUniqueAssetPath("Assets/Prefabs/GameObjects/Buildings/building.prefab");
						PrefabUtility.SaveAsPrefabAsset(gameObj, s);
						wrapper.b = AssetDatabase.LoadAssetAtPath<GameObject>(s).GetComponent<Building>();
						DestroyImmediate(gameObj);
					}
					selectedCategory.Objects.Add(wrapper);
					dataGrid.RefreshItems();
					EditorUtility.SetDirty((BuildButtonHolder)data);
				};
			dataGrid.onRemove =
				(_) =>
				{
					selectedCategory.Objects.Remove((BuildingWrapper)_.selectedItem);
					dataGrid.RefreshItems();
				};
			CreateColumns();
			#endregion
			categorySelector.index = 0;
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
			bool boo = base.LoadCategData(index);
			if (boo)
			{
				dataGrid.style.display = DisplayStyle.Flex;

				dataGrid.itemsSource = selectedCategory.Objects;
				for (int i = 0; i < ((BuildCategWrapper)selectedCategory).columnStates?.Count; i++)
					dataGrid.columns[i].visible = ((BuildCategWrapper)selectedCategory).columnStates[i];
			}
			else
			{
				selectedCategory = new BuildCategWrapper();
			}
			return boo;
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
						field.RegisterValueChangedCallback(NameChange);
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
					field.UnregisterValueChangedCallback(NameChange);
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
				changedType = false;
				((BuildingWrapper)dataGrid.itemsSource[i]).b = (Building)ev.newValue;
				dataGrid.RefreshItem(i);
				EditorUtility.SetDirty((BuildButtonHolder)data);
			}
			else
			{
				((ObjectField)ev.target).SetValueWithoutNotify(((BuildingWrapper)dataGrid.itemsSource[i]).b);
			}
		}

		void NameChange(ChangeEvent<string> ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			if (((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName != ev.newValue)
			{
				((BuildingWrapper)dataGrid.itemsSource[i]).b.objectName = ev.newValue;
				EditorUtility.SetDirty(((BuildingWrapper)dataGrid.itemsSource[i]).b);
			}
		}

		void TypeChange(ChangeEvent<string> ev)
		{
			int i = GetRowIndex((VisualElement)ev.target);
			Building prev = ((BuildingWrapper)dataGrid.itemsSource[i]).b;
			if (prev != null)
			{
				Type t = buildingTypes.First(q => q.Name == ev.newValue);
				if (prev.GetType() != t)
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