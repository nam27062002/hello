using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for defining the interface of the class that will be notified every time a relevant action related to downloadables happens
    /// </summary>
    public abstract class Tracker
    {
        private const float BYTES_TO_MB = 1 / (1024 * 1024);

        public enum EAction
        {
            Download,
            Update,
            Load
        };

        private Dictionary<Error.EType, int> m_maxPerErrorType = null;        

        /// <summary>
        /// Key: downloadable id; Value: TrackierInfo containing tracking related information of this downloadable
        /// </summary>
        private Dictionary<string, Dictionary<Error.EType, int>> m_trackerInfo;

        protected Logger m_logger;

        private bool m_currentDownloadIsUpdate;
        private float m_currentDownloadTimeAtStart;
        private string m_currentDownloadId;
        private float m_currentDownloadExistingSizeAtStart;
        private NetworkReachability m_currentDownloadReachabilityAtStart;

        public Tracker(Dictionary<Error.EType, int> maxPerErrorType, Logger logger)
        {
            m_maxPerErrorType = maxPerErrorType;
            m_currentDownloadTimeAtStart = -1;
        }        

        private float GetSizeInMb(float sizeInBytes)
        {
            return sizeInBytes * BYTES_TO_MB;
        }
        public void NotifyDownloadStart(float currentTime, string downloadableId, float existingSizeMbAtStart, NetworkReachability reachabilityAtStart, bool isUpdate)
        {
            m_currentDownloadTimeAtStart = currentTime;
            m_currentDownloadId = downloadableId;
            m_currentDownloadExistingSizeAtStart = existingSizeMbAtStart;
            m_currentDownloadReachabilityAtStart = reachabilityAtStart;
            m_currentDownloadIsUpdate = isUpdate;
        }

        public void NotifyDownloadEnd(float currentTime, string downloadableId, float existingSizeAtEnd,  float totalSize, NetworkReachability reachabilityAtEnd, Error.EType error)
        {
            if (m_currentDownloadTimeAtStart < 0f)
            {
                if (CanLog())
                {
                    m_logger.LogError("NotifyDownloadEnd called without calling NotifyDownloadStart before");
                }

                return;
            }
            else if (downloadableId != m_currentDownloadId)
            {
                if (CanLog())
                {
                    m_logger.LogError("NotifyDownloadEnd was expected to be called for <" + m_currentDownloadId + 
                        "> but it was called for <" + downloadableId + " instead");
                }

                return;
            }

            if (m_trackerInfo == null)
            {
                m_trackerInfo = new Dictionary<string, Dictionary<Error.EType, int>>();
            }

            if (!m_trackerInfo.ContainsKey(downloadableId))
            {
                m_trackerInfo.Add(downloadableId, new Dictionary<Error.EType, int>());
            }

            bool canLog = true;

            // Checks if the result must be reported (a type of error has a limit for the amount of times it can be reported)
            if (m_maxPerErrorType != null && error != Error.EType.None)
            {
                Dictionary<Error.EType, int> info = m_trackerInfo[downloadableId];
                if (info.ContainsKey(error))
                {
                    info[error]++;
                }
                else
                {
                    info.Add(error, 1);
                }

                canLog = info[error] <= m_maxPerErrorType[error];                
            }

            if (canLog)
            {
                EAction action = (m_currentDownloadIsUpdate) ? EAction.Update : EAction.Download;
                int timeSpent = (int)(currentTime - m_currentDownloadTimeAtStart);                
                TrackActionEnd(action, downloadableId, GetSizeInMb(m_currentDownloadExistingSizeAtStart), GetSizeInMb(existingSizeAtEnd),
                               GetSizeInMb(totalSize), timeSpent, m_currentDownloadReachabilityAtStart, reachabilityAtEnd, error);
            }

            m_currentDownloadTimeAtStart = -1f;
        }

        protected abstract void TrackActionEnd(EAction action, string downloadableId, float existingSizeMbAtStart, float existingSizeMbAtEnd, float totalSizeMb, int timeSpent,
                                             NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error);                

        protected bool CanLog()
        {
            return m_logger != null && m_logger.CanLog();
        }
    }
}