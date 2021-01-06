

using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerInfo
{
    public Timer timer;
    public int repeatCount;
    public float interval;
    public Action<TimerInfo> onCallBack;
    public object[] args;
    public float time;
    public bool isActive = true;

    public TimerInfo(float interval, Action<TimerInfo> onCallBack, params object[] args)
    {
        repeatCount = 1;
        this.interval = interval;
        this.onCallBack = onCallBack;
        this.args = args;
        time = Timer.GetTriggerTime(interval);
    }

    public TimerInfo(int repeatCount, float interval, Action<TimerInfo> onCallBack, params object[] args)
    {
        this.repeatCount = repeatCount;
        this.interval = interval;
        this.onCallBack = onCallBack;
        this.args = args;
        time = Timer.GetTriggerTime(interval);
    }

    public void SetActive(bool isActive)
    {
        if (this.isActive == isActive) return;

        this.isActive = isActive;
        if (this.isActive)
        {
            time = Timer.GetTriggerTime(interval);
        }
    }

    public void Dispose()
    {
        timer.RemoveTimer(this);
    }
}

public class Timer : MonoBehaviour
{
    public const int INFINITE_LOOP = -1;

    private List<TimerInfo> timerInfos = new List<TimerInfo>();
    private List<TimerInfo> timerInfoTriggers = new List<TimerInfo>();

    private void Update()
    {
        if (timerInfos.Count > 0)
        {
            timerInfoTriggers.Clear();
            for (int i = 0; i < timerInfos.Count; i++)
            {
                TimerInfo timerInfo = timerInfos[i];
                if (!timerInfo.isActive) continue;

                if (timerInfo.time <= Time.time)
                {
                    timerInfoTriggers.Add(timerInfo);
                }
            }

            for (int i = 0; i < timerInfoTriggers.Count; i++)
            {
                TimerInfo triggerTimerInfo = timerInfoTriggers[i];
                if (triggerTimerInfo.repeatCount != INFINITE_LOOP)
                {
                    triggerTimerInfo.repeatCount--;
                }
                if (triggerTimerInfo.repeatCount == 0)
                {
                    timerInfos.Remove(triggerTimerInfo);
                }
                else
                {
                    triggerTimerInfo.time = GetTriggerTime(triggerTimerInfo.interval);
                }
                triggerTimerInfo.onCallBack(triggerTimerInfo);
            }
        }
    }

    public TimerInfo AddTimer(float interval, Action<TimerInfo> onCallBack, params object[] args)
    {
        TimerInfo timeInfo = new TimerInfo(interval, onCallBack, args);
        timeInfo.timer = this;
        AddTimer(timeInfo);
        return timeInfo;
    }

    public TimerInfo AddRepeatTimer(int repeatCount, float interval, Action<TimerInfo> onCallBack, params object[] args)
    {
        TimerInfo timeInfo = new TimerInfo(repeatCount, interval, onCallBack, args);
        timeInfo.timer = this;
        AddTimer(timeInfo);
        return timeInfo;
    }

    public TimerInfo AddLoopTimer(float interval, Action<TimerInfo> onCallBack, params object[] args)
    {
        TimerInfo timeInfo = new TimerInfo(INFINITE_LOOP, interval, onCallBack, args);
        timeInfo.timer = this;
        AddTimer(timeInfo);
        return timeInfo;
    }

    public void AddTimer(TimerInfo timerInfo)
    {
        if (null != timerInfo)
        {
            if (timerInfo.interval <= 0)
            {
                timerInfo.onCallBack(timerInfo);
                return;
            }
            timerInfos.Add(timerInfo);
        }
    }

    public void RemoveTimer(TimerInfo timerInfo)
    {
        if (null != timerInfo)
        {
            if (timerInfos.Contains(timerInfo))
            {
                timerInfos.Remove(timerInfo);
            }
        }
    }

    public void ClearAll()
    {
        timerInfos.Clear();
        timerInfoTriggers.Clear();
    }

    public void Destroy()
    {
        Destroy(this);
    }

    public static float GetTriggerTime(float interval)
    {
        return Time.time + interval;
    }

    void OnDestroy()
    {
        ClearAll();
        timerInfos = null;
        timerInfoTriggers = null;
    }
}
