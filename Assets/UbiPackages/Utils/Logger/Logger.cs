public class Logger
{
    public enum EMessageType
    {
        Info,
        Warning,
        Error
    };

    public class EMessage
    {
        public EMessageType MessageType { get; set; }
        public string Message { get; set; }

        public EMessage(EMessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }

    public virtual bool CanLog()
    {
        return true;
    }

    public void Log(string message)
    {
        if (CanLog())
        {
            ExtendedLog(message);
        }
    }

    protected virtual void ExtendedLog(string message)
    {
    }

    public virtual void LogWarning(string message)
    {
        if (CanLog())
        {
            ExtendedLogWarning(message);
        }
    }

    protected virtual void ExtendedLogWarning(string message)
    {
    }

    public void LogError(string message)
    {
        if (CanLog())
        {
            ExtendedLogError(message);
        }
    }

    protected virtual void ExtendedLogError(string message)
    {
    }
}
