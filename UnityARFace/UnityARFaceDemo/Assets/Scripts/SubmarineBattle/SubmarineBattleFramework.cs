using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineBattleFramework : MonoBehaviour
{
    void Awake()
    {
        ResourcesManager.Instance.Init();
        TimerMgr.Instance.Init();

        AppObjConst.GamePlayGo = new GameObject(AppObjConst.GamePlayGoName);
        GamePlayWorld world = AppObjConst.GamePlayGo.AddComponent<GamePlayWorld>();
    }

    void Update()
    {
        CreateManager.Instance.Update();
    }
}
