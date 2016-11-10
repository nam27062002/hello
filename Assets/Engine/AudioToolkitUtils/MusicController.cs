using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour {

	public string m_music = "Music_Test";
	public float m_volume = 0.1f;
	// Use this for initialization
	void Start () {
		AudioController.PlayMusic(m_music, m_volume);
	}
	
	void OnDestroy(){
		AudioController.StopMusic(0.3f);
	}
}
