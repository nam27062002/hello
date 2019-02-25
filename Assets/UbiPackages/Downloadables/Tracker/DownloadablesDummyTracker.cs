using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    public class DummyTracker : Tracker
    {
        public DummyTracker(int maxAttempts, Dictionary<Error.EType, int> maxAttemptsPerErrorType, Logger logger) : base(maxAttempts, maxAttemptsPerErrorType, logger)
        {
        }

        protected override void TrackActionEnd(EAction action, string downloadableId, float existingSizeMbAtStart, float existingSizeMbAtEnd, float totalSizeMb, int timeSpent,
                                              NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached)
        { 
            if (CanLog())
            {
                m_logger.Log("ActionStart " + action.ToString() + " abName = " + downloadableId + " existingSizeMbAtStart = " + existingSizeMbAtStart + " existingSizeMbAtEnd = " + existingSizeMbAtEnd + 
                    " totalSizeMb = " + totalSizeMb + " timeSpent = " + timeSpent + " reachabilityAtStart = " + reachabilityAtStart.ToString() + " reachabilityAtEnd = " + reachabilityAtEnd.ToString() + 
                    " result = " + error.ToString() + " maxAttemptsReached = " + maxAttemptsReached);
            }
        }        
    }
}