using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    public class DummyTracker : Tracker
    {
        public DummyTracker(Dictionary<Error.EType, int> maxPerErrorType, Logger logger) : base(maxPerErrorType, logger)
        {
        }

        protected override void TrackActionEnd(EAction action, string downloadableId, float existingSizeMbAtStart, float existingSizeMbAtEnd, float totalSizeMb, int timeSpent,
                                              NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error)
        { 
            if (CanLog())
            {
                m_logger.Log("ActionStart " + action.ToString() + " abName = " + downloadableId + " existingSizeMbAtStart = " + existingSizeMbAtStart + " existingSizeMbAtEnd = " + existingSizeMbAtEnd + 
                    " totalSizeMb = " + totalSizeMb + " timeSpent = " + timeSpent + " reachabilityAtStart = " + reachabilityAtStart.ToString() + " reachabilityAtEnd = " + reachabilityAtEnd.ToString() + 
                    " result = " + (error.ToString()));
            }
        }        
    }
}