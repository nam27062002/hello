using UnityEngine;
using System.Collections;

public class ViewControl : MonoBehaviour {

	[SerializeField] private float m_walkSpeed = 1f;
	[SerializeField] private float m_runSpeed = 1f;

	private Animator m_animator;

	private bool m_scared;
	private bool m_attack;


	// Use this for initialization
	void Start () {
		m_animator = transform.FindComponentRecursive<Animator>();

		m_scared = false;
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

	public void Aim(float _blendFactor) {
		m_animator.SetFloat("aim", _blendFactor);
	}

	public void Move(float _speed) {
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
		if (m_scared != _scared) {
			m_scared = _scared;
			m_animator.SetBool("scared", _scared);
		}
	}

	public void Attack() {
		if (!m_attack) {
			m_attack = true;
			m_animator.SetBool("attack", true);
		}
	}

	public void StopAttack() {
		if (m_attack) {
			m_attack = false;
			m_animator.SetBool("attack", false);
		}
	}
}
