using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    /// <summary>
    /// 超时时间
    /// </summary>
    public int aLiveTime = 5 * 60;

    /// <summary>
    /// 对象池
    /// </summary>
    public Dictionary<string, PoolItem> poolDic;

    /// <summary>
    /// 添加一个对象组
    /// </summary>
    /// <param name="_name"></param>
    public void PushData(string _name)
    {
        if (poolDic==null)
        {
            poolDic = new Dictionary<string, PoolItem>();
        }
        if (!poolDic.ContainsKey(_name))
        {
            poolDic.Add(_name,new PoolItem(_name));
        }
    }

    /// <summary>
    /// 添加单个对象（首先寻找对象组-》添加单个对象）
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_gameObject"></param>
    public void PushObject(string _name,GameObject _gameObject)
    {
        if (poolDic==null||!poolDic.ContainsKey(_name))
        {
            PushData(_name);
        }
        poolDic[_name].PushObject(_gameObject);
    }

    /// <summary>
    /// 销毁单个对象，
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_gameObject"></param>
    public void RemoveObject(string _name,GameObject _gameObject)
    {
        if (poolDic==null||!poolDic.ContainsKey(_name))
        {
            return;
        }
        poolDic[_name].RemoveObject(_gameObject);
    }

    /// <summary>
    /// 获取对应缓存池中得对象
    /// </summary>
    /// <param name="_name"></param>
    /// <returns></returns>
    public GameObject GetObject(string _name)
    {
        if (poolDic==null||!poolDic.ContainsKey(_name))
        {
            return null;
        }
        return poolDic[_name].GetObject();
    }

    /// <summary>
    /// 回收对应对象池的对象
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_gameObject"></param>
    public void ReclaimActiveObject(string _name,GameObject _gameObject)
    {
        if (poolDic==null||!poolDic.ContainsKey(_name))
        {
            return;
        }
        poolDic[_name].ReclaimObject(_gameObject);
    }

    /// <summary>
    /// 处理超时，在Update中调用
    /// </summary>
    public void CheckBeyondTimeObject()
    {
        if (poolDic==null)
        {
            return;
        }
        foreach (var item in poolDic.Values)
        {
            item.CheckBeyondObject();
        }
    }

    /// <summary>
    /// 销毁所有对象
    /// </summary>
    public void Destory()
    {
        if (poolDic==null)
        {
            return;
        }
        foreach (var item in poolDic.Values)
        {
            item.DestoryAll();
        }
        poolDic.Clear();
    }

    public override void Dispose()
    {
        base.Dispose();

        Destory();
        Debug.Log("PoolManager Dispose!!!");
    }
}
