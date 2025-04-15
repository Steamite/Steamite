using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditorInternal.VR;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorWindows.Research
{
    [UxmlElement]
    public partial class ResearchNodeElem : VisualElement
    {
		public ResearchNodeElem()
		{
			Add(new Label("no node data"));
		}

		public ResearchNodeElem(ResearchNode _nodeData, ResearchEditor editor, ResearchData data)
		{
			Button topConnector = null;

			if (_nodeData.level != 0) 
			{
				topConnector = new();
				topConnector.RegisterCallback<ClickEvent>((ev) => TopConnect(ev, _nodeData, editor));
				topConnector.name = "Top";
				topConnector.AddToClassList("connector");
				topConnector.style.bottom = StyleKeyword.Auto;
				topConnector.style.top = -12;
				Add(topConnector);
			}

			Button botConnector = new(() => BottomConnect(_nodeData, editor));
			botConnector.name = "Bot";
			botConnector.AddToClassList("connector");


			VisualElement body = new();
			
			#region Buttons
			VisualElement moveButtons = new();
			moveButtons.AddToClassList("top-buttons");
			// left move
			Button moveButton = new Button(null, () => editor.Move(_nodeData, -1));
			moveButton.AddToClassList("move-button");
			moveButton.iconImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Icons/Info window/arrow-left@2x.texture2D");
			moveButton.SetEnabled(editor.Exists(_nodeData, -1));
			moveButtons.Add(moveButton);
			moveButtons.Add(new());
			moveButtons[1].style.flexDirection = FlexDirection.RowReverse;
			moveButtons[1].style.flexGrow = 1;

			// right move
			moveButton = new Button(null, () => editor.Move(_nodeData, 1));
			moveButton.AddToClassList("move-button");
			moveButton.iconImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Icons/Info window/arrow-right@2x.texture2D");
			moveButton.SetEnabled(editor.Exists(_nodeData, 1));
			moveButtons[1].Add(moveButton);

			// delete button
			moveButton = new Button();
			moveButton.AddToClassList("cancel-button");
			moveButton.iconImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Icons/Info window/Cancel.png");
			moveButton.RegisterCallback<ClickEvent>((_) =>
			{
				if (_.clickCount == 2)
				{
					editor.Delete(_nodeData);
				}
			});
			moveButtons[1].Add(moveButton);
			
			if(_nodeData.level != 0)
			{
				ColorField colorF = new();
				colorF.value = _nodeData.lineColor;
				colorF.RegisterValueChangedCallback<Color>(
					(ev) => 
					{
						_nodeData.lineColor = ev.newValue;
						editor.SaveValues();
						editor.RepaintRow(_nodeData.level);
					});
				colorF.AddToClassList("color");
				colorF.style.width = 50;
				colorF.style.height = 15;
				moveButtons[1].Add(colorF);
			}
			body.Add(moveButtons);
			#endregion

			#region Name && Time
			body.Add(new());
			body[1].style.flexDirection = FlexDirection.Row;
			TextField nameField = new TextField();
			nameField.AddToClassList("name-text");
			nameField.value = _nodeData.name;
			nameField.RegisterValueChangedCallback<string>(
				(ev) =>
				{
					_nodeData.name = ev.newValue;
					editor.SaveValues();
				});
			body[1].Add(nameField);
			IntegerField intField = new IntegerField();
			intField.AddToClassList("time-text");
			intField.value = _nodeData.researchTime;
			intField.RegisterValueChangedCallback<int>(
				(ev) =>
				{
					_nodeData.researchTime = ev.newValue;
					editor.SaveValues();
				});
			body[1].Add(intField);
			#endregion

			#region Cost and Dropdowns
			ResourceCell cell = new ResourceCell();
			cell.Open(_nodeData.reseachCost, data, true);
			body.Add(cell);

			EnumField stat = new(_nodeData.nodeType);
			DropdownField category = new(
				 editor.GetActiveCategories(_nodeData),
				_nodeData.nodeCategory + 1);

			DropdownField assignee = new(
				editor.GetActiveBuildings(_nodeData),
				_nodeData.nodeAssignee == -1 ? 0 : 1);
			assignee.SetEnabled(_nodeData.nodeCategory != -1);

			// Stat
			stat.RegisterValueChangedCallback<Enum>(
				(ev) =>
				{
					NodeType newVal = (NodeType)ev.newValue;
					if(newVal != _nodeData.nodeType)
					{
						ToggleElements(newVal, body, topConnector, botConnector);
						switch (newVal)
						{
							case NodeType.Dummy:
								break;
							case NodeType.Stat:
								break;
							case NodeType.Building:
								editor.RecalculateAvailableBuildings();
								break;
						}
						_nodeData.nodeType = newVal;
						_nodeData.nodeCategory = -1;
						editor.SaveValues();
						category.choices = editor.GetActiveCategories(_nodeData);
						category.value = "Select";
						assignee.value = "Select";
						assignee.SetEnabled(false);
					}
				});
			body.Add(stat);

			// Category
			category.RegisterValueChangedCallback<string>(
				(ev) =>
				{
					if (_nodeData.nodeCategory != category.index - 1)
					{
						if (ev.newValue == "Select")
							_nodeData.nodeCategory = -1;
						else
						{
							_nodeData.nodeCategory = category.index - 1;
							assignee.choices = editor.GetActiveBuildings(_nodeData);
						}
						_nodeData.nodeAssignee = -1;
						editor.SaveValues();
						editor.RecalculateAvailableBuildings();
						assignee.SetEnabled(_nodeData.nodeCategory != -1);
					}
				});
			body.Add(category);

			// Assign
			assignee.RegisterCallback<PointerDownEvent>(
				(ev) =>
				{
					assignee.choices = editor.GetActiveBuildings(_nodeData);
					assignee.SetValueWithoutNotify(assignee.choices[_nodeData.nodeAssignee == -1 ? 0 : 1]);
				});
			body.Add(assignee);

			assignee.RegisterValueChangedCallback<string>(
				(ev) =>
				{
					if (assignee.index == 0)
					{
						if (_nodeData.nodeAssignee != -1)
						{
							_nodeData.nodeAssignee = -1;
							editor.RecalculateAvailableBuildings();
						}
						_nodeData.nodeAssignee = -1;
					}
					else if (assignee.index == 1)
					{
						if (_nodeData.nodeAssignee == -1)
						{
							_nodeData.nodeAssignee = editor.buildingData.Categories[_nodeData.nodeCategory]
								.availableBuildings.FirstOrDefault(q => q.b.objectName == ev.newValue).id;
							editor.RecalculateAvailableBuildings();
						}
						else
							return;
					}
					else
					{
						_nodeData.nodeAssignee = editor.buildingData.Categories[_nodeData.nodeCategory].availableBuildings.FirstOrDefault(q => q.b.objectName == ev.newValue).id;
						editor.RecalculateAvailableBuildings();
					}
					editor.SaveValues();
				});
			body.Add(assignee);
			#endregion


			TextField field = new();
			field.AddToClassList("description-field");
			field.value = _nodeData.description;
			field.multiline = true;
			field.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
			field.RegisterCallback<FocusOutEvent>(
				(_) => 
				{
					_nodeData.description = field.value;
					editor.SaveValues();
				});

			body.Add(field);
			Add(body);


			Add(botConnector);

			style.flexGrow = 0;
			style.height = 250;
			AddToClassList("body");

			ToggleElements(_nodeData.nodeType, body, topConnector, botConnector);
		}
		void ToggleElements(
			NodeType type, 
			VisualElement parent,
			VisualElement topConnect,
			VisualElement bottomConnect)
		{
			DisplayStyle style = type == NodeType.Building ? DisplayStyle.Flex : DisplayStyle.None;
			if (topConnect != null)
				topConnect.style.display = style;
			for (int i = 0; i < parent.childCount; i++)
			{
				parent[i].style.display = parent[i] is EnumField ? DisplayStyle.Flex : style;
			}
			bottomConnect.style.display = style;
		}

		/// <summary>
		/// Needs to have a previously selected node.
		/// </summary>
		/// <param name="nodeData">This node.</param>
		/// <param name="editor">Editor reference for getting the last selected and updating lines.</param>
		void TopConnect(ClickEvent ev, ResearchNode nodeData, ResearchEditor editor)
		{
			Debug.Log("Top - clicked");
			if (editor.activeNode != null)
			{
				editor.tree[editor.activeNode.level][1][editor.selectedCategory.Objects.FindIndex(q => q.id == editor.activeNode.id)].Q<Button>("Bot").RemoveFromClassList("selected");
				editor.activeNode.ConnectNode(nodeData);
				editor.SaveValues();
				editor.RepaintRow(nodeData.level);
			}
			else if(ev.clickCount == 2)
			{
				nodeData.DisconnectNodes(editor.selectedCategory.Objects, true);
				editor.SaveValues();
				editor.RepaintRow(nodeData.level);
			}
			editor.activeNode = null;
		}

		void BottomConnect(ResearchNode nodeData, ResearchEditor editor)
		{
			Debug.Log("Bot - clicked");
			if (editor.activeNode != nodeData)
				editor.activeNode = nodeData;
			else
				editor.activeNode = null;
			this.Q<Button>("Bot").ToggleInClassList("selected");
		}
	}
}
