using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UbiBCN{
	public class PlaySimpleIndexedAudio : MonoBehaviour {

		public List<string> m_audios;
		public void Play (int index) {
			if (  index < m_audios.Count && index >= 0 && !string.IsNullOrEmpty( m_audios[index] ) )
				AudioController.Play(m_audios[index]);
		}
	}
}
