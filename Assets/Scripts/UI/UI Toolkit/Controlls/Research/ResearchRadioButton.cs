using AbstractControls;
using UnityEngine.UIElements;

namespace Research
{
    public class ResearchRadioButton : CustomRadioButton
    {
        const string RESEARCH_CLASS = "researched";
        const string AVAILABLE_CLASS = "available";

        enum ButtonState
        {
            /// <summary>Still needs some prequisites to be reasearched.</summary>
            Unavailable,
            /// <summary>Has all researched prequisites.</summary>
            Available,
            /// <summary>Researched is set to true.</summary>
            Researched
        }

        //Variables
        public ResearchNode node;
        public ResearchDownLine lineDown;
        public ResearchLine lineUp;

        VisualElement background;

        ButtonState state;

        int categ;
        int building;

        public ResearchRadioButton(ResearchCategory categ, int i) : base("research-button", i, true)
        {
            node = categ.Objects[i];
            name = node.nodeName;

            if (node.nodeType == NodeType.Dummy || node.nodeAssignee == -1)
            {
                style.visibility = Visibility.Hidden;
                return;
            }

            background = new();
            background.AddToClassList("rotator");
            Add(background);

            if (node.researched == true)
            {
                AddToClassList(RESEARCH_CLASS);
                state = ButtonState.Researched;
            }
            else
            {
                node.onFinishResearch += FinishResearch;
            }

            VisualElement preview = new();
            BuildCategWrapper cat = SceneRefs.objectFactory.buildPrefabs.Categories[node.nodeCategory];
            building = cat.Objects.FindIndex(q => q.id == node.nodeAssignee);
            if (building > -1)
            {
                node.preview = cat.Objects[building].preview;
                preview.style.backgroundImage = new(cat.Objects[building].preview);
            }
            background.Add(preview);

            Label nameLabel = new();
            nameLabel.AddToClassList("name-label");
            nameLabel.text = node.nodeName;
            background.Add(nameLabel);

            RegisterCallback<PointerEnterEvent>(_ => ToolkitUtils.localMenu.Open(node, this));
            RegisterCallback<PointerLeaveEvent>(_ => ToolkitUtils.localMenu.Close());
        }

        public void AfterLines(ResearchCategory categ)
        {
            if (node.researched)
                return;
            if (categ.CheckPrequisite(node, UnlockResearch))
            {
                UnlockResearch();
                if (node == UIRefs.research.currentResearch)
                    Select();
            }
            else
                state = ButtonState.Unavailable;
        }

        protected override bool SelectChange(bool UpdateGroup)
        {
            if (state == ButtonState.Available && MyRes.CanAfford(node.reseachCost))
            {
                if (UIRefs.research.currentResearch == null || UIRefs.research.currentResearch.id == node.id)
                {
                    if (node.CurrentTime < 0)
                    {
                        node.CurrentTime = 0;
                        MyRes.PayCostGlobal(node.reseachCost);
                    }
                    if (UIRefs.research.currentResearch == null)
                        ToolkitUtils.localMenu.Open(node, this);
                    base.SelectChange(UpdateGroup);
                    RemoveFromClassList(AVAILABLE_CLASS);
                    return true;
                }
                else
                {
                    ConfirmWindow.window.Open(
                        () =>
                        {
                            UIRefs.research.SetActive(node);
                            Select();
                        },
                        "Change Research",
                        "Do you want to change active Research?");
                }
            }
            return false;
        }

        void FinishResearch()
        {
            AddToClassList(RESEARCH_CLASS);
            state = ButtonState.Researched;
            UIRefs.research.FinishResearch();
            Deselect(false);
            if (lineUp != null)
                lineUp.Fill();
        }

        void UnlockResearch()
        {
            state = ButtonState.Available;
            AddToClassList(AVAILABLE_CLASS);
            if (lineDown != null)
                lineDown.Fill();
        }

        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect(triggerTransition);
            if (state == ButtonState.Available)
                AddToClassList(AVAILABLE_CLASS);
        }
    }
}