using UnityEngine;
using UnityEngine.UI;

public enum UILayerType : int
{
    None = 0,
    Normal = 1,
    Top = 2,
    Highest = 3,
    Animation = 4,
    Tips = 5,
    Loading = 6,
    System = 7,
}

public enum UIType : int
{
    NormalUI = 0,
    FullScreenUI = 1,
    ConfirmationUI = 2,
}

public class UIInfo
{
    public UILayerType layerType = UILayerType.Normal;
    public UIType uiType = UIType.NormalUI;

    public uint openMsg;
    public uint closeMsg;

    // 是否缓存
    public bool isCache = false;
    // 是否切换场景关闭UI
    public bool isSwitchSceneCloseUI = false;
    // 是否有Update函数
    public bool isTickUpdate = false;
    // 是否需要UI动画
    public bool isNeedUIAnimation = false;
}

public abstract class BaseUI
{
    #region Field
    public string uiName;
    public string path;
    public UIInfo uiInfo;
    public bool isClose = true;

    public BaseUICtrl baseUICtrl;
    protected ModelDispatcher modelDispatcher;
    public GameObject baseGObj;

    public Canvas canvas;
    public CanvasScaler canvasScaler;

    #endregion

    #region Constructor
    public BaseUI(BaseUICtrl baseUICtrl)
    {
        this.baseUICtrl = baseUICtrl;
        modelDispatcher = ModelDispatcher.Instance;

        Process_Init();
    }
    #endregion

    #region Interface
    public void Open(object args = null)
    {
        UIManager.Instance.OpenUI(this, args);
    }
    public void Close()
    {
        UIManager.Instance.CloseUI(this);
    }
    public void Display(object args = null)
    {
        UIManager.Instance.DisplayUI(this, args);
    }
    public void Hide()
    {
        UIManager.Instance.HideUI(this);
    }
    #endregion

    #region Process
    private void Process_Init()
    {
        uiInfo = new UIInfo();
        SetUIInfo(uiInfo);
        PostProcess_UIInfo();

        OnInit();
    }
    private void PostProcess_UIInfo()
    {
    }
    public void Process_Bind()
    {
        OnBind();
        OnBindAddListener();
    }
    public void Process_OpenBefore(object args)
    {
        OnOpenBefore(args);
    }
    public void Process_Open(object args)
    {
        AddListener();
        OnOpen(args);
    }
    public void Process_OpenUIAnimEnd()
    {
        OnOpenUIAnimEnd();
    }
    public void Process_CloseUIAnimEnd()
    {
        OnCloseUIAnimEnd();
    }
    public void Process_Close()
    {
        RemoveListener();
        OnClose();
    }
    public void Process_Destroy()
    {
        OnDestroy();
    }
    public void Process_Display(object args)
    {
        OnDisplay(args);
    }
    public void Process_Hide()
    {
        OnHide();
    }
    #endregion

    #region Logic
    public virtual void Update() { }
    protected abstract void SetUIInfo(UIInfo uiInfo);

    protected abstract void OnInit();
    protected abstract void OnBind();
    protected abstract void OnBindAddListener();
    protected abstract void OnOpenBefore(object args);
    protected abstract void OnOpen(object args);
    protected virtual void OnOpenUIAnimEnd() { }
    protected abstract void OnClose();
    protected virtual void OnCloseUIAnimEnd() { }
    protected virtual void OnDestroy() { }
    protected abstract void OnHide();
    protected abstract void OnDisplay(object args);

    protected abstract void AddListener();
    protected abstract void RemoveListener();
    #endregion
}