using UnityEngine;

public abstract class UnitTest
{
    public delegate void OnDoneCallback(bool success);

    private string m_name;
    private bool m_hasPassed;
    protected float m_timeStartAt = -1;

    private OnDoneCallback m_onDone;

    private bool m_isDone;

#if UNITY_EDITOR
    private int m_reachabilityBeforePerform;
    private int m_networkSleepTimeBeforePerform;
    private bool m_diskNoFreeSpaceBeforePerform;
#endif

    public void Setup(string name, OnDoneCallback onDone)
    {
        m_name = name;
        m_onDone = onDone;
        SetHasPassed(false);
        m_isDone = false;
        m_timeStartAt = -1f;
    }

    public void Perform()
    {
#if UNITY_EDITOR
        // Stores current reachability in order to be able to restore it once the test is done
        m_reachabilityBeforePerform = MockNetworkDriver.MockNetworkReachabilityAsInt;

        // We need the test to start in Production mode (with connection)
        MockNetworkDriver.IsMockNetworkReachabilityEnabled = false;

        m_networkSleepTimeBeforePerform = MockNetworkDriver.MockThrottleSleepTime;
        MockNetworkDriver.MockThrottleSleepTime = 0;

        m_diskNoFreeSpaceBeforePerform = MockDiskDriver.IsNoFreeSpaceEnabled;
        MockDiskDriver.IsNoFreeSpaceEnabled = false;
#endif

        m_isDone = false;
        m_timeStartAt = Time.realtimeSinceStartup;
        ExtendedPerform();
    }

    protected abstract void ExtendedPerform();

    public bool HasStarted()
    {
        return m_timeStartAt >= 0;
    }

    private bool GetHasPassed()
    {
        return m_hasPassed;
    }

    private void SetHasPassed(bool value)
    {
        m_hasPassed = value;
    }

    protected void NotifyPasses(bool success)
    {        
        m_isDone = true;
        SetHasPassed(success);

        if (m_onDone != null)
        {            
            m_onDone(success);
        }

        bool hasPassed = GetHasPassed();
        float time = Time.realtimeSinceStartup - m_timeStartAt;
        string msg = "************ " + GetName() + " has passed = " + GetHasPassed() + " time spent = " + time;
        if (hasPassed)
        {
            Debug.Log("<color=green>" + msg + "</color>");
        }
        else
        {
            Debug.LogError(msg);
        }

#if UNITY_EDITOR
        // Restores reachability to the value that was set before performing this test
        MockNetworkDriver.MockNetworkReachabilityAsInt = m_reachabilityBeforePerform;
        MockNetworkDriver.MockThrottleSleepTime = m_networkSleepTimeBeforePerform;
        MockDiskDriver.IsNoFreeSpaceEnabled = m_diskNoFreeSpaceBeforePerform;
#endif
    }

    public string GetName()
    {
        return (m_name == null) ? GetType().ToString() : m_name;
    }

    public bool IsDone()
    {
        return m_isDone;
    }

    public virtual void Update() {}
}