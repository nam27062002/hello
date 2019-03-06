using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

namespace Downloadables
{
    public class ErrorConfig
    {
        private const string ATT_MAX = "max";

        public ErrorConfig()
        {
            Reset();
        }

        /// <summary>
        /// Max amount of times this type of error can happen
        /// </summary>
        public int MaxTimesPerSession { get; private set; }

        public void Reset()
        {
            MaxTimesPerSession = 0;
        }

        public void Load(JSONNode data)
        {
            if (data != null)
            {
                MaxTimesPerSession = data[ATT_MAX].AsInt;
            }
        }
    }

    /// <summary>
    /// This class is responsible for storing downloadables related configuration.
    /// </summary>
    public class Config
    {
        private const string ATT_AUTO_ENABLED = "autoEnabled";
        private const string ATT_MAX_TRACKING_EVENTS_PER_ERROR_TYPE = "maxTrackingEventsPerErrorType";

        private const string ATT_ERRORS = "errors";

        private Dictionary<Error.EType, ErrorConfig> m_errorsConfig;

        private bool m_isAutomaticDownloaderEnabled;

        /// <summary>
        /// When <c>true</c> all downloads will be downloaded automatically. Otherwise a downloadable will be downloaded only on demand (by calling Request)
        /// </summary>        
        public bool IsAutomaticDownloaderEnabled
        {
            get
            {
                return m_isAutomaticDownloaderEnabled;
            }

            set
            {
                m_isAutomaticDownloaderEnabled = value;
            }
        }

        public int MaxTrackingEventsPerErrorType { get; set; }

        public Config()
        {
            m_errorsConfig = new Dictionary<Error.EType, ErrorConfig>();
        }

        public void Reset()
        {
            IsAutomaticDownloaderEnabled = true;
            MaxTrackingEventsPerErrorType = 1;

            m_errorsConfig.Clear();
        }

        public void Load(JSONNode data, Logger logger)
        {
            Reset();

            if (data != null)
            {
                string key = ATT_AUTO_ENABLED;
                if (data.ContainsKey(key))
                {
                    IsAutomaticDownloaderEnabled = data[key].AsBool;
                }

                key = ATT_MAX_TRACKING_EVENTS_PER_ERROR_TYPE;
                if (data.ContainsKey(key))
                {
                    MaxTrackingEventsPerErrorType = data[key].AsInt;
                }

                LoadErrors(data[ATT_ERRORS], logger);
            }
        }       

        private void LoadErrors(JSONNode data, Logger logger)
        {
            if (data != null)
            {                
                string id;
                Error.EType errorType;
                ErrorConfig errorConfig;

                ArrayList keys = ((JSONClass)data).GetKeys();
                int count = keys.Count;
                for (int i = 0; i < count; i++)
                {
                    id = (string)keys[i];

                    errorType = Error.StringToType(id);
                    if (m_errorsConfig.ContainsKey(errorType))
                    {
                        if (logger != null && logger.CanLog())
                        {
                            logger.LogError("Duplicate error " + id + " found in downloadables config");
                        }
                    }
                    else
                    {
                        errorConfig = new ErrorConfig();
                        errorConfig.Load(data[id]);

                        m_errorsConfig.Add(errorType, errorConfig);
                    }
                }
            }
        }

        public ErrorConfig GetErrorConfig(Error.EType type)
        {
            ErrorConfig returnValue = null;
            m_errorsConfig.TryGetValue(type, out returnValue);

            return returnValue;
        }

        public int GetMaxTimesPerSessionPerErrorType(Error.EType type)
        {
            ErrorConfig errorConfig = GetErrorConfig(type);
            return (errorConfig == null) ? int.MaxValue : errorConfig.MaxTimesPerSession;
        }
    }
}
