using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnProtoMsgEvent(BaseS2CJsonProto protoMsg);

public class WSNetDispatcher : SingletonMono<WSNetDispatcher>
{
    private int maxHandleCount = 10;

    private Queue<BaseS2CJsonProto> protoMsgQueue = new Queue<BaseS2CJsonProto>();
    private BaseS2CJsonProto[] protoMsgDispatchArray;
    private int currNeedHandleCount;
    private List<string> logIgnoreList;

    private Dictionary<string, List<OnProtoMsgEvent>> protoPriorityMsgDict = new Dictionary<string, List<OnProtoMsgEvent>>();
    private Dictionary<string, List<OnProtoMsgEvent>> protoMsgDict = new Dictionary<string, List<OnProtoMsgEvent>>();
    private Dictionary<string, List<OnProtoMsgEvent>> protoMsgOnceDict = new Dictionary<string, List<OnProtoMsgEvent>>();

    public void Init(int maxHandleCount, List<string> logIgnoreList)
    {
        this.maxHandleCount = maxHandleCount;
        protoMsgDispatchArray = new BaseS2CJsonProto[maxHandleCount];
        this.logIgnoreList = logIgnoreList;
    }

    private void Update()
    {
        if (protoMsgQueue.Count == 0)
        {
            return;
        }

        GetProtoMsgDispatchArray();
        if (currNeedHandleCount == 0)
        {
            return;
        }
        for (int i = 0; i < currNeedHandleCount; i++)
        {
            try
            {
                BaseS2CJsonProto protoMsg = protoMsgDispatchArray[i];
                AutoDispatch(protoMsg);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                protoMsgDispatchArray[i] = null;
            }
        }
    }

    private void GetProtoMsgDispatchArray()
    {
        currNeedHandleCount = 0;
        for (int i = 0; i < maxHandleCount; i++)
        {
            BaseS2CJsonProto protoMsg = protoMsgQueue.Dequeue();
            protoMsgDispatchArray[i] = protoMsg;
            currNeedHandleCount++;
            if (protoMsgQueue.Count == 0)
            {
                return;
            }
        }
    }

    private void AutoDispatch(BaseS2CJsonProto protoMsg)
    {
        if (AppConst.IsShowNetProtoDebugLog)
        {
            if (!string.IsNullOrEmpty(protoMsg.err))
            {
                string logInfo = string.Format("[WSNetDispatcher]WSNetDispatchResponse: {0} Error: {1}", protoMsg.type,protoMsg.err);
                Debug.LogErrorFormat("{0}\n{1}", logInfo, protoMsg.GetJson());
            }
            else
            {
                if (!logIgnoreList.Contains(protoMsg.type))
                {
                    string logInfo = string.Format("[WSNetDispatcher]DispatchResponse: {0}", protoMsg.type);
                    Debug.LogFormat("{0}\n{1}", logInfo, protoMsg.GetJson());
                }
            }
        }

        string msgID = protoMsg.type;
        InvokeMethods(protoPriorityMsgDict, msgID, protoMsg);
        InvokeMethods(protoMsgDict, msgID, protoMsg);
        InvokeMethods(protoMsgOnceDict, msgID, protoMsg);

        if (protoMsgOnceDict.ContainsKey(msgID))
        {
            protoMsgOnceDict.Remove(msgID);
        }
    }

    private void InvokeMethods(Dictionary<string, List<OnProtoMsgEvent>> msgDict, string msgID, BaseS2CJsonProto protoMsg)
    {
        if (!msgDict.ContainsKey(msgID))
        {
            return;
        }

        List<OnProtoMsgEvent> rawList = msgDict[msgID];
        int funcCount = rawList.Count;
        if (funcCount == 1)
        {
            OnProtoMsgEvent msgEvent = rawList[0];
            msgEvent(protoMsg);
            return;
        }

        List<OnProtoMsgEvent> invokeFuncs = new List<OnProtoMsgEvent>();
        invokeFuncs.AddRange(rawList);
        for (int i = 0; i < funcCount; i++)
        {
            try
            {
                OnProtoMsgEvent msgEvent = invokeFuncs[i];
                msgEvent(protoMsg);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public void Dispatch(BaseS2CJsonProto protoMsg)
    {
        protoMsgQueue.Enqueue(protoMsg);
    }

    public void ClearProtoMsg()
    {
        protoMsgQueue.Clear();
        Debug.LogFormat("[WSNetDispatcher]ClearProtoMsg");
    }

    public void AddPriorityListener(string msgID, OnProtoMsgEvent listener)
    {
        if (protoPriorityMsgDict.ContainsKey(msgID))
        {
            protoPriorityMsgDict[msgID].Add(listener);
        }
        else
        {
            List<OnProtoMsgEvent> list = new List<OnProtoMsgEvent>();
            list.Add(listener);
            protoPriorityMsgDict.Add(msgID, list);
        }
    }

    public void AddListener(string msgID, OnProtoMsgEvent listener)
    {
        if (protoMsgDict.ContainsKey(msgID))
        {
            protoMsgDict[msgID].Add(listener);
        }
        else
        {
            List<OnProtoMsgEvent> list = new List<OnProtoMsgEvent>();
            list.Add(listener);
            protoMsgDict.Add(msgID, list);
        }
    }

    public void AddOnceListener(string msgID, OnProtoMsgEvent listener)
    {
        if (protoMsgOnceDict.ContainsKey(msgID))
        {
            protoMsgOnceDict[msgID].Add(listener);
        }
        else
        {
            List<OnProtoMsgEvent> list = new List<OnProtoMsgEvent>();
            list.Add(listener);
            protoMsgOnceDict.Add(msgID, list);
        }
    }
    public void RemovePriorityListener(string msgId, OnProtoMsgEvent listener)
    {
        if (protoPriorityMsgDict.ContainsKey(msgId))
        {
            List<OnProtoMsgEvent> list = protoPriorityMsgDict[msgId];
            list.Remove(listener);
            if (list.Count == 0)
            {
                protoPriorityMsgDict.Remove(msgId);
            }
        }
    }

    public void RemoveListener(string msgId, OnProtoMsgEvent listener)
    {
        if (protoMsgDict.ContainsKey(msgId))
        {
            List<OnProtoMsgEvent> list = protoMsgDict[msgId];
            list.Remove(listener);
            if (list.Count == 0)
            {
                protoMsgDict.Remove(msgId);
            }
        }
    }

    public void RemoveOnceListener(string msgId, OnProtoMsgEvent listener)
    {
        if (protoMsgOnceDict.ContainsKey(msgId))
        {
            List<OnProtoMsgEvent> list = protoMsgOnceDict[msgId];
            list.Remove(listener);
            if (list.Count == 0)
            {
                protoMsgOnceDict.Remove(msgId);
            }
        }
    }

    public void Clear()
    {
        protoMsgQueue.Clear();
        protoPriorityMsgDict.Clear();
        protoMsgDict.Clear();
        protoMsgOnceDict.Clear();
    }

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Clear();
        protoMsgQueue = null;
        protoPriorityMsgDict = null;
        protoMsgDispatchArray = null;
        protoMsgDict = null;
        protoMsgOnceDict = null;
    }
    #endregion
}
