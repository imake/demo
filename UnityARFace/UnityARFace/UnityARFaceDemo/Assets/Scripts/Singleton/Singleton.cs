using System;

/// <summary>
/// 单例类
/// </summary>
public class Singleton<T> : IDisposable where T : class, new()
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new T();
                //(m_instance as Singleton<T>).Init();
            }
            return m_instance;
        }
    }

    public virtual void Init()
    {

    }

    public virtual void Dispose()
    {
        m_instance = null;
    }
}