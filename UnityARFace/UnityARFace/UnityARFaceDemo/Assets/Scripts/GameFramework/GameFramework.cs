using System;

using UnityEngine;
using UnityEngine.SceneManagement;


public class GameFramework :  MonoBehaviour
{
    private static GameFramework m_instance;
    public static GameFramework Instance
    {
        get
        {
            return m_instance;
        }
    }

    public static bool useResources = true;

    public static bool UseStreamingAssets = false;

    public static bool CloseGame = false;

    public void Awake()
    {
        m_instance = this;

        Debug.Log("场景" + SceneManager.GetActiveScene().name + "加载完成！");

        GameObject.DontDestroyOnLoad(this);
        InitEngineSingleton();

        //Debug.unityLogger.logEnabled = false;

        //Input.backButtonLeavesApp = true;//设置返回键，是否退出程序。(系统默认为 false，所以不自己写方法是不会退出 App 的)

        Application.runInBackground = true;

        Input.multiTouchEnabled = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.orientation = ScreenOrientation.AutoRotation;

        SDKManager.Instance.Init();
        SDKHandler.Instance.Init();
        Init();

        //测试代码
        //SDKHandler.Instance.ResponseSceneName("TomatoBattle");
    }

    //创建EngineSingleton
    private void InitEngineSingleton()
    {
        AppObjConst.EngineSingletonGo = GameObject.Find(AppObjConst.EngineSingletonGoName);
        if (AppObjConst.EngineSingletonGo == null)
        {
            AppObjConst.EngineSingletonGo = new GameObject(AppObjConst.EngineSingletonGoName);
            GameObject.DontDestroyOnLoad(AppObjConst.EngineSingletonGo);
        }
    }

    private void Init()
    {
        InitBaseSys();
        AutoRegister();

        //LoadReporter();

        SDKManager.Instance.SendPlatformStartSenseComplete();
    }

    private void AutoRegister()
    {
        // ModuleMgr
        ModuleMgrRegister.AutoRegisterModel();
        ModuleMgrRegister.AutoRegisterCtrl();
        ModuleMgrRegister.AutoRegisterUICtrl();
    }

    protected void InitBaseSys()
    {
        Singleton<ResourcesManager>.Instance.Init();
        Singleton<UIManager>.Instance.Init();
        TimerMgr.Instance.Init();

        AudioManager.Instance.Init();

        if (!useResources)
        {
            ResourcesManager.Instance.LoadResourcePackerInfoSet();
        }
    }

    /// <summary>
    /// 加载log编辑器
    /// </summary>
    private void LoadReporter()
    {
        GameObject go = ResourcesManager.Instance.GetResource(PrefabPathConst.ReporterPath, typeof(GameObject), enResourceType.ScenePrefab, true).content as GameObject;
        GameObject reporter = GameObject.Instantiate(go);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            CloseGame = true;
        }

        try
        {
            Singleton<UIManager>.Instance.Update();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void OnDestroy()
	{
        m_instance = null;

        DestroyBaseSys();
    }

    protected void DestroyBaseSys()
    {
        Singleton<UIManager>.Instance.Dispose();
        Singleton<ResourcesManager>.Instance.Dispose();
        SDKManager.Instance.Dispose();
    }

    private void OnApplicationQuit()
    {
        CloseGame = true;

        Debug.Log("退出游戏！！！！！");
    }
}