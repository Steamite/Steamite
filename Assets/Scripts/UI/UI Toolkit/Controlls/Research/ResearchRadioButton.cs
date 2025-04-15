using AbstractControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Research
{
	public class ResearchRadioButton : CustomRadioButton
	{
		enum ButtonState
		{
			Unavailable,
			Available,
			Completed
		}

		//Variables
		public ResearchNode node;
		public VisualElement lineDown;
		public VisualElement lineUp;

		ButtonState state;

		int categ;
		int building;

		public ResearchRadioButton(List<ResearchNode> nodes, int i) : base("research-button", i, true)
		{
			name = nodes[i].name;
			node = nodes[i];

			if (node.nodeType == NodeType.Dummy || node.nodeAssignee == -1)
			{
				style.visibility = Visibility.Hidden;
				return;
			}
			VisualElement background = new();
			background.AddToClassList("rotator");
			Add(background);

			VisualElement preview = new();
			BuildCategWrapper cat = UIRefs.research.buildData.Categories[node.nodeCategory];
			building = cat.Objects.FindIndex(q => q.id == node.nodeAssignee);
			if (building > -1)
				preview.style.backgroundImage = new(cat.Objects[building].preview);
			background.Add(preview);

			Label nameLabel = new();
			nameLabel.AddToClassList("name-label");
			nameLabel.text = node.name;
			background.Add(nameLabel);

			RegisterCallback<PointerEnterEvent>(_ => ToolkitUtils.localMenu.Open(node, this));
			RegisterCallback<PointerLeaveEvent>(_ => ToolkitUtils.localMenu.Close());
		}

		/*public void Initialize(ResearchNode researchNode, List<ResearchNode> nodes)
		{
			
			

			
			ManageBuildButton();
		}

		void Recolor(bool doLines = false)
		{
			switch (state)
			{
				case ButtonState.Unavailable:
					transform.GetChild(0).GetComponent<Image>().color = Col(63, 66, 67);
					transform.GetChild(1).GetComponent<Image>().color = Col(86, 90, 91);
					transform.GetChild(2).GetComponent<Image>().color = Col(105, 108, 109);
					break;
				case ButtonState.Available:
					transform.GetChild(0).GetComponent<Image>().color = Col(158, 101, 38);
					transform.GetChild(1).GetComponent<Image>().color = Col(197, 144, 49);
					transform.GetChild(2).GetComponent<Image>().color = Col(210, 159, 69);
					break;
				case ButtonState.Completed:
					transform.GetChild(0).GetComponent<Image>().color = Col(19, 128, 95);
					transform.GetChild(1).GetComponent<Image>().color = Col(22, 159, 120);
					transform.GetChild(2).GetComponent<Image>().color = Col(71, 187, 139);
					Destroy(borderFill);
					break;
			}
			if (doLines)
				RecolorLines();
		}

		public void RecolorLines()
		{
			switch (state)
			{
				case ButtonState.Unavailable:
					foreach (Image image in unlocksLines)
					{
						image.color = Col(105, 108, 109);
					}
					break;
				case ButtonState.Available:
					foreach (Image image in unlockedByLines)
					{
						image.color = Col(210, 159, 69);
					}
					break;
				case ButtonState.Completed:
					foreach (Image image in unlockedByLines)
					{
						image.color = Col(29, 176, 126);
					}
					foreach (Image image in unlocksLines)
					{
						image.color = Col(210, 159, 69);
					}
					break;
			}
		}

		Color Col(float r, float g, float b)
		{
			return new(r / 255f, g / 255f, b / 255f, 1);
		}

		public void OnClick()
		{
			UIRefs.research.ResearchButtonClick(this);
		}

		void ManageBuildButton()
		{
			Transform button = SceneRefs.buildMenu.GetChild(1).GetChild(node.buttonCategory).transform.GetChild(node.buildButton);
			switch (state)
			{
				case ButtonState.Unavailable:
					button.gameObject.SetActive(false);
					if (button.parent.GetComponentsInChildren<BuildButton>().Length == 0)
						SceneRefs.buildMenu.GetChild(0).GetChild(button.parent.GetSiblingIndex()).gameObject.SetActive(false);
					break;
				case ButtonState.Available:
					button.gameObject.SetActive(true);
					button.GetComponent<Button>().interactable = false;
					SceneRefs.buildMenu.GetChild(0).GetChild(button.parent.GetSiblingIndex()).gameObject.SetActive(true);
					break;
				case ButtonState.Completed:
					button.gameObject.SetActive(true);
					button.GetComponent<Button>().interactable = true;
					SceneRefs.buildMenu.GetChild(0).GetChild(button.parent.GetSiblingIndex()).gameObject.SetActive(true);
					break;
			}
		}

		public void Complete(bool init = false)
		{
			state = ButtonState.Completed;
			node.researched = true;
			node.currentTime = node.researchTime;
			if (!init)
			{
				EndAnim(true);
				foreach (int i in node.unlocks)
				{
					int categ = transform.parent.parent.GetSiblingIndex();
					UIRefs.research.GetResearchUIButton(categ, i).Unlock(this);
				}
				borderFill.fillAmount = 1;
			}
			SceneRefs.ShowMessage($"Reseach finished: {node.name}");
			Recolor(true);
			UIRefs.research.UpdateInfoWindow(this);
			ManageBuildButton();
		}

		void Unlock(ResearchUIButton currentResearch)
		{
			if (node.unlockedBy.Contains(currentResearch.node.id))
			{
				unlockedPrevs++;
				if (unlockedPrevs == node.unlockedBy.Count)
				{
					state = ButtonState.Available;
					Recolor();
				}
				ManageBuildButton();
			}
		}
		*/
	}
}