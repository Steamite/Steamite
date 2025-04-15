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
		protected TextField categoryNameField;
		Button createCategory;
		Button categoryRemover;
		VisualElement iconElement;

		bool refresh;
		protected virtual void CreateGUI()
		{
			VisualElement doc = windowAsset.CloneTree();
			rootVisualElement.Add(doc);

			#region Category
			categoryRemover = rootVisualElement.Q<Button>("Category-Remover");
			categoryRemover.clicked += () => RemoveCateg();

			categorySelector = doc.Q<DropdownField>("Category-Selctor");
			categorySelector.choices = data.Choices();
			categorySelector.choices.Add("Create new");
			#endregion


			categorySelector.RegisterValueChangedCallback(
				(_) => LoadCategData(categorySelector.index));
		}


		private void OnFocus()
		{
			if(categorySelector != null && categorySelector.index < data.Categories.Count)
				LoadCategData(categorySelector.index);
		}

		#region Category Switching
		protected virtual bool LoadCategData(int index)
		{
			ObjectField iconSelector;
			TopBar(out iconSelector);

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
				categoryNameField.value = selectedCategory.Name;
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
				categoryNameField.value = "";
			}

			return categoryExists;
		}

		protected virtual void TopBar(out ObjectField iconSelector)
		{
			createCategory = rootVisualElement.Q<Button>("Category-Create");
			createCategory.SetEnabled(false);

			iconElement = rootVisualElement.Q<VisualElement>("Icon-Image");
			iconSelector = rootVisualElement.Q<ObjectField>("Icon-Changer");
			iconSelector.UnregisterValueChangedCallback<Object>(IconChange);
			iconSelector.RegisterValueChangedCallback<Object>(IconChange);

			categoryNameField = rootVisualElement.Q<TextField>("Category-Name");
			categoryNameField.UnregisterValueChangedCallback<string>(NameChange);
			categoryNameField.RegisterValueChangedCallback<string>(NameChange);


		}

		void IconChange(ChangeEvent<Object> ev)
		{
			if (selectedCategory != null && selectedCategory.Icon != (Texture2D)ev.newValue)
			{
				refresh = false;
				selectedCategory.Icon = (Texture2D)ev.newValue;
				iconElement.style.backgroundImage = selectedCategory.Icon;
				EditorUtility.SetDirty(data);
			}
		}

		void NameChange(ChangeEvent<string> ev)
		{
			createCategory.SetEnabled(
				ev.newValue.Length > 0 &&
				selectedCategory.Name != ev.newValue &&
				data.Categories.Count(q => (q as DataCategory<TW>).Name == ev.newValue) == 0);
		}

		#region Categ Buttons
		protected virtual void RenameCateg()
		{
			createCategory.SetEnabled(false);
			selectedCategory.Name = categoryNameField.value;
			categorySelector.choices[categorySelector.index] = categoryNameField.value;
			categorySelector.SetValueWithoutNotify(categoryNameField.value);
			EditorUtility.SetDirty(data);
		}

		protected virtual void CreateCateg()
		{
			createCategory.SetEnabled(false);
			selectedCategory.Name = categoryNameField.value;
			selectedCategory.Objects = new();
			data.Categories.Add((T)selectedCategory);
			categorySelector.choices.Insert(data.Categories.Count - 1, selectedCategory.Name);
			categorySelector.value = selectedCategory.Name;
			categorySelector.MarkDirtyRepaint();
			EditorUtility.SetDirty(data);
		}

		protected virtual bool RemoveCateg()
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
				return true;
			}
			return false;
		}
		#endregion


		#endregion
	}
}