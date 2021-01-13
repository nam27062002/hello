using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof( AudioSource ))]
public class PauseSoundOnPause : MonoBehaviour, IBroadcastListener {

	public AudioSource m_audio;
	// Use this for initialization
	void Awake () 
	{
		Broadcaster.AddListener(BroadcastEventType.GAME_PAUSED, this);
	}

	void OnDestroy()
	{
		Broadcaster.RemoveListener(BroadcastEventType.GAME_PAUSED, this);
	}

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
	{
		bool pause = (broadcastEventInfo as ToggleParam).value;
		if (pause)
		{
			m_audio.Pause();
		}
		else
		{
			m_audio.Play();
		}
	}

}
