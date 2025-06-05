using AbstractControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace ResearchUI
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
        /// <summary>Node of the button.</summary>
        public ResearchNode node;
        /// <summary>Line from the button down.</summary>
        public ResearchDownLine lineDown;
        /// <summary>Line from the button up.</summary>
        public ResearchLine lineUp;

        /// <summary>Background image.</summary>
        VisualElement background;

        /// <summary>Research state of the button.</summary>
        ButtonState state;

        /// <summary>Building Index.</summary>
        int building;

        /// <summary>Creates the button using the <paramref name="categ"/>.</summary>
        /// <param name="categ">Research category to find the node.</param>
        /// <param name="i">Index of the node in category.</param>
        public ResearchRadioButton(ResearchCategory categ, int i) : base("research-button", i, true)
        {
            node = categ.Objects[i];
            name = node.Name;

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
                node.RegisterFinishCallback(FinishResearch);
            }

            VisualElement preview = new();
            preview.AddToClassList("research-button-background");
            BuildCategWrapper cat = SceneRefs.objectFactory.buildPrefabs.Categories[node.nodeCategory];
            building = cat.Objects.FindIndex(q => q.id == node.nodeAssignee);
            if (building > -1)
            {
                node.preview = cat.Objects[building].preview;
                preview.style.backgroundImage = new(cat.Objects[building].preview);
            }
            Add(preview);

            Label nameLabel = new();
            nameLabel.AddToClassList("name-label");
            nameLabel.text = node.Name;
            Add(nameLabel);

            RegisterCallback<PointerEnterEvent>(_ => ToolkitUtils.localMenu.UpdateContent(node, this));
            RegisterCallback<PointerLeaveEvent>(_ => ToolkitUtils.localMenu.Close());
        }

        /// <summary>
        /// Marks lines and updates the <see cref="state"/>.
        /// </summary>
        /// <param name="categ">Category to check prequisite nodes.</param>
        public void AfterLines(ResearchCategory categ)
        {
            if (node.researched)
                return;
            if (categ.CheckPrequisite(node, () => UnlockResearch(false)))
            {
                if (node == UIRefs.research.currentResearch)
                {
                    UnlockResearch(true);
                    SelectWithoutTransition(false); // updates the group but without checks
                    ToolkitUtils.GetParentOfType<ResearchRadioButtonGroup>(this).SetSelection(value);
                }
                else
                    UnlockResearch(false);
                    
            }
            else
                state = ButtonState.Unavailable;
        }
        protected override bool SelectChange(bool UpdateGroup)
        {
            if(UpdateGroup == false)
            {
                base.SelectChange(UpdateGroup);
                return true;
            }
            else if (state == ButtonState.Available && MyRes.CanAfford(node.reseachCost))
            {
                if (UIRefs.research.currentResearch == null || UpdateGroup == false)
                {
                    if (node.CurrentTime < 0)
                    {
                        node.CurrentTime = 0;
                        MyRes.PayCostGlobal(node.reseachCost);
                    }
                    ToolkitUtils.localMenu.UpdateContent(node, this, true);
                    base.SelectChange(UpdateGroup);
                    RemoveFromClassList(AVAILABLE_CLASS);
                    return true;
                }
                else
                {
                    ToolkitUtils.ChangeClassWithoutTransition(AVAILABLE_CLASS, "forceHover", this);
                    ConfirmWindow.window.Open(
                        () =>
                        {
                            // Clear research(queue[WIP]) and select this button.
                            UIRefs.research.SetActive(null);
                            ToolkitUtils.RemoveClassWithoutTransition("forceHover", this);
                            Select();
                        },
                        () =>
                        {
                            RemoveFromClassList("forceHover");
                            AddToClassList(AVAILABLE_CLASS);
                        },
                        "Change Research",
                        "Do you want to change active Research?");
                }
            }
            return false;
        }

        /// <summary>Called when finishing research.</summary>
        void FinishResearch()
        {
            AddToClassList(RESEARCH_CLASS);
            state = ButtonState.Researched;
            UIRefs.research.FinishResearch();
            Deselect(false);
            if (lineUp != null)
                lineUp.Fill();
        }

        void UnlockResearch(bool init)
        {
            state = ButtonState.Available;
            if (init == false)
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