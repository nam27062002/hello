using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    public class DummyTracker : Tracker
    {
        public DummyTracker(Config config, Logger logger) : base(config, logger)
        {
        }

        public override void TrackActionStart(EAction action, string downloadableId, long existingSizeAtStart, long totalSize)
        {
            if (CanLog())
            {
                m_logger.Log("ActionStart " + action.ToString() + " abName = " + downloadableId + " existingSizeAtStart = " + existingSizeAtStart + " totalSize = " + totalSize);
            }
        }

        public override void TrackActionEnd(EAction action, string downloadableId, long existingSizeAtStart, long existingSizeAtEnd, long totalSize, int timeSpent,
                                              NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached)
        { 
            if (CanLog())
            {
                m_logger.Log("ActionStart " + action.ToString() + " abName = " + downloadableId + " existingSizeAtStart = " + existingSizeAtStart + " existingSizeAtEnd = " + existingSizeAtEnd + 
                    " totalSize = " + totalSize + " timeSpent = " + timeSpent + " reachabilityAtStart = " + reachabilityAtStart.ToString() + " reachabilityAtEnd = " + reachabilityAtEnd.ToString() + 
                    " result = " + error.ToString() + " maxAttemptsReached = " + maxAttemptsReached);
            }
        }        
    }
}