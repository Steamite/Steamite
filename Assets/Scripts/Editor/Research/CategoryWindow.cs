using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EditorWindows
{
	public class CategoryWindow<T, TW> : EditorWindow where T : DataCategory<TW>
	{
		public DataCategory<TW> selectedCategory;
		protected DataHolder<T> data;
		[SerializeField] VisualTreeAsset windowAsset;

		protected DropdownField categorySelector;
		TextField textField;
		Button createCategory;
		Button categoryRemover;


		protected virtual void CreateGUI()
		{
			VisualElement doc = windowAsset.CloneTree();
			rootVisualElement.Add(doc);

			#region Category
			categoryRemover = rootVisualElement.Q<Button>("Category-Remover");
			categoryRemover.clicked += RemoveCateg;

			categorySelector = doc.Q<DropdownField>("Category-Selctor");
			categorySelector.choices = data.Choices();
			categorySelector.choices.Add("Create new");
			#endregion


			categorySelector.RegisterValueChangedCallback(
				(_) => LoadCategData(categorySelector.index));
		}

		#region Category Switching
		protected virtual bool LoadCategData(int index)
		{
			VisualElement iconElement;
			ObjectField iconSelector;
			TopBar(out iconElement, out iconSelector);

			bool categoryExists;
			if (index < data.Categories.Count)
			{
				categoryExists = true;
				selectedCategory = data.Categories[index];

				categoryRemover.SetEnabled(true);
				createCategory.text = "Rename";
				createCategory.clicked -= CreateCateg;
				createCategory.clicked += RenameCateg;

				iconElement.style.backgroundImage = selectedCategory.Icon;
				iconSelector.value = selectedCategory.Icon;
				textField.value = selectedCategory.Name;
			}
			else
			{
				categoryExists = false;
				selectedCategory = null;

				categoryRemover.SetEnabled(false);
				createCategory.text = "Create new category";
				createCategory.clicked += CreateCateg;
				createCategory.clicked -= RenameCateg;

				iconElement.style.backgroundImage = null;
				iconSelector.value = null;
				textField.value = "";
			}

			return categoryExists;
		}

		protected virtual void TopBar(out VisualElement iconElement, out ObjectField iconSelector)
		{
			createCategory = rootVisualElement.Q<Button>("Category-Create");
			createCategory.SetEnabled(false);

			iconElement = rootVisualElement.Q<VisualElement>("Icon-Image");
			var iconElem = iconElement;
			iconSelector = rootVisualElement.Q<ObjectField>("Icon-Changer");
			iconSelector.RegisterValueChangedCallback(
				(ev) =>
				{
					selectedCategory.Icon = (Texture2D)ev.newValue;
					iconElem.style.backgroundImage = selectedCategory.Icon;
					EditorUtility.SetDirty(data);
				});

			textField = rootVisualElement.Q<TextField>("Category-Name");
			textField.RegisterValueChangedCallback(
				(ev) =>
				{
					createCategory.SetEnabled(
						ev.newValue.Length > 0 &&
						selectedCategory.Name != ev.newValue &&
						data.Categories.Count(q => (q as DataCategory<TW>).Name == ev.newValue) == 0);
				});
		}

		#region Categ Buttons
		void RenameCateg()
		{
			createCategory.SetEnabled(false);
			selectedCategory.Name = textField.value;
			categorySelector.choices[categorySelector.index] = textField.value;
			categorySelector.SetValueWithoutNotify(textField.value);
			EditorUtility.SetDirty(data);
		}

		void CreateCateg()
		{
			createCategory.SetEnabled(false);
			selectedCategory.Name = textField.value;
			selectedCategory.Objects = new();
			data.Categories.Add((T)selectedCategory);
			categorySelector.choices.Insert(data.Categories.Count - 1, selectedCategory.Name);
			categorySelector.value = selectedCategory.Name;
			categorySelector.MarkDirtyRepaint();
			EditorUtility.SetDirty(data);
		}

		void RemoveCateg()
		{
			if (categoryRemover.enabledSelf && EditorUtility.DisplayDialog(
				"Delete category",
				"Are you sure you want to delete this category? All data will be lost.",
				"Confirm", "Cancel"))
			{
				data.Categories.RemoveAt(categorySelector.index);
				categorySelector.choices.RemoveAt(categorySelector.index);
				categorySelector.index = categorySelector.index - 1 > -1 ? categorySelector.index - 1 : 0;
				EditorUtility.SetDirty(data);
			}
		}
		#endregion


		#endregion
	}
}