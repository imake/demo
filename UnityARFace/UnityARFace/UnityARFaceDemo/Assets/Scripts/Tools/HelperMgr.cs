using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperMgr : Singleton<HelperMgr>
{
    private RenderTexture rt;
    private Camera camera;
    private Rect rect;
    public void InitScreenTextureInfo(Camera _camera, Rect _rect)
    {
        camera = _camera;
        rect = _rect;
        // 创建一个RenderTexture对象
        rt = new RenderTexture((int)rect.width, (int)rect.height, 24);
    }

    /// <summary>
    /// 截图
    /// </summary>
    /// <returns></returns>
    public byte[] GetScreenTexture()
    {
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
        camera.targetTexture = rt;
        camera.Render();
        // 激活这个rt, 并从中中读取像素。
        RenderTexture.active = rt;

        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        GameObject.Destroy(rt);
        // 最后将这些纹理数据，成一个png图片文件
        byte[] bytes = screenShot.EncodeToPNG();

        return bytes;
    }
}
