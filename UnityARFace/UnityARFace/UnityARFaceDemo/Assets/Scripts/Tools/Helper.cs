using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Helper
{
    public static void LookToCamera(GameObject obj)
    {
        //正方向朝向摄像头
        var cameraForward = Camera.main.transform.forward;
        var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
        obj.transform.rotation = Quaternion.LookRotation(cameraBearing);
    }

    /// <summary>
    /// 检测是否点击UI
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <returns></returns>
    public static bool IsPointerOverGameObject(Vector2 mousePosition)
    {
        //创建一个点击事件
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //向点击位置发射一条射线，检测是否点击UI
        EventSystem.current.RaycastAll(eventData, raycastResults);
        if (raycastResults.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 截图
    /// </summary>
    /// <returns></returns>
    public static byte[] GetScreenTexture(Camera camera, Rect rect)
    {
        // 创建一个RenderTexture对象
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 24);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
        camera.targetTexture = rt;
        camera.Render();
        //ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像。
        //ps: camera2.targetTexture = rt;
        //ps: camera2.Render();
        //ps: -------------------------------------------------------------------

        // 激活这个rt, 并从中中读取像素。
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
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

    public static void Hurt(int damage, GameObject go)
    {
        GameObject o = new GameObject("-x");
        o.transform.position = go.transform.position + Vector3.up/* * Mathf.Lerp(4f, 2f, HP / MaxHP)*/;
        o.transform.rotation = Camera.main.transform.rotation;
        TextMesh tm = o.AddComponent<TextMesh>();
        tm.text = damage.ToString("00");
        tm.characterSize = 0.3f;
        tm.color = new Color(0.9f, 0.3f, 0.3f, 1f);
        o.GetComponent<MeshRenderer>().sortingOrder = 1;
        GameObject.Destroy(o, 1f);
    }

    /// <summary>
    /// 固定数组中的不重复随机
    /// </summary>
    /// <param name="nums">数组</param>
    /// <param name="count">要随机的个数</param>
    /// <returns></returns>
    public static List<T> GetRandom<T>(this List<T> nums, int count)
    {
        if (count > nums.Count)
        {
            Debug.LogError("要取的个数大于数组长度！");
            return null;
        }

        List<T> result = new List<T>();
        List<int> id = new List<int>();

        for (int i = 0; i < nums.Count; i++)
        {
            id.Add(i);
        }

        int r;
        while (id.Count > nums.Count - count)
        {
            r = Random.Range(0, id.Count);
            result.Add(nums[id[r]]);
            id.Remove(id[r]);
        }
        return (result);
    }
}
