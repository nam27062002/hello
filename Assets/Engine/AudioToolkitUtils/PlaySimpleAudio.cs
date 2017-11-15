using UnityEngine;
using System.Collections;

namespace UbiBCN{
	public class PlaySimpleAudio : MonoBehaviour {

		public string m_audioId;
		public void Play () {
			Play(m_audioId);
		}

		public void Play(string _id) {
			if(!string.IsNullOrEmpty(_id)) {
				DebugUtils.Log("SFX <color=magenta>" + _id + "</color>", this);
				AudioController.Play(_id);
			} else {
				DebugUtils.Log("SFX <color=red>NULL!</color>", this);
			}
		}
	}
}
