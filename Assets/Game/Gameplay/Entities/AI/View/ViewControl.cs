using UnityEngine;
using System.Collections.Generic;

public class ViewControl : MonoBehaviour {

	[SerializeField] private float m_walkSpeed = 1f;
	[SerializeField] private float m_runSpeed = 1f;

	[SeparatorAttribute]
	[SerializeField] private List<string> m_onEatenParticles = new List<string>();


	private Animator m_animator;

	private bool m_scared;
	private bool m_panic; //bite and hold state
	private bool m_attack;


	// Use this for initialization
	void Start () {
		m_animator = transform.FindComponentRecursive<Animator>();

		m_scared = false;
		m_panic = false;
		m_attack = false;
	}
	//


	// Queries
	public bool canAttack() {
		return !m_attack;
	}

	public bool hasAttackEnded() {
		return false;
	}
	//


	//Particles
	public void SpawnEatenParticlesAt(Transform _transform) {
		if (m_onEatenParticles.Count <= 0) {
			GameObject go = ParticleManager.Spawn("PS_Blood_Explosion_Small", transform.position + (Vector3.back * 10), "Blood/");
			if (go != null) {
				FollowTransform ft = go.GetComponent<FollowTransform>();
				if (ft != null) {
					ft.m_follow = _transform;
					ft.m_offset = Vector3.back * 10;
				}
			}
		} else {
			for( int i = 0; i < m_onEatenParticles.Count; i++) {
				if (!string.IsNullOrEmpty(m_onEatenParticles[i])) {
					GameObject go = ParticleManager.Spawn(m_onEatenParticles[i], transform.position);
					if (go != null)	{
						FollowTransform ft = go.GetComponent<FollowTransform>();
						if (ft != null)
							ft.m_follow = _transform;
					}
				}
			}
		}
	}


	// Animations
	public void Aim(float _blendFactor) {
		m_animator.SetFloat("aim", _blendFactor);
	}

	public void Move(float _speed) {
		if (m_panic)
			return;

		if (_speed > 0.01f) {
			// 0- walk  1- run, blending in between
			float blendFactor = 0f;
			float animSpeedFactor = 1f;

			if (_speed <= m_walkSpeed) {
				blendFactor = 0f;
				animSpeedFactor = Mathf.Max(0.5f, _speed / m_walkSpeed);
			} else if (_speed >= m_runSpeed) {
				blendFactor = 1f;
				animSpeedFactor = Mathf.Min(1.5f, _speed / m_runSpeed);
			} else {
				blendFactor = 0f + (_speed - m_walkSpeed) * ((1f - 0f) / (m_runSpeed - m_walkSpeed));
			}

			m_animator.SetFloat("speed", blendFactor);
			m_animator.SetBool("move", true);
			m_animator.speed = animSpeedFactor;
		} else {
			m_animator.SetBool("move", false);
			m_animator.speed = 1f;
		}
	}

	public void Scared(bool _scared) {
		if (m_panic)
			return;
		
		if (m_scared != _scared) {
			m_scared = _scared;
			m_animator.SetBool("scared", _scared);
		}
	}

	public void Panic(bool _panic) {
		if (m_panic != _panic) {
			m_panic = _panic;
			m_animator.SetBool("hold", _panic);
		}
	}

	public void Attack() {
		if (m_panic)
			return;
		
		if (!m_attack) {
			m_attack = true;
			m_animator.SetBool("attack", true);
		}
	}

	public void StopAttack() {
		if (m_panic)
			return;
		
		if (m_attack) {
			m_attack = false;
			m_animator.SetBool("attack", false);
		}
	}
}
