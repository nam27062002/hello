using System;
using UnityEngine;
using System.Collections.Generic;

public class ViewControlBg : ISpawnable {

	[Serializable]
	public class ParticleData {
		public string name = "";
		public string path = "";
		public Vector3 offset = Vector3.zero;
	}

	public enum SpecialAnims {
		A = 0,
		B,
		C,

		Count
	}

	//-----------------------------------------------
	[SerializeField] private float m_walkSpeed = 1f;
	[SerializeField] private float m_runSpeed = 1f;

	[SerializeField] private bool m_hasNavigationLayer = false;
	[SerializeField] private bool m_hasRotationLayer = false;

	[SeparatorAttribute("Special Actions Animations")] // map a special action from the pilot to a specific animation.
	[SerializeField] private string m_animA = "";
	[SerializeField] private string m_animB = "";
	[SerializeField] private string m_animC = "";



	//-----------------------------------------------
	private Animator m_animator;

	private bool m_scared;
	private bool m_panic; //bite and hold state
	private bool m_attack;

	private float m_desiredBlendX;
	private float m_desiredBlendY;

	private float m_currentBlendX;
	private float m_currentBlendY;

	private bool[] m_specialAnimations;

	//-----------------------------------------------
	// Use this for initialization
	//-----------------------------------------------
	void Awake() {
		m_animator = transform.FindComponentRecursive<Animator>();
		m_animator.logWarnings = false;

		m_specialAnimations = new bool[(int)SpecialAnims.Count];
	}
	//

	override public void Spawn(ISpawner _spawner) {
		m_scared = false;
		m_panic = false;
		m_attack = false;

		m_animator.speed = 1f;
	}

	override public void CustomUpdate() {
		if (m_hasNavigationLayer) {
			m_currentBlendX = Util.MoveTowardsWithDamping(m_currentBlendX, m_desiredBlendX, 3f * Time.deltaTime, 0.2f);
			m_animator.SetFloat( GameConstants.Animator.DIR_X , m_currentBlendX);

			m_currentBlendY = Util.MoveTowardsWithDamping(m_currentBlendY, m_desiredBlendY, 3f * Time.deltaTime, 0.2f);
			m_animator.SetFloat( GameConstants.Animator.DIR_Y, m_currentBlendY);
		}
	}


	// Queries
	public bool canAttack() {
		return !m_attack;
	}

	public bool hasAttackEnded() {
		return false;
	}
	//




	// Animations
	public void NavigationLayer(Vector3 _dir) {
		if (m_hasNavigationLayer) {
			Vector3 localDir = transform.InverseTransformDirection(_dir);	// todo: replace with direction to target if trying to bite, or during bite?
			m_desiredBlendX = Mathf.Clamp(-localDir.z * 3f, -1f, 1f);	// max X bend is about 30 degrees, so *3
			m_desiredBlendY = Mathf.Clamp(localDir.y * 2f, -1f, 1f);	// max Y bend is about 45 degrees, so *2.
		}
	}

	public void RotationLayer(ref Quaternion _from, ref Quaternion _to) {
		if (m_hasRotationLayer) {
			float angle = Quaternion.Angle(_from, _to);
			m_animator.SetBool( GameConstants.Animator.ROTATE_LEFT , angle < 0);
			m_animator.SetBool( GameConstants.Animator.ROTATE_RIGHT, angle > 0);
		}
	}

	public void Aim(float _blendFactor) {
		m_animator.SetFloat( GameConstants.Animator.AIM, _blendFactor);
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

			m_animator.SetFloat( GameConstants.Animator.SPEED , blendFactor);
			m_animator.SetBool( GameConstants.Animator.MOVE, true);
			m_animator.speed = animSpeedFactor;
		} else {
			m_animator.SetBool( GameConstants.Animator.MOVE, false);
			m_animator.speed = 1f;
		}
	}

	public void Scared(bool _scared) {
		if (m_panic)
			return;
		
		if (m_scared != _scared) {
			m_scared = _scared;
			m_animator.SetBool( GameConstants.Animator.SCARED , _scared);
		}
	}

	public void Panic(bool _panic, bool _burning) {
		if (m_panic != _panic) {
			m_panic = _panic;

			if (_burning) {
				// lets buuurn!!!
				// will we have a special animation when burning?
				m_animator.speed = 0f;
			} else {
				m_animator.SetBool( GameConstants.Animator.HOLDED , _panic);
			}
		}
	}

	public void Attack() {
		if (m_panic)
			return;
		
		if (!m_attack) {
			m_attack = true;
			m_animator.SetBool( GameConstants.Animator.ATTACK , true);
		}
	}

	public void StopAttack() {
		if (m_panic)
			return;
		
		if (m_attack) {
			m_attack = false;
			m_animator.SetBool( GameConstants.Animator.ATTACK, false);
		}
	}

	public void SpecialAnimation(SpecialAnims _anim, bool _value) {
		if (m_specialAnimations[(int)_anim] != _value) {
			switch(_anim) {
				case SpecialAnims.A: m_animator.SetBool(m_animA, _value); break;
				case SpecialAnims.B: m_animator.SetBool(m_animB, _value); break;
				case SpecialAnims.C: m_animator.SetBool(m_animC, _value); break;
			}

			if (_value) OnSpecialAnimationEnter(_anim);
			else 		OnSpecialAnimationExit(_anim);
		}
		m_specialAnimations[(int)_anim] = _value;
	}

	protected virtual void OnSpecialAnimationEnter(SpecialAnims _anim) {}
	protected virtual void OnSpecialAnimationExit(SpecialAnims _anim) {}

	public void Die(bool _eaten = false) 
	{
	}

	public void Burn()
	{
		m_animator.enabled = false;
	}
}
