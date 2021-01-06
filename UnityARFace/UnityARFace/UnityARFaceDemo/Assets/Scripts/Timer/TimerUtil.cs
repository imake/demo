
public static class TimerUtil
{
    #region Global
    public static SimpleTimer GlobalSimpleTimer
    {
        get { return TimerMgr.Instance.GlobalSimpleTimer; }
    }
    public static Timer GlobalTimer
    {
        get { return TimerMgr.Instance.GlobalTimer; }
    }
    public static HeavyTimer GlobalHeavyTimer
    {
        get { return TimerMgr.Instance.GlobalHeavyTimer; }
    }
    #endregion

    #region GamePlay
    private static SimpleTimer _GamePlaySimpleTimer;
    private static Timer _GamePlayTimer;
    private static HeavyTimer _GamePlayHeavyTimer;

    public static SimpleTimer GamePlaySimpleTimer
    {
        get
        {
            if (_GamePlaySimpleTimer == null)
            {
                _GamePlaySimpleTimer = TimerMgr.Instance.CreateSimpleTimer();
            }
            return _GamePlaySimpleTimer;
        }
        set
        {
            _GamePlaySimpleTimer = value;
        }
    }
    public static Timer GamePlayTimer
    {
        get
        {
            if (_GamePlayTimer == null)
            {
                _GamePlayTimer = TimerMgr.Instance.CreateTimer();
            }
            return _GamePlayTimer;
        }
        set
        {
            _GamePlayTimer = value;
        }
    }
    public static HeavyTimer GamePlayHeavyTimer
    {
        get
        {
            if (_GamePlayHeavyTimer == null)
            {
                _GamePlayHeavyTimer = TimerMgr.Instance.CreateHeavyTimer();
            }
            return _GamePlayHeavyTimer;
        }
        set
        {
            _GamePlayHeavyTimer = value;
        }
    }
    #endregion
}
