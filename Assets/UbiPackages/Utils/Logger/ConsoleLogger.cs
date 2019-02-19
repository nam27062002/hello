using UnityEngine;

/// <summary>
/// This logger prints messages out in the unity console.
/// </summary>
public class ConsoleLogger : Logger
{
    private string m_prefix = null;

    public ConsoleLogger(string prefix)
    {
        m_prefix = prefix;
        if (!string.IsNullOrEmpty(prefix))
        {
            m_prefix = "[" + m_prefix + "] ";
        }
    }

    private string FormatMessage(string message)
    {
        return (string.IsNullOrEmpty(m_prefix)) ? message : m_prefix + message; 
    }

    protected override void ExtendedLog(string message)
    {        
        Debug.Log(FormatMessage(message));
    }

    protected override void ExtendedLogWarning(string message)
    {
        Debug.LogWarning(FormatMessage(message));
    }

    protected override void ExtendedLogError(string message)
    {
        Debug.LogError(FormatMessage(message));
    }
}
