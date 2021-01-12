﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmarineBattleFramework : MonoBehaviour
{
    public Text numberText;
    public Text integralText;

    public Button reStartBtn;


    void Awake()
    {
        ResourcesManager.Instance.Init();
        TimerMgr.Instance.Init();

        AppObjConst.GamePlayGo = new GameObject(AppObjConst.GamePlayGoName);
        GamePlayWorld world = AppObjConst.GamePlayGo.AddComponent<GamePlayWorld>();

        reStartBtn.onClick.AddListener(OnReStart);
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
            numberText.text = BattleDataMgr.Instance.obstacleCount.ToString();
        }
        if (integralText!=null)
        {
            integralText.text = BattleDataMgr.Instance.integral.ToString();
        }
    }
}
