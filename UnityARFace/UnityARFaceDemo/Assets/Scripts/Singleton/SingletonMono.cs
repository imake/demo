using UnityEngine;

/// <summary>
/// MonoBehaviour 单例类
/// </summary>
public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                if (IsAppQuit)
                {
                    return null;
                }
                CreateInstance();
            }
            return m_instance;
        }
    }

    private static bool IsAppQuit;
    protected virtual string ParentRootName { get; private set; }

    private static void CreateInstance()
    {
        GameObject instanceGO = new GameObject(typeof(T).Name);
        m_instance = instanceGO.AddComponent<T>();
        SetSelfParentRoot(instanceGO, m_instance.ParentRootName);

        m_instance.New();
    }

    private static void SetSelfParentRoot(GameObject go, string parentRootName)
    {
        AppObjConst.EngineSingletonGo = GameObject.Find(AppObjConst.EngineSingletonGoName);

        if (!AppObjConst.EngineSingletonGo) return;

        if (parentRootName == null)
        {
            go.transform.SetParent(AppObjConst.EngineSingletonGo.transform, false);
        }
        else if (parentRootName.Equals(AppObjConst.EngineSingletonGo.name))
        {
            go.transform.SetParent(AppObjConst.EngineSingletonGo.transform, false);
        }
        else
        {
            Transform sinleonRoot = AppObjConst.EngineSingletonGo.transform;
            Transform rootTF = sinleonRoot.Find(parentRootName);
            if (rootTF == null)
            {
                GameObject rootGo = new GameObject(parentRootName);
                rootTF = rootGo.transform;
                rootTF.transform.SetParent(sinleonRoot, false);
            }
            go.transform.SetParent(rootTF.transform, false);
        }
    }

    protected virtual void New() { }
    public virtual void Dispose()
    {

    }

    protected virtual void OnDestroy()
    {
        m_instance = null;
    }

    private void OnApplicationQuit()
    {
        IsAppQuit = true;
        if (m_instance == null) return;

        Destroy(m_instance.gameObject);
    }
}