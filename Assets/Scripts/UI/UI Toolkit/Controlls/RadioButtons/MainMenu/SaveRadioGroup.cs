using System;
using UnityEngine.UIElements;

using AbstractControls;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RadioGroups
{
    public struct Folder
    {
        public string path;
        public DateTime date;
        public override string ToString()
        {
            return SaveController.GetSaveName(path);
        }
    }

    [UxmlElement]
    public partial class SaveRadioGroup : CustomRadioButtonGroup
    {
        public Action<int> deleteAction;
        #region List
        public SaveRadioGroup() : base()
        {
            Debug.Log(itemsSource);
        }
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index); 
            element.RemoveFromClassList("unity-text-element");
            element.RemoveFromClassList("unity-button");
            SaveRadioButton saveRadioButton = (element as SaveRadioButton);
            saveRadioButton.text = _itemsSource[index].data;
            saveRadioButton.saveDate.text = (_itemsSource[index] as SaveRadioButton).saveDate.text;
            saveRadioButton.style.marginTop = 10;

            // clears styles if scrolling and selected
            if (SelectedId == index)
                saveRadioButton.Select(false);
            else if (element.ClassListContains("save-radio-button-selected"))
                saveRadioButton.Deselect(false);

            //saveRadioButton.RegisterCallback<ClickEvent>((_) => saveRadioButton.Select());
        }
        protected override CustomRadioButton DefaultMakeItem()
        {
            return new SaveRadioButton("string", "save-radio-button", -1, new DateTime());
        }
        #endregion

        public override void Init(Action<int> onChange)
        {
            base.Init(onChange);
            VisualElement el = this.Q<VisualElement>("unity-slider");
            el.style.marginTop = 0;
            el.style.marginBottom = 0;
        }

        #region Saves
        public void DeleteSave()
        {
            deleteAction(SelectedId);
        }

        public Folder[] FillItemSource(string path, bool write, bool parentLevel)
        {
            try
            {
                SelectedId = -1;
                ClearItems();
                Folder[] folders;

                if (parentLevel)
                {
                    string[] paths = Directory.GetDirectories(path);
                    folders = new Folder[paths.Length];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        try
                        {
                            folders[i].date = SortSavesByDate(paths[i]).First().date;
                            folders[i].path = paths[i];
                        }
                        catch
                        {
                            Debug.LogWarning($"wrong folder format in {paths[i]}");
                        }
                    }
                    folders = folders.Where(q => q.path != null).OrderByDescending(q => q.date).ToArray();
                }
                else
                {
                    folders = SortSavesByDate(path);
                }

                if(write)
                    for (int i = 0; i < folders.Length; i++)
                        AddItem(new SaveRadioButton(SaveController.GetSaveName(folders[i].path), "save-radio-button", i, folders[i].date));
                schedule.Execute(() => this.Q<VisualElement>("unity-content-container").style.height = (folders.Length * 113) + 30).ExecuteLater(5);
                
                return folders;
            }
            catch
            {
                return null;
            }
        }

        Folder[] SortSavesByDate(string path)
        {
            string[] paths = Directory.GetDirectories(path);
            Folder[] folders = new Folder[paths.Length];

            for (int j = 0; j < paths.Length; j++)
            {
                string s;
                try
                {
                    s = Directory.GetFiles(paths[j]).FirstOrDefault();
                    folders[j].date = File.GetLastWriteTime(s);
                    folders[j].path = paths[j];
                }
                catch
                {
                    Debug.LogWarning($"wrong save format in {paths[j]}");
                }
            }
            return folders.Where(q => q.path != null).OrderByDescending(q => q.date).ToArray();
        }
        #endregion
    }
}