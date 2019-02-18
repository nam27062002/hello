using UnityEngine;

public abstract class UnitTest
{
    public delegate void OnDoneCallback(bool success);

    private string m_name;
    private bool m_hasPassed;
    protected float m_timeStartAt;

    private OnDoneCallback m_onDone;

    private bool m_isDone;

    public void Setup(string name, OnDoneCallback onDone)
    {
        m_name = name;
        m_onDone = onDone;
        SetHasPassed(false);
        m_isDone = false;
    }

    public void Perform()
    {
        m_isDone = false;
        m_timeStartAt = Time.realtimeSinceStartup;
        ExtendedPerform();
    }

    protected abstract void ExtendedPerform();

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
