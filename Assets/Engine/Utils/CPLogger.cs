/// <summary>
/// This class is responsible for showing in control panel the messages logged
/// </summary>
public class CPLogger : Logger
{
    private ControlPanel.ELogChannel m_channel = ControlPanel.ELogChannel.General;

    public CPLogger(ControlPanel.ELogChannel channel)
    {
        m_channel = channel;
    }    

    protected override void ExtendedLog(string message)
    {
        ControlPanel.Log(message, m_channel);
    }

    protected override void ExtendedLogWarning(string message)
    {
        ControlPanel.LogWarning(message, m_channel);
    }

    protected override void ExtendedLogError(string message)
    {
        ControlPanel.LogError(message, m_channel);
    }
}
