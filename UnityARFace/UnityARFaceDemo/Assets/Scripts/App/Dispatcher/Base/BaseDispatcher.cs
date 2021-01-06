using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDispatcher<T, Msg, Param> : IDisposable
    where T : class, new()
    where Param : class
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new T();
            }
            return m_instance;
        }
    }

    public delegate void OnEvent(Param param);
    private Dictionary<Msg, List<OnEvent>> m_msgPriorityDic = new Dictionary<Msg, List<OnEvent>>();
    private Dictionary<Msg, List<OnEvent>> m_msgDic = new Dictionary<Msg, List<OnEvent>>();
    private Dictionary<Msg, List<OnEvent>> m_msgOnceDic = new Dictionary<Msg, List<OnEvent>>();

    public void AddPriorityListener(Msg msgId, OnEvent listener)
    {
        if (m_msgPriorityDic.ContainsKey(msgId))
        {
            m_msgPriorityDic[msgId].Add(listener);
        }
        else
        {
            List<OnEvent> list = new List<OnEvent>();
            list.Add(listener);
            m_msgPriorityDic.Add(msgId, list);
        }
    }

    public void AddListener(Msg msgId, OnEvent listener)
    {
        if (m_msgDic.ContainsKey(msgId))
        {
            m_msgDic[msgId].Add(listener);
        }
        else
        {
            List<OnEvent> list = new List<OnEvent>();
            list.Add(listener);
            m_msgDic.Add(msgId, list);
        }
    }

    public void AddOnceListener(Msg msgId, OnEvent listener)
    {
        if (m_msgOnceDic.ContainsKey(msgId))
        {
            m_msgOnceDic[msgId].Add(listener);
        }
        else
        {
            List<OnEvent> list = new List<OnEvent>();
            list.Add(listener);
            m_msgOnceDic.Add(msgId, list);
        }
    }

    public void RemovePriorityListener(Msg msgId, OnEvent listener)
    {
        if (m_msgPriorityDic.ContainsKey(msgId))
        {
            List<OnEvent> list = m_msgPriorityDic[msgId];
            list.Remove(listener);
            if (list.Count == 0)
            {
                m_msgPriorityDic.Remove(msgId);
            }
        }
    }

    public void RemoveListener(Msg msgId, OnEvent listener)
    {
        if (m_msgDic.ContainsKey(msgId))
        {
            List<OnEvent> list = m_msgDic[msgId];
            list.Remove(listener);
            if (list.Count == 0)
            {
                m_msgDic.Remove(msgId);
            }
        }
    }

    public void RemoveOnceListener(Msg msgId, OnEvent listener)
    {
        if (m_msgOnceDic.ContainsKey(msgId))
        {
            List<OnEvent> list = m_msgOnceDic[msgId];
            list.Remove(listener);
            if (list.Count == 0)
            {
                m_msgOnceDic.Remove(msgId);
            }
        }
    }

    public void Dispatch(Msg msgId, Param param = null)
    {
        InvokeMethods(m_msgPriorityDic, msgId, param);
        InvokeMethods(m_msgDic, msgId, param);
        InvokeMethods(m_msgOnceDic, msgId, param);

        if (m_msgOnceDic.ContainsKey(msgId))
        {
            m_msgOnceDic.Remove(msgId);
        }
    }

    private void InvokeMethods(Dictionary<Msg, List<OnEvent>> msgDict, Msg msgId, Param param)
    {
        if (!msgDict.ContainsKey(msgId)) return;

        List<OnEvent> rawList = msgDict[msgId];
        int funcCount = rawList.Count;

        if (funcCount == 1)
        {
            OnEvent onEvent = rawList[0];
            onEvent(param);
            return;
        }

        List<OnEvent> invokeFuncs = new List<OnEvent>();
        invokeFuncs.AddRange(rawList);
        for (int i = 0; i < funcCount; i++)
        {
            try
            {
                OnEvent onEvent = invokeFuncs[i];
                onEvent(param);
            }
            catch (Exception e) { Debug.LogError(e); }
        }
    }

    public void Clear()
    {
        m_msgPriorityDic.Clear();
        m_msgDic.Clear();
        m_msgOnceDic.Clear();
    }

    public void Dispose()
    {
        m_instance = null;

        m_msgPriorityDic.Clear();
        m_msgDic.Clear();
        m_msgOnceDic.Clear();
        m_msgPriorityDic = null;
        m_msgDic = null;
        m_msgOnceDic = null;
    }
}
