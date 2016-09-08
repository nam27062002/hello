using UnityEngine;
using System.Collections;

namespace FGOL.Plugins
{
	/// <summary>
	/// Class to detect incoming phone calls on Android platform
	/// Cases:
	///		1. Phone call recieved, caller canceled
	///			onIncomingCallReceived -> onMissedCall
	///		2. Phone call recieved and answered
	///			onIncomingCallReceived -> onIncomingCallAnswered -> onOutgoingCallEnded
	///		3. Phone call recieved and reciever denied (same as #1)
	///			onIncomingCallReceived -> onMissedCall 
	/// </summary>
	public class FGOLPhoneCallReciever : MonoBehaviour
	{
		public enum CallState
		{
			Idle,
			Ringing,
			Answered,
			Missed,
			Ended
		}

		//	delegate for phone call updates
		public delegate void PhoneCallEvent(CallState state);

		/// <summary>
		/// Phone call update event. Listen to this to recieve updates
		/// Please note that CallState.Ringing can be called multiple times as ringing is in progress
		/// </summary>
		public event PhoneCallEvent OnPhoneCallStateUpdate;

#if UNITY_ANDROID

		void Awake()
		{
			bool init = false;
			string name = this.gameObject.name;

			using (var pluginClass = new AndroidJavaClass("com.fgol.FGOLPhonecallReciever"))
			{
				Debug.Log("Initalizing FGOLPhoneCallReciever " + name);

				pluginClass.CallStatic("Init", name);

				init = true;
			}

			if (!init)
			{
				Debug.LogError("Could not find FGOLPhoneCallReciever android plugin");
			}
		}

		void onIncomingCallReceived(string empty)
		{
			Debug.Log("onIncomingCallReceived");
			OnPhoneCallStateUpdate(CallState.Ringing);
		}

		void onIncomingCallAnswered(string empty)
		{
			Debug.Log("onIncomingCallAnswered");
			OnPhoneCallStateUpdate(CallState.Answered);
		}

		void onIncomingCallEnded(string empty)
		{
			Debug.Log("onIncomingCallEnded");
			OnPhoneCallStateUpdate(CallState.Ended);
		}

		void onMissedCall(string empty)
		{
			Debug.Log("onMissedCall");
			OnPhoneCallStateUpdate(CallState.Missed);
		}

		void onOutgoingCallStarted(string empty)
		{
			Debug.Log("onOutgoingCallStarted");
		}

		void onOutgoingCallEnded(string empty)
		{
			Debug.Log("onOutgoingCallEnded");
		}

#endif
	}

}