using UnityEngine;
using System.Collections.Generic;
using FGOL.Events;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct DanglingEventInfo
{

    public string eventName;
    public string function;
    public string caller;
    public string target;
}

public class EventDeregistrationValidator : MonoBehaviour {

    Dictionary<Enum, Action<Enum, object[]>> m_events = new Dictionary<Enum, Action<Enum, object[]>>();

    [HideInInspector]
    public List<DanglingEventInfo> liveEvents = new List<DanglingEventInfo>();

    void Start( )
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }


	void OnDestroy () {
         CheckEventsDeregistration();
    }

    public void SimulateAppQuit( )
    {
#if UNITY_EDITOR
        GameObject[] allObjectsInGame = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

        List<GameObject> gameObjectsToDelete = new List<GameObject>();
        for (int i = allObjectsInGame.Length - 1; i >= 0; i--)
        {
            if (allObjectsInGame[ i ] != gameObject)
            {
                if (allObjectsInGame[i] != null && allObjectsInGame[i] != null && allObjectsInGame[i].transform.parent == null )
                {
                    if (allObjectsInGame[i].hideFlags == HideFlags.NotEditable || allObjectsInGame[i].hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    PrefabType prefabType = PrefabUtility.GetPrefabType(allObjectsInGame[i]);
                    if (prefabType == PrefabType.Prefab)
                    {
                        continue;
                    }

                    //Object is not a prefab and it's a valid obejct, mark it for deletion
                    gameObjectsToDelete.Add(allObjectsInGame[i].gameObject);
                }
            }
        }

        for (int i = 0;  i < gameObjectsToDelete.Count; ++i)
        {
            Debug.Log(allObjectsInGame[i].gameObject.name);
            GameObject.Destroy(gameObjectsToDelete[i].gameObject);
        }

        Application.Quit();
#endif
    }

    public void GetRegisteredEvents( bool verbose )
    {
        liveEvents.Clear();
        m_events = EventManager.Instance.GetRegisteredEvents();
        foreach (KeyValuePair<Enum, Action<Enum, object[]>> kvp in m_events)
        {
            if (m_events[kvp.Key] != null)
            {
                Action<Enum, object[]> func = m_events[kvp.Key];
                Delegate[] callers = func.GetInvocationList();
                if (verbose == false)  //Only show an event only once ( even if he has multiple listeners )
                {
                    DanglingEventInfo danglingEventInfo = CreateDanglingEventInfo(kvp.Key, func.Method, func.Target);
                    liveEvents.Add(danglingEventInfo);
                }
                else   //Show all events.  once for each of the multiple listeners it might have
                {

                    for (int i = 0; i < callers.Length; ++i)
                    {
                        DanglingEventInfo danglingEventInfo = CreateDanglingEventInfo(kvp.Key, callers[i].Method, callers[i].Target);
                        liveEvents.Add(danglingEventInfo);
                    }
                }

            }
        }
    }

    public DanglingEventInfo CreateDanglingEventInfo(Enum eventType, MethodInfo methodInfo, object target )
    {
        DanglingEventInfo registeredEvent = new DanglingEventInfo();
        registeredEvent.eventName = "" + eventType;
        registeredEvent.caller = "" + methodInfo.DeclaringType;
        registeredEvent.function = "" + methodInfo.Name;
        if (target != null)
        {
            registeredEvent.target = target.ToString();
        }
        else
        {
            registeredEvent.target = "Static"; // If target is null here, it means it comes from a static class like GUIShark or similar
        }

        return registeredEvent;
    }

    public void CheckEventsDeregistration()
    {
        m_events = EventManager.Instance.GetRegisteredEvents();
        foreach (KeyValuePair<Enum, Action<Enum, object[]>> kvp in m_events)
        {
            if (m_events[kvp.Key] != null)
            {
                Action<Enum, object[]> func = m_events[kvp.Key];

                Debug.LogError("Event not de-registered! Event:" + kvp.Key + ". Function :" + func.Method.Name + ". In class: " + func.Method.DeclaringType);
            }
        }
    }


}
