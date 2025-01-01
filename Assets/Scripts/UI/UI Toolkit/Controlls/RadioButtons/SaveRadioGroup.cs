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

    public class SaveRadioGroup : CustomRadioButtonGroup
    {
        public Action<int> deleteAction;
        #region List
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index); 
            element.RemoveFromClassList("unity-text-element");
            element.RemoveFromClassList("unity-button");
            (element as SaveRadioButton).text = _itemsSource[index].data;
            (element as SaveRadioButton).saveDate.text = (_itemsSource[index] as SaveRadioButton).saveDate.text;
            (element as SaveRadioButton).style.marginTop = 10;

            // clears styles if scrolling and selected
            if (selectedId == index)
                (element as SaveRadioButton).Select(false);
            else if (element.ClassListContains("save-radio-button-selected"))
                (element as SaveRadioButton).Deselect(false);

            (element as SaveRadioButton).RegisterCallback<ClickEvent>((_) => (element as SaveRadioButton).Select());
        }
        protected override CustomRadioButton DefaultMakeItem()
        {
            return new SaveRadioButton("string", "save-radio-button", -1, new DateTime());
        }
        #endregion

        [Obsolete]
        public new class UxmlFactory : UxmlFactory<SaveRadioGroup, UxmlTraits> { }
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
            deleteAction(selectedId);
        }

        public Folder[] FillItemSource(string path, bool write, bool parentLevel)
        {
            try
            {
                selectedId = -1;
                _itemsSource.RemoveAll(q => true);
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
                this.Q<VisualElement>("unity-content-container").style.height = (folders.Length*113) + 30;
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