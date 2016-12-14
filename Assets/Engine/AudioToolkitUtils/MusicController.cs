using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour {

	public string m_music = "Music_Test";
	public float m_volume = 0.1f;
	public AudioMixerSnapshot m_defaultAudioSnapshot;
	private List<AudioMixerSnapshot> m_currentSnapshots = new List<AudioMixerSnapshot>();
	private AudioMixerSnapshot m_usingSnapshot = null;
	private const float m_transitionDuration = 0.1f;

	// Use this for initialization
	void Awake () {
        Messenger.AddListener<string>(EngineEvents.SCENE_LOADED, OnSceneLoaded);
        Messenger.AddListener<string>(EngineEvents.SCENE_PREUNLOAD, OnScenePreunload);        
        InstanceManager.musicController = this;
	}

    void OnDestroy() {
        Messenger.RemoveListener<string>(EngineEvents.SCENE_LOADED, OnSceneLoaded);
        Messenger.RemoveListener<string>(EngineEvents.SCENE_PREUNLOAD, OnScenePreunload);
		InstanceManager.musicController = null;
    }
	
    private void OnSceneLoaded(string scene) {
        AudioController.PlayMusic(m_music, m_volume);
    }

    private void OnScenePreunload(string scene) {
        AudioController.StopMusic(0.3f);        
    }    

	public void RegisterSnapshot( AudioMixerSnapshot snapshot )
    {
		m_currentSnapshots.Add( snapshot );
		snapshot.TransitionTo(m_transitionDuration);
		m_usingSnapshot = snapshot;
    }

	public void UnregisterSnapshot( AudioMixerSnapshot snapshot )
    {
    	for( int i = m_currentSnapshots.Count-1; i >= 0; i-- )
    	{
    		if ( m_currentSnapshots[i] == snapshot )
    		{
    			m_currentSnapshots.RemoveAt(i);
    			break;
    		}
    	}

    	if ( m_currentSnapshots.Count > 0 )
    	{
			if ( m_usingSnapshot != m_currentSnapshots [m_currentSnapshots.Count-1] )
			{
				m_usingSnapshot = m_currentSnapshots [m_currentSnapshots.Count-1];
				m_usingSnapshot.TransitionTo(m_transitionDuration);
			}
		}
		else
		{
			if ( m_usingSnapshot != m_defaultAudioSnapshot )
			{
				m_usingSnapshot = m_defaultAudioSnapshot;
				m_usingSnapshot.TransitionTo(m_transitionDuration);
			}
		}
    }
}
