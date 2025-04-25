using AbstractControls;
using UnityEngine.UIElements;

namespace BuildMenu
{
    public partial class BuildCategoryGroup : CustomRadioButtonGroup
    {
        public BuildCategoryGroup()
        {
            AddToClassList("categ-bar");
            int i = 0;
            foreach (BuildCategWrapper categ in SceneRefs.objectFactory.buildPrefabs.Categories)
            {
                Add(CreateCategButton(categ, i));
                i++;
            }
        }

        CustomRadioButton CreateCategButton(BuildCategWrapper categ, int i)
        {
            CustomRadioButton button = new("building-categ", i, true);

            VisualElement img = new();
            img.style.backgroundImage = categ.Icon;
            img.AddToClassList("building-background");
            button.Add(img);
            return button;
        }
    }
}