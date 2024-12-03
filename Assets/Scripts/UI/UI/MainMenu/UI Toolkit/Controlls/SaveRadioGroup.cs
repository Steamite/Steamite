using System;
using UnityEngine.UIElements;

using AbstractControls;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.WSA;
using UnityEngine;

namespace RadioGroups
{
    public struct Folder
    {
        public string path;
        public DateTime date;
    }

    public class SaveRadioGroup : CustomRadioButtonGroup
    {
        [Obsolete]
        public new class UxmlFactory : UxmlFactory<SaveRadioGroup, UxmlTraits> { }
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            (element as CustomRadioButton).text = _itemsSource[index].data;
            (element as CustomRadioButton).style.fontSize = new(new Length(40, LengthUnit.Percent));
            (element as CustomRadioButton).style.height = new(new Length(98.3f, LengthUnit.Pixel));
            (element as CustomRadioButton).RegisterCallback<ClickEvent>((element as CustomRadioButton).Select);
        }
        protected override CustomRadioButton DefaultMakeItem()
        {
            return new("string", "save-radio-button", -1);
        }
        public override void Init(Action<int> onChange)
        {
            base.Init(onChange);
        }

        public override void Select(CustomRadioButton customRadioButton)
        {
            base.Select(customRadioButton);
        }

        public Folder[] FillItemSource(string path, bool write, bool parentLevel)
        {
            try
            {
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
                        AddItem(new(SaveController.GetSaveName(folders[i].path), "save-radio-button", i));
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
    }
}