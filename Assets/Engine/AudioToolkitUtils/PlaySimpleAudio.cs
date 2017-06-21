using UnityEngine;
using System.Collections;

namespace UbiBCN{
	public class PlaySimpleAudio : MonoBehaviour {

		public string m_audioId;
		public void Play () {
			Play(m_audioId);
		}

		public void Play(string _id) {
			if ( !string.IsNullOrEmpty(m_audioId) )
				AudioController.Play(m_audioId);
		}
	}
}
