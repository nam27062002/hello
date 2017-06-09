using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionProcController : MonoBehaviour {
	private Animator m_animator;
	private DisableInSeconds m_disableInSecs;

	private float m_timer;

	private void Awake() {
		m_animator = GetComponent<Animator>();
		m_disableInSecs = GetComponent<DisableInSeconds>();
		m_timer = 0f;
	}

	public void Explode(float _delay) {
		m_timer = _delay;
		m_disableInSecs.activeTime = 1f + _delay;
	}

	private void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_animator.SetTrigger("explode");
			}
		}
	}
}
