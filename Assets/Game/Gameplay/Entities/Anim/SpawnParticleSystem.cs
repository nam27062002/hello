﻿using UnityEngine;

public class SpawnParticleSystem : StateMachineBehaviour {

	[SerializeField] private ParticleData m_particleSystemData;
	[SerializeField] private float m_delay = 0f;
	[SerializeField] private bool m_attach = true;

	private float m_timer = 0f;

	// Use this for initialization
	void Start () {	
		ParticleManager.CreatePool(m_particleSystemData);
		m_timer = 0f;
	}
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_delay > 0f) {
			m_timer = m_delay;
		} else {
			SpawnParticle(animator.transform);
		}
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				SpawnParticle(animator.transform);
			}
		}
	}

	private void SpawnParticle(Transform _parent) {		
		GameObject ps = ParticleManager.Spawn(m_particleSystemData);
		if (ps != null) {
			ps.transform.localScale = Vector3.one;
			ps.transform.localRotation = Quaternion.identity;
			ps.transform.localPosition = Vector3.zero;

			if (m_attach) {
				ps.transform.SetParent(_parent, false);
				ps.transform.localPosition = m_particleSystemData.offset;
			} else {
				ps.transform.position = _parent.position + m_particleSystemData.offset;
			}
		}
	}
}
