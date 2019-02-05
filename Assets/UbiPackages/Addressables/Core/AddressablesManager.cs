using SimpleJSON;
using UnityEngine;

/// <summary>
/// This class is responsible for serving a resource (either an assets or an scene) given a name 
/// </summary>
public class AddressablesManager
{
    private AddressablesCatalog m_catalog;

    public void Initialize(JSONClass catalogJSON, Logger logger)
    {
        sm_logger = logger;
        
        m_catalog = new AddressablesCatalog();        
        m_catalog.Load(catalogJSON, logger);        
    }

    #region logger
    private static Logger sm_logger;

    public static bool CanLog()
    {
        return sm_logger != null && sm_logger.CanLog();
    }

    public void Log(string msg)
    {
        if (CanLog())
        {
            sm_logger.Log(msg);
        }
    }

    public void LogWarning(string msg)
    {
        if (CanLog())
        {
            sm_logger.LogWarning(msg);
        }
    }

    public void LogError(string msg)
    {
        if (CanLog())
        {
            sm_logger.LogError(msg);
        }
    }
    #endregion    
}
