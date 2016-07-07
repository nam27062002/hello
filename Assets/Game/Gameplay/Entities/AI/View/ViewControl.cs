using System;
using UnityEngine;
using System.Collections.Generic;

public class ViewControl : MonoBehaviour, Spawnable {

	[Serializable]
	public class ParticleData {
		public string name = "";
		public string path = "";
		public Vector3 offset = Vector3.zero;
	}

	//-----------------------------------------------
	[SerializeField] private float m_walkSpeed = 1f;
	[SerializeField] private float m_runSpeed = 1f;

	[SeparatorAttribute]
	[SerializeField] private List<ParticleData> m_onEatenParticles = new List<ParticleData>();

	[SeparatorAttribute]
	[SerializeField] private ParticleData m_explosionParticles; // this will explode when burning


	//-----------------------------------------------
	private Entity m_entity;
	private Animator m_animator;
	private Material m_materialGold;
	private Dictionary<int, Material[]> m_materials;

	private bool m_scared;
	private bool m_panic; //bite and hold state
	private bool m_attack;


	//-----------------------------------------------
	// Use this for initialization
	void Awake() {
		m_entity = GetComponent<Entity>();
		m_animator = transform.FindComponentRecursive<Animator>();

		m_materialGold = Resources.Load("Game/Assets/Materials/Gold") as Material;

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
	}
	//

	public void Spawn(Spawner _spawner) {
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

	public void Die(bool _eaten = false) {
		if (m_explosionParticles.name != "") {
			ParticleManager.Spawn(m_explosionParticles.name, transform.position + m_explosionParticles.offset, m_explosionParticles.path);
		}
	}
}
