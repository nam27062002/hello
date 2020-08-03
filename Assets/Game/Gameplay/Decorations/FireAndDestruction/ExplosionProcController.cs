using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionProcController : MonoBehaviour {
	public Animator m_animator;
	public DisableInSeconds m_disableInSecs;
	public FireTypeAutoSelector m_fireAutoSelector;

	private float m_timer;

	private void Awake() {
		m_timer = 0f;
	}

	public void Explode(float _delay, FireColorSetupManager.FireColorType _color) {
		m_timer = _delay + 0.01f;
		m_disableInSecs.activeTime = 1f + _delay;
		m_fireAutoSelector.m_fireType = _color;
	}

	private void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_animator.SetTrigger( GameConstants.Animator.EXPLODE );
			}
		}
	}
}
