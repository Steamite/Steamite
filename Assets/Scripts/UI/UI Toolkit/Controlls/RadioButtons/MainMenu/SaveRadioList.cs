using System;
using System.IO;
using System.Linq;
using AbstractControls;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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


    public class RadioSaveButtonData : RadioButtonData
    {
        public Folder folder;
        public RadioSaveButtonData(string _text, Folder _folder) : base(_text)
        {
            folder = _folder;
        }
    }


    [UxmlElement]
    public partial class SaveRadioList : ScrollableRadioList
    {
        public Action<int> deleteAction;
        #region List
        public SaveRadioList() : base()
        {
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        }
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            element.RemoveFromClassList("unity-text-element");
            element.RemoveFromClassList("unity-button");
            SaveRadioButton saveRadioButton = (element as SaveRadioButton);
            saveRadioButton.text = (itemsSource[index] as RadioButtonData).text;
            saveRadioButton.saveDate.text = (itemsSource[index] as RadioSaveButtonData).folder.date.ToString();
            saveRadioButton.style.marginTop = 10;
            saveRadioButton.style.height = 94;

            // clears styles if scrolling and selected
            if (SelectedChoice == index)
                saveRadioButton.SelectWithoutTransition(false);
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
            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            fixedItemHeight = 110;
            contentContainer.style.overflow = Overflow.Visible;
        }

        #region Saves
        public void DeleteSave()
        {
            deleteAction(SelectedChoice);
            contentContainer.SendEvent(GeometryChangedEvent.GetPooled());
        }

        public Folder[] FillItemSource(string path, bool write, bool parentLevel)
        {
            try
            {
                SelectedChoice = -1;
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
                itemsSource = folders.Select(q =>
                    new RadioSaveButtonData(
                        Path.GetFileNameWithoutExtension(q.path), 
                        q)
                        as RadioButtonData).ToList();
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