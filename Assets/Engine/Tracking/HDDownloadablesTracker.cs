using Downloadables;
using System;
using System.Collections.Generic;
using UnityEngine;
    
/// <summary>
/// This class is responsible for tracking downloadables related events in HD.
/// </summary>
public class HDDownloadablesTracker : Tracker
{
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
        return (type == Error.EType.None) ? "Success" : type.ToString();
    }

    public HDDownloadablesTracker(int maxAttempts, Dictionary<Error.EType, int> maxAttemptsPerErrorType, Logger logger) : base(maxAttempts, maxAttemptsPerErrorType, logger)
    {
    }

    protected override void TrackActionEnd(EAction action, string downloadableId, float existingSizeMbAtStart, float existingSizeMbAtEnd, float totalSizeMb, int timeSpent,
                                             NetworkReachability reachabilityAtStart, NetworkReachability reachabilityAtEnd, Error.EType error, bool maxAttemptsReached)
    {
        HDTrackingManager.Instance.Notify_DownloadablesEnd(action.ToString(), downloadableId, existingSizeMbAtStart, existingSizeMbAtEnd, totalSizeMb, timeSpent, 
                                                           ReachabilityToString(reachabilityAtStart), ReachabilityToString(reachabilityAtEnd), ErrorTypeToString(error), maxAttemptsReached);            
    }    
}
