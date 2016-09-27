using UnityEngine;
using System.Collections;

public class PlaySimpleAudio : MonoBehaviour {

	public string m_audioId;
	public void Play () {
		AudioController.Play(m_audioId);
	}
}
