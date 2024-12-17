using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace StartMenu
{
    public class MyMainMenu : MonoBehaviour, IToolkitController
    {
        VisualElement root;

        [SerializeReference] public List<MonoBehaviour> toolkitControllers;

        public void Init(VisualElement root)
        {
            foreach (IToolkitController controller in toolkitControllers)
            {
                controller.Init(root);
            }
        }

        void Start()
        {
            root = gameObject.GetComponent<UIDocument>().rootVisualElement;

            Init(root);

            // opening load menu
            Button button = root.Q<Button>("Exit-Button");
            button.RegisterCallback<ClickEvent>((_) => Application.Quit());
        }
    }
}