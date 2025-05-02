using UnityEngine;
using UnityEngine.UIElements;

namespace Research
{
    [UxmlElement]
    public partial class ResearchView : TabView, IInitiableUI, IUIElement
    {
        ResearchRadioButtonGroup prevSetGroup;
        public void Init()
        {
            ResearchData data = UIRefs.research.researchData;
            Vector2 categWindowSize = new(1920, 1080);
            for (int i = 0; i < data.Categories.Count; i++)
            {
                ResearchCategory category = data.Categories[i];
                Tab tab = new(category.Name, category.Icon);

                ResearchRadioButtonGroup group = new ResearchRadioButtonGroup(category);
                group.SetChangeCallback(
                    (nodeIndex) => 
                    {
                        if (nodeIndex > -1) 
                            OpenButton(category.Objects[nodeIndex], group);
                    });

                tab.Add(group);
                Add(tab);
            }
        }

        void OpenButton(ResearchNode node, ResearchRadioButtonGroup group)
        {
            if (node?.researched == false)
            {
                prevSetGroup?.Select(-1);
                UIRefs.research.SetActive(node);
                SceneRefs.ShowMessage($"Research Changed {node.nodeName}");
                prevSetGroup = group;
            }
        }


        public void Open(object data)
        {
            Debug.Log("Opening research!");
        }
    }
}