using System;
using UnityEngine;
using System.Collections.Generic;

public class ViewControl : MonoBehaviour, ISpawnable {

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
	[SeparatorAttribute("Animation playback speed")]
	[SerializeField] private float m_walkSpeed = 1f;
	[SerializeField] private float m_runSpeed = 1f;
	[SerializeField] private float m_minPlaybakSpeed = 1f;
	[SerializeField] private float m_maxPlaybakSpeed = 1.5f;
	[SerializeField] private bool m_onBoostMaxPlaybackSpeed = false;


	[SeparatorAttribute("Animation blending")]
	[SerializeField] private bool m_hasNavigationLayer = false;
	[SerializeField] private bool m_hasRotationLayer = false;

	[SeparatorAttribute("Special Actions Animations")] // map a special action from the pilot to a specific animation.
	[SerializeField] private string m_animA = "";
	[SerializeField] private string m_animB = "";
	[SerializeField] private string m_animC = "";

	[SeparatorAttribute]
	[SerializeField] private List<ParticleData> m_onEatenParticles = new List<ParticleData>();

	[SeparatorAttribute]
	[SerializeField] private ParticleData m_explosionParticles; // this will explode when burning


	//-----------------------------------------------
	private Entity m_entity;
	private Animator m_animator;
	private Material m_materialGold;
	private Dictionary<int, Material[]> m_materials;

	private bool m_boost;
	private bool m_scared;
	private bool m_panic; //bite and hold state
	private bool m_attack;

	private float m_desiredBlendX;
	private float m_desiredBlendY;

	private float m_currentBlendX;
	private float m_currentBlendY;

	private bool[] m_specialAnimations;

	private GameObject m_pcTrail = null;

	//-----------------------------------------------
	// Use this for initialization
	//-----------------------------------------------
	void Awake() {
		m_entity = GetComponent<Entity>();
		m_animator = transform.FindComponentRecursive<Animator>();
		m_animator.logWarnings = false;

		// Load gold material
		m_materialGold = Resources.Load<Material>("Game/Assets/Materials/Gold");

		// keep the original materials, sometimes it will become Gold!
		m_materials = new Dictionary<int, Material[]>(); 
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			m_materials[renderers[i].GetInstanceID()] = renderers[i].materials;
		}

		// particle management
		if (m_onEatenParticles.Count <= 0) {
			// if this entity doesn't have any particles attached, set the standard blood particle
			ParticleData data = new ParticleData();
			data.name = "PS_Blood_Explosion_Small";
			data.path = "Blood/";
			data.offset = Vector3.back * 10f;
		}

		m_specialAnimations = new bool[(int)SpecialAnims.Count];
	}
	//

	public virtual void Spawn(ISpawner _spawner) {
		m_boost = false;
		m_scared = false;
		m_panic = false;
		m_attack = false;

		m_animator.speed = 1f;

		// Restore materials
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			if (m_entity.isGolden) {				
				Material[] materials = renderers[i].materials;
				for (int m = 0; m < materials.Length; m++) {
					if (!materials[m].shader.name.EndsWith("Additive"))
						materials[m] = m_materialGold;
				}
				renderers[i].materials = materials;
			} else {
				if (m_materials.ContainsKey(renderers[i].GetInstanceID()))
					renderers[i].materials = m_materials[renderers[i].GetInstanceID()];
			}
		}

		// Show PC Trail?
		if(m_entity.isPC) {
			// Get an effect instance from the pool
			m_pcTrail = ParticleManager.Spawn("PS_EntityPCTrail", Vector3.zero, "Rewards/");

			// Put it in the view's hierarchy so it follows the entity
			if(m_pcTrail != null) {
				m_pcTrail.transform.SetParent(transform);
				m_pcTrail.transform.localPosition = Vector3.zero;
			}
		}
	}

	protected virtual void Update() {
		if (m_hasNavigationLayer) {
			m_currentBlendX = Util.MoveTowardsWithDamping(m_currentBlendX, m_desiredBlendX, 3f * Time.deltaTime, 0.2f);
			m_animator.SetFloat("direction X", m_currentBlendX);

			m_currentBlendY = Util.MoveTowardsWithDamping(m_currentBlendY, m_desiredBlendY, 3f * Time.deltaTime, 0.2f);
			m_animator.SetFloat("direction Y", m_currentBlendY);
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


	//Particles
	public void SpawnEatenParticlesAt(Transform _transform) {
		for( int i = 0; i < m_onEatenParticles.Count; i++) {
			ParticleData data = m_onEatenParticles[i];
			if (!string.IsNullOrEmpty(data.name)) {
				GameObject go = ParticleManager.Spawn(data.name, transform.position + data.offset, data.path);
				if (go != null)	{
					FollowTransform ft = go.GetComponent<FollowTransform>();
					if (ft != null) {
						ft.m_follow = _transform;
						ft.m_offset = data.offset;
					}
				}
			}
		}
	}


	// Animations
	public void NavigationLayer(Vector3 _dir) {
		if (m_hasNavigationLayer) {
			Vector3 localDir = transform.InverseTransformDirection(_dir);	// todo: replace with direction to target if trying to bite, or during bite?
			m_desiredBlendX = Mathf.Clamp(localDir.x * 3f, -1f, 1f);	// max X bend is about 30 degrees, so *3
			m_desiredBlendY = Mathf.Clamp(localDir.y * 2f, -1f, 1f);	// max Y bend is about 45 degrees, so *2.
		}
	}

	public void RotationLayer(ref Quaternion _from, ref Quaternion _to) {
		if (m_hasRotationLayer) {
			float angle = Quaternion.Angle(_from, _to);
			m_animator.SetBool("rotate left", angle < 0);
			m_animator.SetBool("rotate right", angle > 0);
		}
	}

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
				animSpeedFactor = Mathf.Max(m_minPlaybakSpeed, _speed / m_walkSpeed);
			} else if (_speed >= m_runSpeed) {
				blendFactor = 1f;
				animSpeedFactor = Mathf.Min(m_maxPlaybakSpeed, _speed / m_runSpeed);
			} else {
				blendFactor = 0f + (_speed - m_walkSpeed) * ((1f - 0f) / (m_runSpeed - m_walkSpeed));
			}

			if (m_boost && m_onBoostMaxPlaybackSpeed) {
				animSpeedFactor = m_maxPlaybakSpeed;
			}

			m_animator.SetFloat("speed", blendFactor);
			m_animator.SetBool("move", true);
			m_animator.speed = animSpeedFactor;
		} else {
			m_animator.SetBool("move", false);
			m_animator.speed = 1f;
		}
	}

	public void Boost(bool _boost) {
		if (m_panic)
			return;

		if (m_boost != _boost) {
			m_boost = _boost;
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

	public void Panic(bool _panic, bool _burning) {
		if (m_panic != _panic) {
			m_panic = _panic;

			if (_burning) {
				// lets buuurn!!!
				// will we have a special animation when burning?
				m_animator.speed = 0f;
			} else {
				m_animator.SetBool("hold", _panic);
			}
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
		if (m_explosionParticles.name != "") {
			ParticleManager.Spawn(m_explosionParticles.name, transform.position + m_explosionParticles.offset, m_explosionParticles.path);
		}

		// Stop pc trail effect (if any)
		if(m_pcTrail != null) {
			ParticleManager.ReturnInstance(m_pcTrail);
			m_pcTrail = null;
		}
	}

	public void Burn()
	{
		m_animator.enabled = false;
	}
}
