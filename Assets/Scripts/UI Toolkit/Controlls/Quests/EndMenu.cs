using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EndMenu : MonoBehaviour, IUIElement
{
    [SerializeField] PanelSettings settings;
    [SerializeField] VisualTreeAsset asset;

    public LoadGameMenu loadGameMenu;
    UIDocument doc;

    public void Open(object data)
    {
        MainShortcuts.DisableAll();
        bool result = (bool)data;
        doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = settings;
        doc.visualTreeAsset = asset;
        doc.sortingOrder = 5;

        UIRefs.FullscreenConstraint();
        SceneRefs.Tick.UIWindowToggle(false);
        if (result)
        {
            GameWon(doc.rootVisualElement[0]);
        }
        else
        {
            GameLost(doc.rootVisualElement[0][2]);
        }
    }

    public void GameWon(VisualElement element)
    {
        Label label = element[0] as Label;
        label.text = "Victory";
        label = element[1] as Label;
        label.text = "You finished all orders and made the company proud!";

        element = element[2];
        Button button = element[0] as Button;
        button.text = "Continue";
        button.clicked += () =>
        {
            Destroy(doc);
            MainShortcuts.EnableAll();
            SceneRefs.Tick.UIWindowToggle(true);
        };

        (element[1] as Button).style.display = DisplayStyle.None;

        (element[2] as Button).clicked += async () => await SceneManager.LoadSceneAsync(0);
    }


    public void GameLost(VisualElement element)
    {
        Button button = element[0] as Button;
        button.text = "Load Last Save";
        button.RegisterCallback<ClickEvent>(loadGameMenu.Continue);

        button = element[1] as Button;
        button.style.display = DisplayStyle.Flex;
        button.RegisterCallback<ClickEvent>(loadGameMenu.OpenWindow);

        (element[2] as Button).clicked += async () => await SceneManager.LoadSceneAsync(0);

        UIRefs.PauseMenu.menuContainer.style.display = DisplayStyle.Flex;
        UIRefs.PauseMenu.menuContainer.pickingMode = PickingMode.Ignore;
        UIRefs.PauseMenu.menuContainer[0].style.display = DisplayStyle.None;
    }
}
