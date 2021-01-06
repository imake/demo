
using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTimer : MonoBehaviour
{
    private Dictionary<Action, float> mIntervalDic = new Dictionary<Action, float>();
    private List<Action> triggers = new List<Action>();

    private void Update()
    {
        if (mIntervalDic.Count > 0)
        {
            triggers.Clear();
            foreach (KeyValuePair<Action, float> KeyValue in mIntervalDic)
            {
                if (KeyValue.Value <= Time.time)
                {
                    triggers.Add(KeyValue.Key);
                }
            }
            for (int i = 0; i < triggers.Count; i++)
            {
                Action func = triggers[i];
                mIntervalDic.Remove(func);

                func();
            }
        }
    }

    public void AddTimer(float interval, Action func)
    {
        if (null != func)
        {
            if (interval <= 0)
            {
                func();
                return;
            }
            mIntervalDic[func] = Time.time + interval;
        }
    }

    public void RemoveTimer(Action func)
    {
        if (null != func)
        {
            if (mIntervalDic.ContainsKey(func))
            {
                mIntervalDic.Remove(func);
            }
        }
    }

    public void ClearAll()
    {
        mIntervalDic.Clear();
        triggers.Clear();
    }

    public void Destroy()
    {
        Destroy(this);
    }

    void OnDestroy()
    {
        ClearAll();
        mIntervalDic = null;
        triggers = null;
    }
}
