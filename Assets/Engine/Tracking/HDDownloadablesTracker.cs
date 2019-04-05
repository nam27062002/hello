using Downloadables;
using System;
using System.Collections.Generic;
using UnityEngine;
    
/// <summary>
/// This class is responsible for tracking downloadables related events in HD.
/// </summary>
public class HDDownloadablesTracker : Tracker
{
	public static string RESULT_SUCCESS = "Success";

	private static string[] sm_reachabilityAsString;

    private static string ReachabilityToString(NetworkReachability reachability)
    {
        if (sm_reachabilityAsString == null)
        {
            Array values = Enum.GetValues(typeof(NetworkReachability));
            int count = values.Length;
            sm_reachabilityAsString = new string[count];

            string valueAsText;
            for (int i = 0; i < count; i++)
            {
                switch ((NetworkReachability)i)
                {
                    case NetworkReachability.NotReachable:
                        valueAsText = "None";
                        break;

                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        valueAsText = "Mobile";
                        break;

                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        valueAsText = "WIFI";
                        break;

                    default:
                        valueAsText = "Unknown";
                        break;
                }

                sm_reachabilityAsString[i] = valueAsText;
            }
        }

        return sm_reachabilityAsString[(int)reachability];
    }

    private static string ErrorTypeToString(Error.EType type)
    {
		return (type == Error.EType.None) ? RESULT_SUCCESS : type.ToString();
    }

    private Dictionary<string, bool> m_idsLoadTracked;

    public HDDownloadablesTracker(Downloadables.Config config, Logger logger) : base(config, logger)
    {
    }

    public void ResetIdsLoadTracked()
    {
        if (m_idsLoadTracked != null)
        {
            m_idsLoadTracked.Clear();
        }
    }

    public override void TrackActionStart(EAction action, string downloadableId, long existingSizeAtStart, long totalSize)
    {
        HDTrackingManager.Instance.Notify_DownloadablesStart(action, downloadableId, existingSizeAtStart, totalSize);        
    }

    public override void TrackActionEnd(EAction action, string downloadableId, long existingSizeAtStart, long existingSizeAtEnd, long totalSize, int timeSpent,
                                             NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached)
    {
        bool canTrack = true;
        if (action == EAction.Load)
        {
            canTrack = m_idsLoadTracked == null || !m_idsLoadTracked.ContainsKey(downloadableId);          
        }

        if (canTrack)
        {
            if (action == EAction.Load)
            {
                if (m_idsLoadTracked == null)
                {
                    m_idsLoadTracked = new Dictionary<string, bool>();
                }

                m_idsLoadTracked.Add(downloadableId, true);
            }

            HDTrackingManager.Instance.Notify_DownloadablesEnd(action, downloadableId, existingSizeAtStart, existingSizeAtEnd, totalSize, timeSpent,
                                                                ReachabilityToString(reachabilityAtStart), ReachabilityToString(reachabilityAtEnd), ErrorTypeToString(error), maxAttemptsReached);
        }
    }    
}
