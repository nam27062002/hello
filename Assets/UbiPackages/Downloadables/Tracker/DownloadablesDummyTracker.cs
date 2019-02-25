using UnityEngine;

namespace Downloadables
{
    public class DummyTracker : Tracker
    {
        private Logger m_logger;

        public DummyTracker(Logger logger)
        {
            m_logger = logger;
        }

        public override void Track_ActionStart(EAction action, string assetBundleName, float existingSizeMb, float totalSizeMb, NetworkReachability reachability)
        {
            if (CanLog())
            {
                m_logger.Log("ActionStart " + action.ToString() + " abName = " + assetBundleName + " existingSizeMb = " + existingSizeMb + " totalSizeMb = " + totalSizeMb + " reachability = " + reachability.ToString());
            }
        }

        public override void Track_ActionEnd(EAction action, string assetBundleName, float existingSizeMb, float totalSizeMb, int timeSpent, NetworkReachability reachability, Error error)
        {
            if (CanLog())
            {
                m_logger.Log("ActionStart " + action.ToString() + " abName = " + assetBundleName + " existingSizeMb = " + existingSizeMb + " totalSizeMb = " + totalSizeMb + " timeSpent = " + timeSpent +
                    " reachability = " + reachability.ToString() + " result = " + ((error == null) ? " Success " : error.Type.ToString()));
            }
        }

        private bool CanLog()
        {
            return m_logger != null && m_logger.CanLog();
        }
    }
}