﻿using UnityEngine;
using System.Collections;

public class MagicProjectile : MonoBehaviour, IProjectile {
	
	[SerializeField] private float m_damageDelay = 0f;

	[SerializeField] private EffectSettings m_effect;
	[SerializeField] private GameObject m_effectIdle;
	private Transform m_oldParent;

	private float m_damage;

	private bool m_hasBeenShot;
	private bool m_isDragonHit;
	private float m_timer;


	void OnEnable() {
		m_effect.gameObject.SetActive(false);
		m_effectIdle.SetActive(false);
		m_hasBeenShot = false;
		m_isDragonHit = false;

		m_effect.CollisionEnter += OnCollisionEnterAOC;	// [AOC] Can't be called OnCollisionEnter since there is already a message with this name!
	}

	void OnDisable() {
		
	}

	public void OnCollisionEnterAOC(object _o, CollisionInfo _collision) {
		if (_collision.Hit.rigidbody != null) {
			m_isDragonHit = true;
			m_timer = m_damageDelay;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (m_hasBeenShot) {
			if (m_isDragonHit) {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, DamageType.NORMAL, transform, true);
					m_isDragonHit = false;
				}
			}
			if (!m_effect.gameObject.activeInHierarchy) {
				PoolManager.ReturnInstance(gameObject);
			}
		}
	}

	public void Explode(bool b)
	{

	}

	public void AttachTo(Transform _parent) {		
		AttachTo(_parent, Vector3.zero);
	}

	public void AttachTo(Transform _parent, Vector3 _offset) {
		m_oldParent = transform.parent;
		transform.parent = _parent;
		transform.position = _offset;
		transform.localPosition = Vector3.zero;
		m_effect.transform.position = Vector3.zero;
		m_effect.transform.localPosition = Vector3.zero;

		m_effect.gameObject.SetActive(false);
		m_effectIdle.SetActive(true);
	}

	public void Shoot(Vector3 _target, float _damage) {
		transform.parent = m_oldParent;
		m_effect.Target = InstanceManager.player.gameObject;

		m_damage = _damage;

		m_effectIdle.SetActive(false);
		m_effect.gameObject.SetActive(true);
		m_hasBeenShot = true;
	}

	public void ShootTowards(Vector3 _direction, float _speed, float _damage) {}

	public void ShootAtPosition(Transform _from, float _damage, Vector3 _pos) {		
		transform.parent = m_oldParent;
		// m_effect.Target = InstanceManager.player.gameObject;
		m_effect.MoveVector = _pos - _from.transform.position;

		m_damage = _damage;

		m_effectIdle.SetActive(false);
		m_effect.gameObject.SetActive(true);
		m_hasBeenShot = true;
	}
}
