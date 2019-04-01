using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Broadcaster
{
    public static List<IBroadcastListener>[] m_listeners = new List<IBroadcastListener>[ (int)BroadcastEventType.COUNT ];

    public static BroadcastEventInfo m_defaultInfo = new BroadcastEventInfo();
    
    static Broadcaster()
    {
        int max = m_listeners.Length;
        for (int i = 0; i < max; i++)
        {
            m_listeners[i] = new List<IBroadcastListener>();
        }
    }
    public static void AddListener( BroadcastEventType type, IBroadcastListener listener )
    {
        m_listeners[(int)type].Add( listener );
    }
    
    public static void RemoveListener( BroadcastEventType type, IBroadcastListener listener )
    {
        m_listeners[(int)type].Remove( listener );
    }
    
    public static void Broadcast( BroadcastEventType type )
    {
        List<IBroadcastListener> listeners = m_listeners[(int)type];
        int start = listeners.Count - 1 ;
        for (int i = start; i >= 0; --i)
        {
            listeners[i].OnBroadcastSignal(type, m_defaultInfo);
        }
    }
    
    public static void Broadcast( BroadcastEventType type, BroadcastEventInfo info )
    {
        List<IBroadcastListener> listeners = m_listeners[(int)type];
        int start = listeners.Count - 1 ;
        for (int i = start; i >= 0; --i)
        {
            listeners[i].OnBroadcastSignal(type, info);
        }
    }

}
