
public abstract class BaseCtrl
{
    public string ctrlName;
    protected CtrlDispatcher ctrlDispatcher;
    protected ModelDispatcher modelDispatcher;
    //protected WSNetDispatcher wsNetDispatcher;

    public BaseCtrl()
    {
        Init();
    }

    protected virtual void Assignment()
    {
        ctrlDispatcher = CtrlDispatcher.Instance;
        modelDispatcher = ModelDispatcher.Instance;
        //wsNetDispatcher = WSNetDispatcher.Instance;
    }

    protected virtual void Init()
    {
        Assignment();
        AddListener();
        AddServerListener();
        OnInit();
    }
    public virtual void Dispose()
    {
        RemoveListener();
        RemoveServerListener();
        OnDispose();
    }

    protected abstract void OnInit();
    protected abstract void OnDispose();

    protected abstract void AddListener();
    protected abstract void RemoveListener();
    protected abstract void AddServerListener();
    protected abstract void RemoveServerListener();
}