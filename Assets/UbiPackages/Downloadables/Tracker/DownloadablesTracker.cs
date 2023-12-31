﻿using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    namespace Popup {
        public enum EAction {
            Dismiss = 0,
            Wifi_Only,
            Wifi_Mobile,
            View_Storage_Options
        };
    }

    /// <summary>
    /// This class is responsible for defining the interface of the class that will be notified every time a relevant action related to downloadables happens
    /// </summary>
    public abstract class Tracker
    {        
        public enum EAction
        {
            Download,
            Update,
            Load
        };
        
        private Config Config;

        /// <summary>
        /// Key: downloadable id; Value: TrackierInfo containing tracking related information of this downloadable
        /// </summary>
        private Dictionary<string, Dictionary<Error.EType, int>> m_trackerInfo;

        protected Logger m_logger;

        private bool m_currentDownloadIsUpdate;
        private float m_currentDownloadTimeAtStart;
        private string m_currentDownloadId;
        private long m_currentDownloadExistingSizeAtStart;
        private NetworkReachability m_currentDownloadReachabilityAtStart;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxAttempts">Max amount of attempts allowed per error type</param>        
        /// <param name="logger"></param>
        public Tracker(Config config, Logger logger)
        {
            Config = config; 
            m_currentDownloadTimeAtStart = -1;
        }                

        /// <summary>
        /// Notifies a download has started
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="downloadableId"></param>
        /// <param name="existingSizeAtStart">Size in bytes right before starting to download.</param>
        /// <param name="totalSize">Total size in bytes.</param>
        /// <param name="reachabilityAtStart"></param>
        /// <param name="isUpdate"></param>
        public void NotifyDownloadStart(float currentTime, string downloadableId, long existingSizeAtStart, long totalSize, NetworkReachability reachabilityAtStart, bool isUpdate)
        {
            m_currentDownloadTimeAtStart = currentTime;
            m_currentDownloadId = downloadableId;
            m_currentDownloadExistingSizeAtStart = existingSizeAtStart;
            m_currentDownloadReachabilityAtStart = reachabilityAtStart;
            m_currentDownloadIsUpdate = isUpdate;

            // The event is not tracked here because the end event might not be tracked if an error happens if that error type max limit was met. That's why we need to track
            // the start event along with the end event in order to avoid a start event without the corresponding end event.            
        }

        public void NotifyDownloadEnd(float currentTime, string downloadableId, long existingSizeAtEnd,  long totalSize, NetworkReachability reachabilityAtEnd, Error.EType error)
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
            bool maxReached = false;

            int maxAttempts = Config.MaxTrackingEventsPerErrorType;

            // Checks if the result must be reported (a type of error has a limit for the amount of times it can be reported)
            ErrorConfig errorConfig = Config.GetErrorConfig(error);
            if (errorConfig != null)
            {
                maxAttempts = errorConfig.MaxTimesPerSession;
            }            
            
            Dictionary<Error.EType, int> info = m_trackerInfo[downloadableId];
            if (error != Error.EType.None)
            {
                if (info.ContainsKey(error))
                {
                    info[error]++;
                }
                else
                {
                    info.Add(error, 1);
                }

                canLog = info[error] <= maxAttempts;
                maxReached = info[error] >= maxAttempts;
            }
            
            if (canLog)
            {
                // The start event is tracked here in order to make sure there won't be any start event without its end eventnt.            
                TrackActionStart((m_currentDownloadIsUpdate) ? EAction.Update : EAction.Download, downloadableId, m_currentDownloadExistingSizeAtStart, totalSize);
                  
               // End event          
                EAction action = (m_currentDownloadIsUpdate) ? EAction.Update : EAction.Download;
                int timeSpent = (int)(currentTime - m_currentDownloadTimeAtStart);                
                TrackActionEnd(action, downloadableId, m_currentDownloadExistingSizeAtStart, existingSizeAtEnd, totalSize, timeSpent, 
                    m_currentDownloadReachabilityAtStart, reachabilityAtEnd, error, maxReached);
            }

            m_currentDownloadTimeAtStart = -1f;
        }      

        public abstract void TrackActionStart(EAction action, string downloadableId, long existingSize, long totalSize);

        public abstract void TrackActionEnd(EAction action, string downloadableId, long existingSize, long existingSizeAtEnd, long totalSize, int timeSpent,
                                             NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached);                

        protected bool CanLog()
        {
            return m_logger != null && m_logger.CanLog();
        }
    }
}