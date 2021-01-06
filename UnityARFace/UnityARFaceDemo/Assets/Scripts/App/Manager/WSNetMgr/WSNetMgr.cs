using BestHTTP;
using BestHTTP.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WSNetState
{
    None = 0,
    //无网络
    NoNetWork = 1,
    //连接中
    Connecting = 2,
    //已连接
    Connected = 3,
    //已关闭
    Closed = 4,
    //网络异常
    NetWorkException = 5,
    //登陆中
    Logining = 6,
    //请求配置
    ResquestConfiging = 7,
    //登陆成功
    LoginSussess = 8,
    //登录失败，需要延迟登录
    LoginFailed_MustDelay = 9,
    //需要更新
    NeedUpdate = 10,
    //强制更新
    MustUpdate = 11,
    //
    PreferencesParseError = 12,

    ConfigParseError = 13,
}

public class WSNetMgr : Singleton<WSNetMgr>
{
    private const int DefaultTimeOut = 5;

    private Dictionary<string, Type> protoC2STypeDict = new Dictionary<string, Type>();
    private Dictionary<string, Type> protoS2CTypeDict = new Dictionary<string, Type>();
    private List<string> c2sProtoLogIgnoreList = new List<string>();
    private List<string> s2cProtoLogIngoreList = new List<string>();

    public WSNetState state = WSNetState.None;
    public bool isConnecting;
    public bool isConnected;
    public bool isWss;

    private WebSocket webSocket;
    private int maxProtoHandleCount = 10;
    private Action connectSucceedFunc;

    public override void Init()
    {
        base.Init();

        InitBestHttpSetting();
        WSNetDispatcher.Instance.Init(maxProtoHandleCount, s2cProtoLogIngoreList);
    }

    public override void Dispose()
    {
        base.Dispose();

        HTTPUpdateDelegator.Instance.ApplicationQuit();
        Close();
    }

    public void InitBestHttpSetting()
    {
        HTTPManager.ConnectTimeout = TimeSpan.FromSeconds(DefaultTimeOut);
        HTTPManager.RequestTimeout = TimeSpan.FromSeconds(DefaultTimeOut);
    }

    public void Close()
    {
        if (webSocket != null)
        {
            state = WSNetState.Closed;
            isConnecting = false;
            isConnected = false;

            webSocket.Close();
            webSocket = null;
        }
    }

    public void RegisterC2SProtoType(string protoKey, Type protoType)
    {
        if (!protoC2STypeDict.ContainsKey(protoKey))
        {
            protoC2STypeDict.Add(protoKey, protoType);
        }
    }

    public void RegisterS2CProtoType(string protoKey,Type protoType)
    {
        if (!protoS2CTypeDict.ContainsKey(protoKey))
        {
            protoS2CTypeDict.Add(protoKey, protoType);
        }
    }

    public void RegisterC2SProtoLogIgnore(string msgType)
    {
        if (!c2sProtoLogIgnoreList.Contains(msgType))
        {
            c2sProtoLogIgnoreList.Add(msgType);
        }
    }

    public void RegisterS2CProtoLogIgnore(string msgType)
    {
        if (!s2cProtoLogIngoreList.Contains(msgType))
        {
            s2cProtoLogIngoreList.Add(msgType);
        }
    }

    public bool Connect(Action connectSuccessedFunc = null)
    {
        Debug.LogFormat("[WSNetMgr]ConnectBefore IsNetAvailable:{0} isConnecting:{1} IsConnected:{2}",NetConst.IsNetAvailable, isConnecting, isConnected);

        state = WSNetState.None;
        if (!NetConst.IsNetAvailable)
        {
            state = WSNetState.NoNetWork;
            return false;
        }
        if (isConnecting)
        {
            return false;
        }
        if (isConnected)
        {
            Close();
        }

        this.connectSucceedFunc = connectSuccessedFunc;
        Connect(NetConst.WebSocketUrl);
        return true;
    }

    public void Connect(string url)
    {
        state = WSNetState.Connecting;
        isConnected = false;

        isWss = IsWssUrl(url);
        webSocket = new WebSocket(new Uri(url));
        webSocket.OnOpen = OnWebSocketOpen;
        webSocket.OnClosed = OnWebSocketClosed;
        if (isWss)
        {
            webSocket.OnMessage = OnWebSocketText;
        }
        else
        {
            webSocket.OnBinary = OnWebSocketBinary;
        }
        webSocket.OnError = OnWebSocketError;
        webSocket.OnErrorDesc = OnWebSocketErrorDesc;
        webSocket.Open();
        isConnecting = true;
    }

    private void OnWebSocketOpen(WebSocket webSocket)
    {
        Debug.Log("[WSNetMgr]OnWebSocketOpen");
        state = WSNetState.Connected;
        isConnecting = false;
        isConnected = true;
        if (connectSucceedFunc !=null)
        {
            connectSucceedFunc();
            connectSucceedFunc = null;
        }
    }

    private void OnWebSocketClosed(WebSocket webSocket, ushort code, string message)
    {
        Debug.LogFormat("[WSNetMgr]OnWebSocketClosed code:{0} message:{1}", code, message);
        state = WSNetState.Closed;
        isConnecting = false;
        isConnected = false;
    }

    private void OnWebSocketText(WebSocket webSocket, string message)
    {
        OnReceiveText(message);
    }

    private void OnWebSocketBinary(WebSocket webSocket, byte[] data)
    {
        OnReceiveBinary(data);
    }

    private void OnWebSocketError(WebSocket webSocket, Exception ex)
    {
        if (ex != null)
        {
            Debug.LogFormat("[WSNetMgr]OnWebSocketError\n{0}", ex);
        }
        else
        {
            Debug.Log("[WSNetMgr]OnWebSocketError");
        }
        Close();
        state = WSNetState.NetWorkException;
    }

    private void OnWebSocketErrorDesc(WebSocket webSocket, string reason)
    {
        Debug.LogFormat("[WSNetMgr]OnWebSocketErrorDesc\n{0}", reason);
    }

    public bool IsWssUrl(string url)
    {
        return url.StartsWith("wss:");
    }

    private void ReceiveProtoMsg(string jsonMsg)
    {
        BaseS2CJsonProto s2CJsonProto = JsonConvert.DeserializeObject<BaseS2CJsonProto>(jsonMsg);
        string msgType = s2CJsonProto.type;
        try
        {
            if (!protoS2CTypeDict.ContainsKey(msgType))
            {
                Debug.LogErrorFormat("[WSNetMgr]OnReceiveResponse: {0} No Registered Message, Json:\n{1}", msgType, jsonMsg);
                return;
            }
            Type protoMsgType = protoS2CTypeDict[msgType];
            BaseS2CJsonProto proto= JsonConvert.DeserializeObject(jsonMsg, protoMsgType) as BaseS2CJsonProto;
            proto.SetJson(jsonMsg);
            WSNetDispatcher.Instance.Dispatch(proto);
        }
        catch(Exception e)
        {
            Debug.LogErrorFormat("[WSNetMgr]OnReceiveResponse: {0} Exception: {1} Json:\n{2}", msgType, e.ToString(), jsonMsg);
        }
    }

    private void OnReceiveText(string text)
    {
        ReceiveProtoMsg(text);
    }

    private void OnReceiveBinary(byte[] data)
    {
        string jsonMsg = JsonEncryptUtil.DecryptJsonProto(data);
        ReceiveProtoMsg(jsonMsg);
    }

    public bool Send(BaseC2SJsonProto protoMsg)
    {
        string msgType = protoMsg.type;
        try
        {
            bool isCanSend = webSocket != null && isConnected && NetConst.IsNetAvailable && AppGlobal.IsLoginSucceed;
            if (isCanSend)
            {
                if (!protoC2STypeDict.ContainsKey(msgType))
                {
                    Debug.LogErrorFormat("[WSNetMgr]SendRequest: {0} No Registered Message", msgType);
                    return false;
                }
                SendProtoMsg(protoMsg);
                if (AppConst.IsShowNetProtoDebugLog)
                {
                    if (!c2sProtoLogIgnoreList.Contains(msgType))
                    {
                        string logInfo = string.Format("[WSNetMgr]SendRequest: {0}", msgType);
                        Debug.Log(logInfo + " protoMsg:" + protoMsg);
                    }
                }
            }
            else
            {
                if (AppConst.IsShowNetProtoDebugLog)
                {
                    if (!c2sProtoLogIgnoreList.Contains(msgType))
                    {
                        string logInfo = string.Format("[WSNetMgr]Can't Send ImmediateSendRequest: {0} ", msgType);
                        Debug.Log(logInfo+ " protoMsg:" + protoMsg);
                    }
                }
            }
            return isCanSend;
        }
        catch(Exception e)
        {
            Debug.LogErrorFormat("[WSNetMgr]SendRequest: {0} Exception: {1}", msgType, e.ToString());
            return false;
        }
    }

    public bool ImmediateSend(BaseC2SJsonProto protoMsg)
    {
        string msgType = protoMsg.type;
        try
        {
            bool isCanSend = webSocket != null && isConnected && NetConst.IsNetAvailable;
            if (isCanSend)
            {
                if (!protoC2STypeDict.ContainsKey(msgType))
                {
                    Debug.LogErrorFormat("[WSNetMgr]ImmediateSendRequest: {0} No Registered Message", msgType);
                    return false;
                }

                SendProtoMsg(protoMsg);

                if (AppConst.IsShowNetProtoDebugLog)
                {
                    if (!c2sProtoLogIgnoreList.Contains(msgType))
                    {
                        string logInfo = string.Format("[WSNetMgr]ImmediateSendRequest: {0}", msgType);
                        Debug.Log(logInfo + " protoMsg:" + protoMsg);
                    }
                }
            }
            else
            {
                if (AppConst.IsShowNetProtoDebugLog)
                {
                    if (!c2sProtoLogIgnoreList.Contains(msgType))
                    {
                        string logInfo = string.Format("[WSNetMgr]Can't Send ImmediateSendRequest: {0} ", msgType);
                        Debug.Log(logInfo + " protoMsg:" + protoMsg);
                    }
                }
            }
            return isCanSend;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("[WSNetMgr]ImmediateSendRequest {0} Exception: {1}", msgType, e.ToString());
            return false;
        }
    }

    private void SendProtoMsg(BaseC2SJsonProto protoMsg)
    {
        Type protoMsgType = protoC2STypeDict[protoMsg.type];
        if (isWss)
        {
            string proto = JsonEncryptUtil.NoEncryptJsonProto(protoMsg, protoMsgType);
            webSocket.Send(proto);
        }
        else
        {
            byte[] bytes = JsonEncryptUtil.EncryptJsonProto(protoMsg, protoMsgType);
            webSocket.Send(bytes);
        }
    }
}
