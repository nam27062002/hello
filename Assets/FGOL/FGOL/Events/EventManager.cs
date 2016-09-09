﻿using System;
using FGOL.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace FGOL.Events
{
    public class EventManager : AutoGeneratedSingleton<EventManager>
    {
        Dictionary<Enum, Action<Enum, object[]>> m_events = new Dictionary<Enum, Action<Enum, object[]>>();

        public void RegisterEvent(Enum eventType, Action<Enum, object[]> listener)
        {
            if(!m_events.ContainsKey(eventType))
            {
                m_events.Add(eventType, listener);
            }
            else
            {
                Action<Enum, object[]> events = m_events[eventType];

                if(events == null || Array.IndexOf(events.GetInvocationList(), listener) == -1)
                {
                    m_events[eventType] += listener;
                }
                else
                {
                    Debug.LogError("EventManager (RegisterEvent) :: Duplicate event listener: " + listener.Target.ToString() + "." + listener.Method.ToString());
                }
            }
        }

        public void TriggerEvent(Enum eventType, params object[] optParams)
        {
            if(m_events.ContainsKey(eventType) && m_events[eventType] != null)
            {
                m_events[eventType](eventType, optParams);
            }
        }

        public void DeregisterEvent(Enum eventType, Action<Enum, object[]> listener)
        {
            if(m_events.ContainsKey(eventType))
            {
                m_events[eventType] -= listener;
            }
        }

       

        public Dictionary<Enum, Action<Enum, object[]>> GetRegisteredEvents( )
        {
            return m_events;
        }
    }
}