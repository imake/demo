
using UnityEngine;


public class TimerMgr : SingletonMono<TimerMgr>
{
    private GameObject simpleTimersRoot;
    private GameObject timersRoot;
    private GameObject heavyTimersRoot;

    public SimpleTimer GlobalSimpleTimer { get; private set; }
    public Timer GlobalTimer { get; private set; }
    public HeavyTimer GlobalHeavyTimer { get; private set; }

    private void InitGlobalTimers()
    {
        simpleTimersRoot = new GameObject("SimpleTimers");
        simpleTimersRoot.transform.SetParent(transform);
        timersRoot = new GameObject("Timers");
        timersRoot.transform.SetParent(transform);
        heavyTimersRoot = new GameObject("HeavyTimers");
        heavyTimersRoot.transform.SetParent(transform);

        GlobalSimpleTimer = CreateSimpleTimer();
        GlobalTimer = CreateTimer();
        GlobalHeavyTimer = CreateHeavyTimer();
    }

    public SimpleTimer CreateSimpleTimer()
    {
        return simpleTimersRoot.AddComponent<SimpleTimer>();
    }

    public Timer CreateTimer()
    {
        return timersRoot.AddComponent<Timer>();
    }

    public HeavyTimer CreateHeavyTimer()
    {
        return heavyTimersRoot.AddComponent<HeavyTimer>();
    }

    #region Mgr
    public void Init()
    {
        InitGlobalTimers();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion
}
