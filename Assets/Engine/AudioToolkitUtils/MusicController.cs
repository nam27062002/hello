using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour {

	public string m_music = "Music_Test";
	public float m_volume = 0.1f;
	// Use this for initialization
	void Awake () {
        Messenger.AddListener<string>(EngineEvents.SCENE_LOADED, OnSceneLoaded);
        Messenger.AddListener<string>(EngineEvents.SCENE_PREUNLOAD, OnScenePreunload);        
	}

    void OnDestroy() {
        Messenger.RemoveListener<string>(EngineEvents.SCENE_LOADED, OnSceneLoaded);
        Messenger.RemoveListener<string>(EngineEvents.SCENE_PREUNLOAD, OnScenePreunload);
    }
	
    private void OnSceneLoaded(string scene) {
        AudioController.PlayMusic(m_music, m_volume);
    }

    private void OnScenePreunload(string scene) {
        AudioController.StopMusic(0.3f);        
    }    
}
