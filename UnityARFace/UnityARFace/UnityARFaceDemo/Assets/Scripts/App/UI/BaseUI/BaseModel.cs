public abstract class BaseModel
{
    public string modelName;
    private string localStorageKey;
    protected ModelDispatcher modelDispatcher;

    public BaseModel()
    {
        modelDispatcher = ModelDispatcher.Instance;
        Init();
    }

    private void Init()
    {
        AddListener();
        OnInit();
    }
    public void Dispose()
    {
        RemoveListener();
        OnDispose();
    }
    public void Reset()
    {
        Dispose();
        Init();
    }
    public void ReadData()
    {
        OnReadData();
    }

    protected abstract void OnInit();
    protected abstract void OnDispose();
    protected abstract void OnReset();
    protected virtual void OnReadData() { }

    protected abstract void AddListener();
    protected abstract void RemoveListener();
}
