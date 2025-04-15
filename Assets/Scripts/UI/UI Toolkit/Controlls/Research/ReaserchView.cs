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
		public void Init()
		{
			ResearchData data = UIRefs.research.researchData;
			Vector2 categWindowSize = new(1920, 1080);
			for (int i = 0; i < data.Categories.Count; i++)
			{
				ResearchCategory category = data.Categories[i];
				Tab tab = new(category.Name, category.Icon);

				ResearchRadioButtonGroup group = new ResearchRadioButtonGroup(category);
				group.SetChangeCallback((nodeIndex) => OpenButton(category.Objects[nodeIndex]));

				tab.Add(group);
				Add(tab);
			}
			activeTabChanged += OnTabChange;
		}

		void OnTabChange(Tab prevTab, Tab arg2)
		{
			prevTab.Q<ResearchRadioButtonGroup>().Select(-1);
		}

		void OpenButton(ResearchNode node)
		{
			if(node.researched == false)
			{
				UIRefs.research.SetActive(node);
				SceneRefs.ShowMessage($"Research Changed {node.name}");
			}
		}


		public void Open(object data)
		{
			Debug.Log("Opening research!");
		}
	}
}