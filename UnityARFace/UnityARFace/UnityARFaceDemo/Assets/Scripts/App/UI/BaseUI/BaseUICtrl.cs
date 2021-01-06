
public abstract class BaseUICtrl : BaseCtrl
{
    protected UICtrlDispatcher uiCtrlDispatcher;

    protected override void Assignment()
    {
        base.Assignment();
        uiCtrlDispatcher = UICtrlDispatcher.Instance;
    }

    public abstract void OpenUI(object args = null);
    public abstract void CloseUI(object args = null);
}
