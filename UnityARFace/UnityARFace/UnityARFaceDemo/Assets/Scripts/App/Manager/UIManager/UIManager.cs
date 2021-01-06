using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private GameObject uiRoot;

    private EventSystem uiEventSystem;

    private List<BaseUI> existDynamicUIs = new List<BaseUI>();

    private List<BaseUI> tickUpdateUIs = new List<BaseUI>();

    private Vector2 referenceResolution = new Vector2(1080, 1920);

    private string[] uiTypes = new string[] { "[Normal]", "[Top]", "[Highest]", "[Animation]", "[Tips]", "[Loading]", "[System]", };

    private Camera uiCamera;

    public Camera UICamera
    {
        get
        {
            return uiCamera;
        }
    }

    public override void Init()
    {
        base.Init();

        Debug.Log("[UIManager]Init");

        CreateUIRoot();
        CreateEventSystem();
        CreateCamera();
    }

    public override void Dispose()
    {
        base.Dispose();

        existDynamicUIs.Clear();
        tickUpdateUIs.Clear();
        existDynamicUIs = null;
        tickUpdateUIs = null;
    }

    /// <summary>
    /// 创建UI根节点
    /// </summary>
    private void CreateUIRoot()
    {
        uiRoot = new GameObject(AppObjConst.UIManagerGoName);
        GameObject obj2 = GameObject.Find(AppObjConst.ApplicationGoName);
        if (obj2 != null)
        {
            uiRoot.transform.parent = obj2.transform;
        }

        for (int i = 0; i < uiTypes.Length; i++)
        {
            if (!string.IsNullOrEmpty(uiTypes[i]))
            {
                GameObject go = new GameObject(uiTypes[i]);
                go.transform.SetParent(uiRoot.transform);
            }
        }
    }

    /// <summary>
    /// 创建UI事件系统
    /// </summary>
    private void CreateEventSystem()
    {
        uiEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
        if (uiEventSystem == null)
        {
            GameObject obj2 = new GameObject("EventSystem");
            uiEventSystem = obj2.AddComponent<EventSystem>();
            obj2.AddComponent<StandaloneInputModule>();
        }
        uiEventSystem.gameObject.transform.parent = uiRoot.transform;
    }

    /// <summary>
    /// 创建渲染UI所需的摄像机
    /// </summary>
    private void CreateCamera()
    {
        GameObject obj2 = new GameObject(AppObjConst.UICameraName);
        obj2.transform.SetParent(uiRoot.transform, true);
        obj2.transform.localPosition = Vector3.zero;
        obj2.transform.localRotation = Quaternion.identity;
        obj2.transform.localScale = Vector3.one;
        Camera camera = obj2.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 50f;
        camera.clearFlags = CameraClearFlags.Depth;
        camera.cullingMask = 32;
        camera.depth = 10f;
        uiCamera = camera;
    }

    public void CloseAllUI()
    {
        for (int i = existDynamicUIs.Count - 1; i >= 0; i--)
        {
            BaseUI ui = existDynamicUIs[i];
            CloseUI(ui);
        }
    }

    public void DisposeAllUI()
    {
        for (int i = existDynamicUIs.Count - 1; i >= 0; i--)
        {
            BaseUI ui = existDynamicUIs[i];
            CloseUI(ui);
        }
    }

    public void SwitchSceneCloseAllUI()
    {
        for (int i = existDynamicUIs.Count - 1; i >= 0; i--)
        {
            BaseUI ui = existDynamicUIs[i];
            if (ui.uiInfo.isSwitchSceneCloseUI)
            {
                CloseUI(ui);
            }
        }
    }

    /// <summary>
    /// 打开UI
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="args"></param>
    public void OpenUI(BaseUI ui, object args = null)
    {
        ui.isClose = false;
        LoadUI(ui, args, OpenUIProcess);
    }

    private void OpenUIProcess(BaseUI ui, object args)
    {
        existDynamicUIs.Add(ui);

        ui.Process_Bind();
        ui.Process_OpenBefore(args);
        ui.Process_Open(args);

        if (ui.uiInfo.isTickUpdate)
        {
            tickUpdateUIs.Add(ui);
        }
        if (ui.uiInfo.isNeedUIAnimation)
        {
            //OpenUIAnim(ui);
        }
    }

    public void CloseUI(BaseUI ui)
    {
        if (existDynamicUIs.Contains(ui))
        {
            existDynamicUIs.Remove(ui);

            if (ui.uiInfo.isTickUpdate)
            {
                tickUpdateUIs.Remove(ui);
            }

            ui.Process_Close();

            if (!ui.uiInfo.isNeedUIAnimation)
            {
                DestroyUI(ui);
            }
            else
            {
                //CloseUIAnim(ui, () => DestroyUI(ui));
            }
        }
    }

    public void DisplayUI(BaseUI ui, object args)
    {
        ui.baseGObj.SetActive(true);
        ui.Process_Display(args);
    }

    public void HideUI(BaseUI ui)
    {
        ui.baseGObj.SetActive(false);
        ui.Process_Hide();
    }

    private void LoadUI(BaseUI ui, object args, Action<BaseUI, object> func)
    {
        GameObject gameObject = CreateForm(ui.path);
        if (gameObject == null)
        {
            Debug.Log("UIgameObject {0} is NUll!!!" + ui.path);
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        string formName = GetFormName(ui.path);
        gameObject.name = formName;

        ui.baseGObj = gameObject;
        ui.canvas = gameObject.GetComponent<Canvas>();
        ui.canvas.worldCamera = uiCamera;
        ui.canvasScaler = gameObject.GetComponent<CanvasScaler>();
        SetCanvas(ui);
        MatchScreen(ui);
        SetSortingLayer(ui);

        func(ui, args);
    }

    /// <summary>
    /// 创建UI
    /// </summary>
    /// <param name="formPrefabPath">窗体路径</param>
    /// <returns>对象</returns>
    private GameObject CreateForm(string path)
    {
        GameObject gameObject = null;
        if (gameObject == null)
        {
            GameObject go = (GameObject)Singleton<ResourcesManager>.Instance.GetResource(path, typeof(GameObject), enResourceType.UIForm, false, false).content;
            if (go == null)
            {
                Debug.Log("UIgameobject {0} is NUll!!!" + path);
                return null;
            }
            gameObject = GameObject.Instantiate(go);
        }
        return gameObject;
    }

    /// <summary>
    /// 根据UI路径得到UI名字
    /// </summary>
    /// <param name="formPath">路径</param>
    /// <returns>名字</returns>
    private string GetFormName(string formPath)
    {
        return FileManager.EraseExtension(FileManager.GetFullName(formPath));
    }

    private void DestroyUI(BaseUI ui)
    {
        try
        {
            UnityEngine.Object.Destroy(ui.baseGObj);
        }
        catch (Exception ex)
        {
            Debug.AssertFormat(false, "Error destroy {0} formScript gameObject: message: {1}, callstack: {2}", new object[]
            {
                    ui.path,
                    ex.Message,
                    ex.StackTrace
            });
        }

        ui.isClose = true;
        ui.baseGObj = null;

        ui.Process_Destroy();
    }

    public void Update()
    {
        for (int i = tickUpdateUIs.Count - 1; i >= 0; i--)
        {
            BaseUI ui = tickUpdateUIs[i];
            if (!ui.uiInfo.isTickUpdate)
                continue;
            ui.Update();
        }
    }

    public void SetCanvas(BaseUI ui)
    {
        if (ui.canvas != null)
        {
            if (ui.canvas.worldCamera == null)
            {
                if (ui.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    ui.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }
            else if (ui.canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                ui.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }
            ui.canvas.pixelPerfect = true;
        }
    }

    public void MatchScreen(BaseUI ui)
    {
        if (ui.canvasScaler == null)
        {
            return;
        }
        ui.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ui.canvasScaler.referenceResolution = referenceResolution;
        ui.canvasScaler.matchWidthOrHeight = 0f;
        //if ((float)Screen.width / ui.canvasScaler.referenceResolution.x > (float)Screen.height / ui.canvasScaler.referenceResolution.y)
        //{
        //    if (ui.uiInfo.uiType == UIType.FullScreenUI)
        //    {
        //        ui.canvasScaler.matchWidthOrHeight = 0f;
        //    }
        //    else
        //    {
        //        ui.canvasScaler.matchWidthOrHeight = 1f;
        //    }
        //}
        //else if (ui.uiInfo.uiType == UIType.FullScreenUI)
        //{
        //    ui.canvasScaler.matchWidthOrHeight = 1f;
        //}
        //else
        //{
        //    ui.canvasScaler.matchWidthOrHeight = 0f;
        //}
    }

    public void SetSortingLayer(BaseUI ui)
    {
        GameObject go = null;
        switch (ui.uiInfo.layerType)
        {
            case UILayerType.None:
                break;
            case UILayerType.Normal:
                go = uiRoot.transform.Find("[Normal]").gameObject;
                break;
            case UILayerType.Top:
                go = uiRoot.transform.Find("[Top]").gameObject;
                break;
            case UILayerType.Highest:
                go = uiRoot.transform.Find("[Highest]").gameObject;
                break;
            case UILayerType.Animation:
                go = uiRoot.transform.Find("[Animation]").gameObject;
                break;
            case UILayerType.Tips:
                go = uiRoot.transform.Find("[Tips]").gameObject;
                break;
            case UILayerType.Loading:
                go = uiRoot.transform.Find("[Loading]").gameObject;
                break;
            case UILayerType.System:
                go = uiRoot.transform.Find("[System]").gameObject;
                break;
        }
        if (go!=null)
        {
            ui.baseGObj.transform.SetParent(go.transform);
        }
        else
        {
            ui.baseGObj.transform.SetParent(uiRoot.transform);
        }

        ui.canvas.sortingLayerName = ui.uiInfo.layerType.ToString();
    }
}
