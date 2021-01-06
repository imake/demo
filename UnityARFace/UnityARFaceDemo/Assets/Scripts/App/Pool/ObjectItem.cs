using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectItem
{
    /// <summary>
    /// 具体对象
    /// </summary>
    public GameObject gameObject;

    /// <summary>
    /// 存入时间
    /// </summary>
    public float aliveTime;

    /// <summary>
    /// 是否是销毁状态
    /// </summary>
    public bool isDestroyStatus;

    public ObjectItem(GameObject _gameObject)
    {
        this.gameObject = _gameObject;
        this.isDestroyStatus = false;
    }

    /// <summary>
    /// 激活显示对象
    /// </summary>
    /// <returns></returns>
    public GameObject Active()
    {
        this.gameObject.SetActive(true);
        this.isDestroyStatus = false;
        aliveTime = 0;
        return this.gameObject;
    }

    /// <summary>
    /// 回收对象，将对象隐藏
    /// </summary>
    public void Reclaim()
    {
        this.gameObject.SetActive(false);
        this.isDestroyStatus = true;
        aliveTime = Time.time;
    }

    /// <summary>
    /// 检查当前对象是否已超时
    /// </summary>
    /// <returns></returns>
    public bool isBeyondAliveTime()
    {
        if (!this.isDestroyStatus)
        {
            return false;
        }
        if (Time.time- PoolManager.Instance.aLiveTime>= 5)
        {
            Debug.Log("已超时！");
            return true;
        }
        return false;
    }
}
