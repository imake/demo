using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubmarineBattleFramework : MonoBehaviour
{
    private static SubmarineBattleFramework m_instance;

    public static SubmarineBattleFramework Instance
    {
        get
        {
            return m_instance;
        }
    }

    public Text numberText;
    public Text integralText;

    public Button reStartBtn;

    public RawImage myImage;
    public RawImage otherImage;

    public Camera arCamera;

    void Awake()
    {
        m_instance = this;

        Debug.Log("场景" + SceneManager.GetActiveScene().name + "加载完成！");

        //SDKManager.Instance.SendSetUnityViewUpToIosView();

        ResourcesManager.Instance.Init();
        //TimerMgr.Instance.Init();

        AppObjConst.GamePlayGo = new GameObject(AppObjConst.GamePlayGoName);
        GamePlayWorld world = AppObjConst.GamePlayGo.AddComponent<GamePlayWorld>();
        AppObjConst.GamePlayGo.transform.position = new Vector3(AppObjConst.GamePlayGo.transform.position.x, AppObjConst.GamePlayGo.transform.position.y, 1);
        AppObjConst.GamePlayGo.transform.localScale = new Vector3(1f,1f,1f);

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
        AppObjConst.GamePlayGo.transform.position = new Vector3(AppObjConst.GamePlayGo.transform.position.x, AppObjConst.GamePlayGo.transform.position.y, 1);
        AppObjConst.GamePlayGo.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    void Update()
    {
        CreateManager.Instance.Update();

        if (BattleDataMgr.Instance == null)
        {
            return;
        }

        if (BattleDataMgr.Instance.isGameOver)
        {
            BlendTexture();
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

    private Texture2D newTexture;
    public void BlendTexture()
    {
        BattleDataMgr.Instance.battleTexture = HelperTools.GetScreenTexture(Camera.main, new Rect(0, 0, 480, 640));
        BattleDataMgr.Instance.arTexture = HelperTools.GetScreenTexture(arCamera, new Rect(0, 0, 480, 640));

        int battle_width = BattleDataMgr.Instance.battleTexture.width;
        int battle_height = BattleDataMgr.Instance.battleTexture.height;

        int ar_width = BattleDataMgr.Instance.arTexture.width;
        int ar_height = BattleDataMgr.Instance.arTexture.height;

        Destroy(newTexture);
        newTexture = Instantiate(BattleDataMgr.Instance.arTexture);

        for (int i = 0; i < battle_width; i++)
        {
            for (int j = 0; j < battle_height; j++)
            {
                Color battleColor = BattleDataMgr.Instance.battleTexture.GetPixel(i, j);
                Color arColor = BattleDataMgr.Instance.arTexture.GetPixel(i, j);

                if (battleColor.a <= 0.05f)
                {
                    //newTexture.SetPixel(i, j, arColor);
                }
                else
                {
                    newTexture.SetPixel(i, j, battleColor);
                }

            }
        }

        newTexture.Apply();

        myImage.texture = newTexture;

        byte[] bytes = newTexture.EncodeToPNG();

        SDKManager.Instance.RefreshSubmarineWithBytes(bytes);

        Destroy(BattleDataMgr.Instance.battleTexture);
        Destroy(BattleDataMgr.Instance.arTexture);
    }

    public void RefreshOtherImage(Texture2D texture)
    {
        //Debug.Log("RefreshOtherImage 宽度=" + texture.width);
        otherImage.texture = texture;
    }

}
