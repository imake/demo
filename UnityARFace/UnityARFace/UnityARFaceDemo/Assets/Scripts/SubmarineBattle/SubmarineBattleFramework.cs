using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubmarineBattleFramework : MonoBehaviour
{
    public Text numberText;
    public Text integralText;

    public Button reStartBtn;


    void Awake()
    {
        Debug.Log("场景" + SceneManager.GetActiveScene().name + "加载完成！");

        SDKManager.Instance.SendSetUnityViewUpToIosView();

        //ResourcesManager.Instance.Init();
        //TimerMgr.Instance.Init();

        AppObjConst.GamePlayGo = new GameObject(AppObjConst.GamePlayGoName);
        GamePlayWorld world = AppObjConst.GamePlayGo.AddComponent<GamePlayWorld>();

        reStartBtn.onClick.AddListener(OnReStart);

        //Debug.Log("width=" + Screen.width + " height=" + Screen.height);
    }

    private void OnReStart()
    {
        reStartBtn.gameObject.SetActive(false);
        GameObject.Destroy(AppObjConst.GamePlayGo);
        BattleDataMgr.Instance.ResetData();
        CreateManager.Instance.RemoveAllEntity();

        AppObjConst.GamePlayGo = new GameObject(AppObjConst.GamePlayGoName);
        GamePlayWorld world = AppObjConst.GamePlayGo.AddComponent<GamePlayWorld>();
    }

    void Update()
    {
        CreateManager.Instance.Update();

        if (BattleDataMgr.Instance == null)
        {
            return;
        }

        if (numberText!=null)
        {
            numberText.text = "数量:" + BattleDataMgr.Instance.obstacleCount;
        }
        if (integralText!=null)
        {
            integralText.text = "积分:" + BattleDataMgr.Instance.integral;
        }
    }
}
