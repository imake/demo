using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolItem
{
    /// <summary>
    /// 对象池名称，作标识
    /// </summary>
    public string name;

    /// <summary>
    /// 对象列表，存储同一个名称的所有对象
    /// </summary>
    public Dictionary<int, ObjectItem> objectDic;

    public PoolItem(string _name)
    {
        this.name = _name;
        this.objectDic = new Dictionary<int, ObjectItem>();
    }

    /// <summary>
    /// 往对象池里添加对象
    /// </summary>
    /// <param name="_gameObject"></param>
    public void PushObject(GameObject _gameObject)
    {
        int hashKey = _gameObject.GetHashCode();
        if (!this.objectDic.ContainsKey(hashKey))
        {
            this.objectDic.Add(hashKey, new ObjectItem(_gameObject));
        }
        else
        {
            this.objectDic[hashKey].Active();
        }
    }

    /// <summary>
    /// 回收对象，没有真正意义的销毁
    /// </summary>
    /// <param name="_gameObject"></param>
    public void ReclaimObject(GameObject _gameObject)
    {
        int hasKey = _gameObject.GetHashCode();
        if (this.objectDic.ContainsKey(hasKey))
        {
            this.objectDic[hasKey].Reclaim();
        }
        else
        {
            Debug.Log("当前对象池："+name+"不存在"+ _gameObject.name+"，回收不了该对象，直接删除");
            GameObject.Destroy(_gameObject);
        }
    }

    /// <summary>
    /// 从对象池获取一个对象
    /// </summary>
    /// <returns></returns>
    public GameObject GetObject()
    {
        if (this.objectDic==null||this.objectDic.Count==0)
        {
            return null;
        }
        foreach (var item in objectDic.Values)
        {
            if (item.isDestroyStatus)
            {
                return item.Active();
            }
        }
        return null;
    }

    /// <summary>
    /// 销毁对象（真正不回收销毁）
    /// </summary>
    /// <param name="_gameObject"></param>
    public void RemoveObject(GameObject _gameObject)
    {
        int hasKey = _gameObject.GetHashCode();
        if (this.objectDic.ContainsKey(hasKey))
        {
            this.objectDic.Remove(hasKey);
        }
        GameObject.Destroy(_gameObject);
    }

    /// <summary>
    /// 把所有对象销毁
    /// </summary>
    public void DestoryAll()
    {
        List<ObjectItem> objectList = new List<ObjectItem>();
        foreach (var item in this.objectDic.Values)
        {
            objectList.Add(item);
        }
        while(objectList.Count>0)
        {
            if (objectList[0]!=null&& objectList[0].gameObject!=null)
            {
                GameObject.Destroy(objectList[0].gameObject);
                objectList.Remove(objectList[0]);
            }
            else
            {
                objectList.Remove(objectList[0]);
            }
        }
        objectDic.Clear();
    }

    /// <summary>
    /// 检测超时，超时就销毁
    /// </summary>
    public void CheckBeyondObject()
    {
        List<ObjectItem> items = new List<ObjectItem>();
        foreach (var item in objectDic.Values)
        {
            if (item.isBeyondAliveTime())
            {
                items.Add(item);
            }
        }
        for (int i = 0; i < items.Count; i++)
        {
            this.RemoveObject(items[i].gameObject);
        }
    }
}
