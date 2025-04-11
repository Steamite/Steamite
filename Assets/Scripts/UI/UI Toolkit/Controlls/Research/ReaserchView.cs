using AbstractControls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Research
{
	[UxmlElement]
	public partial class ResearchView : TabView, IInitiableUI, IUIElement
	{
		[UxmlAttribute] VisualTreeAsset button;
		public void Init()
		{
			/*ResearchData data = UIRefs.research.researchData;
			Vector2 categWindowSize = new(1920, 1080);
			return;
			for (int i = 0; i < data.categories.Count; i++)
			{
				ResearchCategory category = data.categories[i];
				Tab tab = new(category.categName, category.icon);
				for (int j = 0; j < category.nodes[^1].level; j++)
					tab.Add(new());

				foreach(ResearchNode node in category.nodes)
				{
					ResearchUIButton researchUIButton = new(node, category.nodes);
				}
				Add(tab);
			}*/
		}

		public void Open(object data)
		{
			throw new System.NotImplementedException();
		}
	}
}