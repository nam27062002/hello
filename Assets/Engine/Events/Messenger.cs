/*
 * Advanced C# messenger by Ilya Suzdalnitski. V1.0
 * 
 * Based on Rod Hyde's "CSharpMessenger" and Magnus Wolffelt's "CSharpMessenger Extended".
 * 
 * Features:
 	* Prevents a MissingReferenceException because of a reference to a destroyed message handler.
 	* Option to log all messages
 	* Extensive error detection, preventing silent bugs
 * 
 * Usage examples:
 	1. Messenger.AddListener<GameObject>("prop collected", PropCollected);
 	   Messenger.Broadcast<GameObject>("prop collected", prop);
 	2. Messenger.AddListener<float>("speed changed", SpeedChanged);
 	   Messenger.Broadcast<float>("speed changed", 0.5f);
 * 
 * Messenger cleans up its evenTable automatically upon loading of a new level.
 * 
 * Don't forget that the messages that should survive the cleanup, should be marked with Messenger.MarkAsPermanent(string)
 * 
 * Custom changes:
 * [AOC] Created flag to prevent listeners to be cleared on level change (problematic for singletons)
 * [AOC] Changed events Id from strings to enum for better performance
 */

//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE
//#define REQUIRE_LISTENER
//#define CLEAR_LISTENERS_ON_LEVEL_CHANGE

using System;
using System.Collections.Generic;
using UnityEngine;

static internal class Messenger {
	#region Internal variables

	#if CLEAR_LISTENERS_ON_LEVEL_CHANGE
	//Disable the unused variable warning
	#pragma warning disable 0414
	//Ensures that the MessengerHelper will be created automatically upon start of the game.
	static private MessengerHelper messengerHelper = ( new GameObject("MessengerHelper") ).AddComponent< MessengerHelper >();
	#pragma warning restore 0414
	#endif
	
	// static public Dictionary<Enum, Delegate> eventTable = new Dictionary<Enum, Delegate>();
	static public Delegate[] eventTable = new Delegate[ (int)MessengerEvents.COUNT ];

	#endregion
	#region Helper methods

	static Messenger()
    {
        Cleanup();
    }
	
	static public void Cleanup()
	{
		#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER Cleanup. Make sure that none of necessary listeners are removed.");
		#endif

		int count = (int)MessengerEvents.COUNT;
		for( int i = 0; i<count; ++i )
			eventTable[i] = null;
	}
	
	static public void PrintEventTable()
	{
		Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

		int count = (int)MessengerEvents.COUNT;
		for( int i = 0; i< count; ++i)
		{
			Debug.Log("\t\t\t" + (MessengerEvents)i + "\t\t" + eventTable[ i ]);
		}
		Debug.Log("\n");
	}
	#endregion
	
	#region Message logging and exception throwing
	static public void OnListenerAdding(MessengerEvents eventType, Delegate listenerBeingAdded) {
		#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
		Debug.Log("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
		#endif

		#if UNITY_DEBUG
		Delegate d = eventTable[eventType];
		if (d != null && d.GetType() != listenerBeingAdded.GetType()) {
			throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
		}
		#endif
	}
	
	static public void OnListenerRemoving(MessengerEvents eventType, Delegate listenerBeingRemoved) {
		#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
		#endif
		
		#if UNITY_DEBUG
		Delegate d = eventTable[eventType];
		if (d == null) {
			throw new ListenerException(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null.", eventType));
		} else if (d.GetType() != listenerBeingRemoved.GetType()) {
			throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, d.GetType().Name, listenerBeingRemoved.GetType().Name));
		}
		#endif
		
	}
	
	static public void OnBroadcasting(MessengerEvents eventType) {
		#if REQUIRE_LISTENER
		if (eventTable[ (int)eventType ] == null) {
			throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventType));
		}
		#endif
	}
	
	static public BroadcastException CreateBroadcastSignatureException(Enum eventType) {
		return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
	}
	
	public class BroadcastException : Exception {
		public BroadcastException(string msg)
		: base(msg) {
		}
	}
	
	public class ListenerException : Exception {
		public ListenerException(string msg)
		: base(msg) {
		}
	}
	#endregion
	
	#region AddListener
	//No parameters
	static public void AddListener(MessengerEvents eventType, UbiBCN.Callback handler) {
		OnListenerAdding(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback)eventTable[(int)eventType] + handler;
	}
	
	//Single parameter
	static public void AddListener<T>(MessengerEvents eventType, UbiBCN.Callback<T> handler) {
		OnListenerAdding(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback<T>)eventTable[(int)eventType] + handler;
	}
	
	//Two parameters
	static public void AddListener<T, U>(MessengerEvents eventType, UbiBCN.Callback<T, U> handler) {
		OnListenerAdding(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback<T, U>)eventTable[(int)eventType] + handler;
	}
	
	//Three parameters
	static public void AddListener<T, U, V>(MessengerEvents eventType, UbiBCN.Callback<T, U, V> handler) {
		OnListenerAdding(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback<T, U, V>)eventTable[(int)eventType] + handler;
	}

    //Four parameters
    static public void AddListener<T, U, V, W>(MessengerEvents eventType, UbiBCN.Callback<T, U, V, W> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[(int)eventType] = (UbiBCN.Callback<T, U, V, W>)eventTable[(int)eventType] + handler;
    }
    #endregion

    #region RemoveListener
    //No parameters
    static public void RemoveListener(MessengerEvents eventType, UbiBCN.Callback handler) {
		OnListenerRemoving(eventType, handler);   
		eventTable[(int)eventType] = (UbiBCN.Callback)eventTable[(int)eventType] - handler;
	}
	
	//Single parameter
	static public void RemoveListener<T>(MessengerEvents eventType, UbiBCN.Callback<T> handler) {
		OnListenerRemoving(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback<T>)eventTable[(int)eventType] - handler;
	}
	
	//Two parameters
	static public void RemoveListener<T, U>(MessengerEvents eventType, UbiBCN.Callback<T, U> handler) {
		OnListenerRemoving(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback<T, U>)eventTable[(int)eventType] - handler;
	}
	
	//Three parameters
	static public void RemoveListener<T, U, V>(MessengerEvents eventType, UbiBCN.Callback<T, U, V> handler) {
		OnListenerRemoving(eventType, handler);
		eventTable[(int)eventType] = (UbiBCN.Callback<T, U, V>)eventTable[(int)eventType] - handler;
	}

    //Three parameters
    static public void RemoveListener<T, U, V, W>(MessengerEvents eventType, UbiBCN.Callback<T, U, V, W> handler)
    {
        OnListenerRemoving(eventType, handler);
        eventTable[(int)eventType] = (UbiBCN.Callback<T, U, V, W>)eventTable[(int)eventType] - handler;
    }
    #endregion

    #region Broadcast
    //No parameters
    static public void Broadcast(MessengerEvents eventType) {
		#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
		#endif
		OnBroadcasting(eventType);

		UbiBCN.Callback callback = eventTable[ (int)eventType ] as UbiBCN.Callback;
		if (callback != null) {
			callback();
		} else {
			// throw CreateBroadcastSignatureException(eventType);
		}
		
	}
	
	//Single parameter
	static public void Broadcast<T>(MessengerEvents eventType, T arg1) {
		#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
		#endif
		OnBroadcasting(eventType);

		UbiBCN.Callback<T> callback = eventTable[ (int)eventType ] as UbiBCN.Callback<T>;
		if (callback != null) {
			callback(arg1);
		} else {
			// throw CreateBroadcastSignatureException(eventType);
		}
	}
	
	//Two parameters
	static public void Broadcast<T, U>(MessengerEvents eventType, T arg1, U arg2) {
		#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
		#endif
		OnBroadcasting(eventType);

		UbiBCN.Callback<T, U> callback = eventTable[ (int)eventType ] as UbiBCN.Callback<T, U>;
		
		if (callback != null) {
			callback(arg1, arg2);
		} else {
			// throw CreateBroadcastSignatureException(eventType);
		}
		
	}
	
	//Three parameters
	static public void Broadcast<T, U, V>(MessengerEvents eventType, T arg1, U arg2, V arg3) {
		#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
		#endif
		OnBroadcasting(eventType);

		UbiBCN.Callback<T, U, V> callback = eventTable[ (int)eventType ] as UbiBCN.Callback<T, U, V>;
		
		if (callback != null) {
			callback(arg1, arg2, arg3);
		} else {
			// throw CreateBroadcastSignatureException(eventType);
		}
		
	}

    //Three parameters
    static public void Broadcast<T, U, V, W>(MessengerEvents eventType, T arg1, U arg2, V arg3, W arg4)
    {
        #if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
        #endif
        OnBroadcasting(eventType);

        UbiBCN.Callback<T, U, V, W> callback = eventTable[(int)eventType] as UbiBCN.Callback<T, U, V, W>;

        if (callback != null)
        {
            callback(arg1, arg2, arg3, arg4);
        }
        else
        {
            // throw CreateBroadcastSignatureException(eventType);
        }

    }
    #endregion
}

//This manager will ensure that the messenger's eventTable will be cleaned up upon loading of a new level.
#if CLEAR_LISTENERS_ON_LEVEL_CHANGE
public sealed class MessengerHelper : MonoBehaviour {
	void Awake ()
	{
		DontDestroyOnLoad(gameObject);	
	}
	
	//Clean up eventTable every time a new level loads.
	public void OnLevelWasLoaded(int unused) {
		Messenger.Cleanup();
	}
}
#endif
