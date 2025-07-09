using ResearchUI;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
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
            #region Top Connector
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
            #endregion

            #region BottomConnector
            Button botConnector = new(() => BottomConnect(_nodeData, editor));
            botConnector.name = "Bot";
            botConnector.AddToClassList("connector");
            #endregion

            VisualElement body = new();

            #region Buttons
            VisualElement moveButtons = new();
            moveButtons.AddToClassList("top-buttons");
            // left move
            Button moveButton = new Button(null, () => editor.Move(_nodeData, -1));
            moveButton.AddToClassList("move-button");
            moveButton.iconImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Icons/Info window/arrow-left@2x.texture2D");
            moveButton.SetEnabled(editor.Exists(_nodeData, false));
            moveButtons.Add(moveButton);
            moveButtons.Add(new());
            moveButtons[1].style.flexDirection = FlexDirection.RowReverse;
            moveButtons[1].style.flexGrow = 1;

            // right move
            moveButton = new Button(null, () => editor.Move(_nodeData, 1));
            moveButton.AddToClassList("move-button");
            moveButton.iconImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Icons/Info window/arrow-right@2x.texture2D");
            moveButton.SetEnabled(editor.Exists(_nodeData, true));
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

            Toggle toggle = new Toggle();
            toggle.AddToClassList("toggle");
            toggle.text = "";
            toggle.value = _nodeData.researched;
            toggle.RegisterValueChangedCallback<bool>(
                (ev) =>
                {
                    _nodeData.researched = ev.newValue;
                    editor.SaveValues();
                });
            moveButtons[1].Add(toggle);
            #endregion

            #region Color
            if (_nodeData.level != 0)
            {
                ColorField colorF = new();
                colorF.value = _nodeData.lineColor;
                //Debug.Log("bbbbbb");
                colorF.RegisterValueChangedCallback<Color>(
                    (ev) =>
                    {
                        Debug.Log("adsadsd");
                        _nodeData.lineColor = colorF.value;
                        editor.SaveValues();
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
            nameField.value = _nodeData.Name;
            nameField.RegisterValueChangedCallback<string>(
                (ev) =>
                {
                    _nodeData.Name = ev.newValue;
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
            ResCell cell = new ResCell();
            cell.Open(_nodeData.reseachCost, data, true);
            body.Add(cell);

            EnumField stat = new(_nodeData.nodeType);
            DropdownField category = new(
                 editor.GetActiveCategories(_nodeData),
                _nodeData.nodeCategory + 1);

            DropdownField assignee = new(
                editor.GetAvailable(_nodeData),
                _nodeData.nodeAssignee == -1 ? 0 : 1);
            assignee.SetEnabled(_nodeData.nodeCategory != -1);

            // Stat
            stat.RegisterValueChangedCallback<Enum>(
                (ev) =>
                {
                    NodeType newVal = (NodeType)ev.newValue;
                    if (newVal != _nodeData.nodeType)
                    {
                        ToggleElements(newVal, body, topConnector, botConnector);
                        editor.RecalculateAvailableByNode(_nodeData);
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
                            assignee.choices = editor.GetAvailable(_nodeData);
                        }
                        _nodeData.nodeAssignee = -1;
                        editor.SaveValues();
                        editor.RecalculateAvailableByNode(_nodeData);
                        assignee.SetEnabled(_nodeData.nodeCategory != -1);
                    }
                });
            body.Add(category);

            // Assign
            assignee.RegisterCallback<PointerDownEvent>(
                (ev) =>
                {
                    assignee.choices = editor.GetAvailable(_nodeData);
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
                            editor.RecalculateAvailableByNode(_nodeData);
                        }
                        _nodeData.nodeAssignee = -1;
                    }
                    else if (assignee.index == 1)
                    {
                        if (_nodeData.nodeAssignee == -1)
                        {
                            SetAssigne(_nodeData, ev.newValue, editor);
                        }
                        else
                            return;
                    }
                    else
                    {
                        SetAssigne(_nodeData, ev.newValue, editor);
                    }
                    editor.SaveValues();
                });
            body.Add(assignee);
            #endregion


            #region Description
            TextField field = new();
            field.AddToClassList("description-field");
            field.value = _nodeData.description;
            field.style.height = 100;
            field.multiline = true;
            field.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            field.RegisterCallback<FocusOutEvent>(
                (_) =>
                {
                    _nodeData.description = field.value;
                    editor.SaveValues();
                });

            body.Add(field);
            #endregion

            Add(body);

            Add(botConnector);

            style.flexGrow = 0;
            style.height = 300;
            AddToClassList("body");

            ToggleElements(_nodeData.nodeType, body, topConnector, botConnector);
        }

        void SetAssigne(ResearchNode _nodeData, string newVal, ResearchEditor editor)
        {
            switch (_nodeData.nodeType)
            {
                case NodeType.Dummy:
                    return;
                case NodeType.Building:
                    _nodeData.nodeAssignee =
                        editor.buildingData.Categories[_nodeData.nodeCategory]
                        .availableObjects.FirstOrDefault(q => q.GetName() == newVal).id;
                    break;
                case NodeType.Stat:
                    BuildingStats.Stat stat = editor.statData.Categories[_nodeData.nodeCategory]
                        .availableObjects.FirstOrDefault(q => q.GetName() == newVal);
                    _nodeData.nodeAssignee = stat.id;
                    GetDescr(_nodeData, editor);
                    break;
            }
            editor.RecalculateAvailableByNode(_nodeData);
        }

        void GetDescr(ResearchNode _nodeData, ResearchEditor editor)
        {
            if (_nodeData.nodeAssignee > -1)
            {
                string descr = _nodeData.GetDescr(editor.statData.Categories[_nodeData.nodeCategory]
                    .Objects.FirstOrDefault(q => q.id == _nodeData.nodeAssignee));
                this.Q<TextField>(className: "description-field").value = descr;
            }
        }

        /// <summary>Toggles the elements <see cref="NodeType.Dummy"/>.</summary>
        /// <param name="type">Type of the node.</param>
        /// <param name="body">Body component.</param>
        /// <param name="topConnect">Top button.</param>
        /// <param name="bottomConnect">Bot button.</param>
        void ToggleElements(
            NodeType type,
            VisualElement body,
            VisualElement topConnect,
            VisualElement bottomConnect)
        {
            DisplayStyle style = type == NodeType.Dummy ? DisplayStyle.None : DisplayStyle.Flex;
            if (topConnect != null)
                topConnect.style.display = style;
            for (int i = 0; i < body.childCount; i++)
            {
                body[i].style.display = body[i] is EnumField ? DisplayStyle.Flex : style;
            }
            bottomConnect.style.display = style;
        }

        #region Connections
        /// <summary>Needs to have a previously selected node.</summary>
        /// <param name="nodeData">This node.</param>
        /// <param name="editor">Editor reference for getting the last selected and updating lines.</param>
        void TopConnect(ClickEvent ev, ResearchNode nodeData, ResearchEditor editor)
        {
            Debug.Log("Top - clicked");
            if (editor.activeNode != null)
            {
                int i = editor.GetIndexInRow(editor.activeNode);
                editor.tree[editor.activeNode.level][1][i]
                    .Q<Button>("Bot").RemoveFromClassList("selected");
                editor.activeNode.ConnectNode(nodeData);
                editor.SaveValues();
                editor.RepaintRow(nodeData.level);
            }
            else if (ev.clickCount == 2)
            {
                nodeData.DisconnectNodes(editor.selectedCategory.Objects, true);
                editor.SaveValues();
                editor.RepaintRow(nodeData.level);
            }
            editor.activeNode = null;
        }

        /// <summary>Marks the node for connecting.</summary>
        /// <param name="nodeData"></param>
        /// <param name="editor"></param>
        void BottomConnect(ResearchNode nodeData, ResearchEditor editor)
        {
            Debug.Log("Bot - clicked");
            if (editor.activeNode != nodeData)
                editor.activeNode = nodeData;
            else
                editor.activeNode = null;
            this.Q<Button>("Bot").ToggleInClassList("selected");
        }
        #endregion
    }
}
