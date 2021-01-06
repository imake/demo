
using System;
using System.Collections.Generic;
using UnityEngine;


public class HeavyTimer : MonoBehaviour
{
    private class TimerTask
    {
        public HeavyTimer heavyTimer;

        private string name;
        private TimerMode mode;
        private float startTime;
        private float duration;

        private bool isBreak = false;
        private float breakStart;
        private float breakDuration = 0;

        private Action timerEvent;
        private Action<object[]> timerArgsEvent;
        private object[] args = null;

        public float TimeLeft
        {
            get
            {
                return Mathf.Max(0f, duration - (Time.time - startTime) + breakDuration);
            }
        }

        public TimerTask(string name, TimerMode mode, float startTime, float duration, Action handler)
        {
            this.name = name;
            this.mode = mode;
            this.startTime = startTime;
            this.duration = duration;
            timerEvent = handler;
        }
        public TimerTask(string name, TimerMode mode, float startTime, float duration, Action<object[]> handler, params object[] args)
        {
            this.name = name;
            this.mode = mode;
            this.startTime = startTime;
            this.duration = duration;
            timerArgsEvent = handler;
            this.args = args;
        }

        public void Run()
        {
            if (isBreak)
            {
                return;
            }
            if (TimeLeft > 0f)
            {
                return;
            }

            if (mode == TimerMode.Normal)
            {
                heavyTimer.RemoveTimer(name);
            }
            else
            {
                startTime = Time.time;
                breakDuration = 0;
            }

            if (timerEvent != null)
            {
                timerEvent();
            }
            if (timerArgsEvent != null)
            {
                timerArgsEvent(args);
            }
            return;
        }

        public void Break()
        {
            if (isBreak)
            {
                return;
            }

            isBreak = true;
            breakStart = Time.time;
        }

        public void Resume()
        {
            if (!isBreak)
            {
                return;
            }

            breakDuration += (Time.time - breakStart);
            isBreak = false;
        }
    }

    private enum TimerMode
    {
        Normal,
        Repeat,
    }

    private Dictionary<string, TimerTask> addTimerList = new Dictionary<string, TimerTask>();
    private Dictionary<string, TimerTask> timerList = new Dictionary<string, TimerTask>();
    private List<string> destroyTimerList = new List<string>();

    private void Update()
    {
        if (destroyTimerList.Count > 0)
        {
            foreach (string i in destroyTimerList)
            {
                timerList.Remove(i);
            }
            destroyTimerList.Clear();
        }

        if (addTimerList.Count > 0)
        {
            foreach (KeyValuePair<string, TimerTask> i in addTimerList)
            {
                if (i.Value == null)
                {
                    continue;
                }

                if (timerList.ContainsKey(i.Key))
                {
                    timerList[i.Key] = i.Value;
                }
                else
                {
                    timerList.Add(i.Key, i.Value);
                }
            }
            addTimerList.Clear();
        }

        if (timerList.Count > 0)
        {
            foreach (TimerTask timer in timerList.Values)
            {
                if (timer == null)
                {
                    return;
                }
                timer.Run();
            }
        }
    }

    public bool AddTimer(string key, float duration, Action handler)
    {
        return InternalAddTimer(key, TimerMode.Normal, duration, handler);
    }
    public bool AddTimer(string key, float duration, Action<object[]> handler, params object[] args)
    {
        return InternalAddTimer(key, TimerMode.Normal, duration, handler, args);
    }

    public bool AddRepeatTimer(string key, float duration, Action handler)
    {
        return InternalAddTimer(key, TimerMode.Repeat, duration, handler);
    }
    public bool AddRepeatTimer(string key, float duration, Action<object[]> handler, params object[] args)
    {
        return InternalAddTimer(key, TimerMode.Repeat, duration, handler, args);
    }

    private bool InternalAddTimer(string key, TimerMode mode, float duration, Action handler)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }
        if (duration <= 0.0f)
        {
            if (mode == TimerMode.Normal)
            {
                handler();
            }
            else
            {
                Debug.LogError("[HeavyTimer]Add Repeat Timer Error: Time is Not Greater Than Zero");
            }
            return false;
        }

        TimerTask timer = new TimerTask(key, mode, Time.time, duration, handler);
        timer.heavyTimer = this;

        if (addTimerList.ContainsKey(key))
        {
            addTimerList[key] = timer;
        }
        else
        {
            addTimerList.Add(key, timer);
        }
        return true;
    }

    private bool InternalAddTimer(string key, TimerMode mode, float duration, Action<object[]> handler, params object[] args)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }
        if (duration <= 0.0f)
        {
            if (mode == TimerMode.Normal)
            {
                handler(args);
            }
            else
            {
                Debug.LogError("[HeavyTimer]Add Repeat Timer Error: Time is Not Greater Than Zero");
            }
            return false;
        }

        TimerTask timer = new TimerTask(key, mode, Time.time, duration, handler, args);
        timer.heavyTimer = this;

        if (addTimerList.ContainsKey(key))
        {
            addTimerList[key] = timer;
        }
        else
        {
            addTimerList.Add(key, timer);
        }
        return true;
    }

    public void BreakTimerWithPrefix(string prefix)
    {
        if (timerList != null && timerList.Count > 0)
        {
            string[] arr = new string[timerList.Count];
            timerList.Keys.CopyTo(arr, 0);

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].StartsWith(prefix))
                {
                    BreakTimer(arr[i]);
                }
            }
        }
    }

    public void BreakTimer(string key)
    {
        if (!timerList.ContainsKey(key))
        {
            return;
        }

        TimerTask timer = timerList[key];
        timer.Break();
    }

    public void ResumeTimerWithPrefix(string prefix)
    {
        if (timerList != null && timerList.Count > 0)
        {
            string[] arr = new string[timerList.Count];
            timerList.Keys.CopyTo(arr, 0);

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].StartsWith(prefix))
                {
                    ResumeTimer(arr[i]);
                }
            }
        }
    }

    public void ResumeTimer(string key)
    {
        if (!timerList.ContainsKey(key))
        {
            return;
        }

        TimerTask timer = timerList[key];
        timer.Resume();
    }

    public void ClearTimerWithPrefix(string prefix)
    {
        if (timerList != null && timerList.Count > 0)
        {
            foreach (string timerKey in timerList.Keys)
            {
                if (timerKey.StartsWith(prefix))
                {
                    RemoveTimer(timerKey);
                }
            }
        }
    }

    public bool RemoveTimer(string key)
    {
        if (!timerList.ContainsKey(key))
        {
            return false;
        }

        if (!destroyTimerList.Contains(key))
        {
            destroyTimerList.Add(key);
        }
        return true;
    }

    public bool IsRunning(string key)
    {
        return timerList.ContainsKey(key);
    }

    public float GetTimerLeft(string key)
    {
        if (!timerList.ContainsKey(key))
        {
            return 0.0f;
        }

        TimerTask timer = timerList[key];
        return timer.TimeLeft;
    }

    public float GetTimerLeftWithPrefix(string prefix)
    {
        if (timerList != null && timerList.Count > 0)
        {
            string[] arr = new string[timerList.Count];
            timerList.Keys.CopyTo(arr, 0);

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].StartsWith(prefix))
                {
                    return GetTimerLeft(arr[i]);
                }
            }
        }
        return 0.0f;
    }

    public void ClearAll()
    {
        addTimerList.Clear();
        timerList.Clear();
        destroyTimerList.Clear();
    }

    public void Destroy()
    {
        Destroy(this);
    }

    private void OnDestroy()
    {
        ClearAll();
        addTimerList = null;
        timerList = null;
        destroyTimerList = null;
    }
}
