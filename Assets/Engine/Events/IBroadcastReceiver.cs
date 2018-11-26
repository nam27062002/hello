using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBroadcastListener 
{
    void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo);
}
