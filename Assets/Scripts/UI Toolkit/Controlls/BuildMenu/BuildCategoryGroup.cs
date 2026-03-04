using AbstractControls;
using System.Linq;
using UnityEngine.UIElements;

namespace BottomBar.Building
{
    public partial class BuildCategoryGroup : CustomRadioButtonGroup
    {
        public BuildCategoryGroup()
        {
            AddToClassList("categ-bar");
            int i = 0;
            foreach (BuildCategWrapper categ in SceneRefs.ObjectFactory.buildPrefabs.Categories)
            {
                Add(CreateCategButton(categ, i));
                i++;
            }
        }

        CustomRadioButton CreateCategButton(BuildCategWrapper categ, int i)
        {
            CustomRadioButton button = new("building-categ", i, this, true);
            if (i == 0)
                button.style.display = DisplayStyle.None;

            VisualElement img = new();
            img.style.backgroundImage = Background.FromVectorImage(categ.Icon);
            img.AddToClassList("building-background");
            button.Add(img);
            return button;
        }
    }
}