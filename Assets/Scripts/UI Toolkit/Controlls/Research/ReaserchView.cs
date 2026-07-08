using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace ResearchUI
{
    [UxmlElement]
    public partial class ResearchView : TabView, IInitiableUI<ResearchData>, IUIElement
    {
        int prevGroup;
        List<ResearchRadioButtonGroup> groups;
        Button closeButton;

        public ResearchView() : base()
        {
            closeButton = new Button();
            closeButton.AddToClassList("close-button");
            hierarchy.Add(closeButton);
        }

        public void Init(ResearchData data)
        {
            closeButton.clicked += UIRefs.ResearchWindow.CloseWindow;
            Vector2 categWindowSize = new(1920, 1080);
            groups = new List<ResearchRadioButtonGroup>();
            for (int i = 0; i < data.Categories.Count; i++)
            {
                ResearchCategory category = data.Categories[i];
                Tab tab = new(category.Name);
                VisualElement element;
                tab.tabHeader.Insert(0, element = new VisualElement() { style = { backgroundImage = Background.FromVectorImage(category.Icon) } });
                tab.tabHeader.style.minWidth = new Length(100 / data.Categories.Count, LengthUnit.Percent);
                if (i == data.Categories.Count - 1)
                    tab.tabHeader.style.borderRightWidth = 0;
                element.AddToClassList("tab-icon");

                ResearchRadioButtonGroup group = new ResearchRadioButtonGroup(category);
                group.SetChangeCallback(
                    (nodeIndex) =>
                    {
                        if (nodeIndex > -1)
                            OpenButton(category.Objects[nodeIndex], group);
                    });
                groups.Add(group);
                tab.Add(group);
                Add(tab);
            }
        }

        /// <summary>
        /// Sets the node as active reseach and shows a message.
        /// </summary>
        /// <param name="node">New active node</param>
        /// <param name="group">Group used to get find the category index.</param>
        void OpenButton(ResearchNode node, ResearchRadioButtonGroup group)
        {
            if (node?.researched == false)
            {
                UIRefs.Research.SetActive(node);
                NotificationController.ShowMessage($"Research Changed {node.Name}");
                prevGroup = groups.IndexOf(group);
            }
        }


        public void Open(object data)
        {
            if (data != null)
            {
                switch (data)
                {
                    case (int cat, int row, int j):
                        selectedTabIndex = cat;
                        VisualElement button = groups[cat].contentContainer[row][1][j];
                        button.AddToClassList("found");
                        button.schedule.Execute(
                            () =>
                            {
                                button.RemoveFromClassList("found");
                            }).ExecuteLater(600);
                        break;
                }
            }
            Debug.Log("Opening research!");
        }
    }
}