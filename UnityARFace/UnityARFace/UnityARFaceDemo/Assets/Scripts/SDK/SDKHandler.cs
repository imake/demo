using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SDKHandler : SingletonMono<SDKHandler>
{
    #region Base
    protected override string ParentRootName
    {
        get
        {
            return AppObjConst.EngineSingletonGoName;
        }
    }

    protected override void New()
    {
        base.New();
    }
    #endregion

    public void Init()
    {
        //UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 接受要切换的场景名称
    /// </summary>
    public void ResponseSceneName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.Log("ResponseSceneName == null");
            return;
        }

        Debug.Log("LoadScene===" + name);

        SceneManager.LoadScene(name);
    }

    /// <summary>
    /// 接收原生传的人脸图片
    /// </summary>
    /// <param name="textureBytes"></param>
    public void ResponseFaceTexture(string textureBytes)
    {
        if (TextureBlendSceneFramework.Instance == null)
        {
            return;
        }

        Texture2D imageTextureMouth=null, imageTextureLeftEye=null, imageTextureRightEye=null;
        string[] str = textureBytes.Split(',');
        Debug.Log("str的长度=" + str.Length);
        if (str.Length > 0)
        {
            byte[] byteMouthData = Convert.FromBase64String(str[0]);

            if (byteMouthData == null || byteMouthData.Length == 0)
            {
                Debug.Log("ResponseFaceTexture 接收到嘴巴图片Data为空");
                return;
            }
            //bytes转成Texture
            imageTextureMouth = new Texture2D(128, 128, TextureFormat.RGB24, false);
            imageTextureMouth.LoadImage(byteMouthData);
            imageTextureMouth.Apply();

            if (str.Length > 1)
            {
                byte[] byteLeftEyeData = Convert.FromBase64String(str[1]);

                if (byteLeftEyeData == null || byteLeftEyeData.Length == 0)
                {
                    Debug.Log("ResponseFaceTexture 接收到左眼图片Data为空");
                    return;
                }
                //bytes转成Texture
                imageTextureLeftEye = new Texture2D(64, 64, TextureFormat.RGB24, false);
                imageTextureLeftEye.LoadImage(byteLeftEyeData);
                imageTextureLeftEye.Apply();

                if (str.Length > 2)
                {
                    byte[] byteRightEyeData = Convert.FromBase64String(str[2]);

                    if (byteRightEyeData == null || byteRightEyeData.Length == 0)
                    {
                        Debug.Log("ResponseFaceTexture 接收到右眼图片Data为空");
                        return;
                    }
                    //bytes转成Texture
                    imageTextureRightEye = new Texture2D(64, 64, TextureFormat.RGB24, false);
                    imageTextureRightEye.LoadImage(byteRightEyeData);
                    imageTextureRightEye.Apply();
                }
            }
        }
        TextureBlendSceneFramework.Instance.TextureBlend(imageTextureMouth, imageTextureLeftEye, imageTextureRightEye);
        Destroy(imageTextureMouth);
        Destroy(imageTextureLeftEye);
        Destroy(imageTextureRightEye);
    }

    int x;
    float y, z;
    /// <summary>
    /// 橘子模型接受人脸旋转角度
    /// </summary>
    /// <param name="strAngle"></param>
    public void ResponseFaceRotation(string strAngle)
    {
        if (TextureBlendSceneFramework.Instance == null)
        {
            return;
        }

        string[] angles = strAngle.Split(',');

        if (angles.Length>0)
        {
            x = Int32.Parse(angles[0]);
            if (angles.Length > 1)
            {
                y = float.Parse(angles[1]);
                if (angles.Length > 2)
                {
                    z = float.Parse(angles[2]);
                }
            }
        }

        TextureBlendSceneFramework.Instance.Rotate(x,y,z);
    }

    /// <summary>
    /// 潜水艇游戏接收人脸位置
    /// </summary>
    /// <param name="posRate"></param>
    public void ResponseFacePosition(float posRate)
    {

    }
}