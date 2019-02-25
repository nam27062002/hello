using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
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

        private struct TrackerInfo
        {
            /// <summary>
            /// Key: error type, Value: amount of times this error has happened when downloading this catalog entry
            /// </summary>
            public Dictionary<Error, int> m_errorTimes;
        }

        public abstract void Track_ActionStart(EAction action, string assetBundleName, float existingSizeMb, float totalSizeMb, NetworkReachability reachability);
        public abstract void Track_ActionEnd(EAction action, string assetBundleName, float existingSizeMb, float totalSizeMb, int timeSpent, NetworkReachability reachability, Error error);
    }
}