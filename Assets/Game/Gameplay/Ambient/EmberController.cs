using UnityEngine;
using System.Collections;

public class EmberController : MonoBehaviour {

	ParticleSystem m_emberParticle;

	// Use this for initialization
	void Start () {
		m_emberParticle = GetComponent<ParticleSystem>();
		m_emberParticle.Stop();

		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
	}

	void OnDisable() {
		Messenger.RemoveListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
	}

	private void OnFuryToggled(bool _value) {
		if (_value) {
			m_emberParticle.Clear();
			m_emberParticle.Play();
		} else {
			m_emberParticle.Stop();
		}
	}
}
