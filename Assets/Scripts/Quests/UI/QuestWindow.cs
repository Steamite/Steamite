using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

[UnityEngine.RequireComponent(typeof(QuestController))]
public class QuestWindow : FullscreenWindow
{
    IUIElement questCatalog;
    IUIElement orderCatalog;

    QuestController questController;
    private void Awake()
    {
        questController = GetComponent<QuestController>();
    }
    protected override void OnUIReload()
    {
        base.OnUIReload();

        questCatalog = Root.Q("QuestCatalog") as IUIElement;
        (Root[0][1] as Button).clicked += CloseWindow;

        orderCatalog = Root.Q("OrderInterface") as IUIElement;
    }
    protected override void OnDataLoadLogic()
    {
        throw new NotImplementedException();
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        questCatalog.Open(questController);

        orderCatalog.Open(questController.orderController);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
    }

}
