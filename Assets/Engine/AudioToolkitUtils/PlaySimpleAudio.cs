using UnityEngine;
using System.Collections;

namespace UbiBCN{
	public class PlaySimpleAudio : MonoBehaviour {

		public string m_audioId;
		public void Play () {
			if ( !string.IsNullOrEmpty(m_audioId) )
				AudioController.Play(m_audioId);
		}
	}
}
