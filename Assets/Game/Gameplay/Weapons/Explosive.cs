﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive {
	
	private float m_damage;
	public float damage { set { m_damage = value; } }

	private DamageType m_damageType;
	private float m_radiusSqr;

	private float m_playerRadiusSqr;

	private float m_cameraShakeTime;

	private ParticleData m_particleData;


	public Explosive(bool _isMine, float _damage, float _radius, float _cameraShakeTime, ParticleData _particleData = null) {
		if (_isMine) m_damageType = DamageType.MINE;
		else 		 m_damageType = DamageType.EXPLOSION;

		m_damage = _damage;
		m_radiusSqr = _radius * _radius;

		m_cameraShakeTime = _cameraShakeTime;

		m_particleData = _particleData;

		if (m_particleData != null) {
			if (m_particleData.IsValid()) {
				ParticleManager.CreatePool(m_particleData);
			}
		}

		SphereCollider sc = InstanceManager.player.GetComponentInChildren<SphereCollider>();
		m_playerRadiusSqr = sc.radius * sc.radius;
	}

	public void Explode(Vector3 _at, float _knockback, bool _triggeredByPlayer) {
		DragonPlayer dragon = InstanceManager.player;
		bool hasPlayerReceivedDamage = _triggeredByPlayer;

		if (!hasPlayerReceivedDamage && m_radiusSqr > 0f) {
			float dSqr = (dragon.transform.position - _at).sqrMagnitude;
			float rSqr = m_radiusSqr + m_playerRadiusSqr;

			hasPlayerReceivedDamage = (dSqr <= rSqr);
		}

		if (hasPlayerReceivedDamage) {			
			if (dragon.HasShield(m_damageType)) {
				dragon.LoseShield(m_damageType);
			} else {
				DragonHealthBehaviour health = dragon.dragonHealthBehaviour;
				if (health != null) {
					health.ReceiveDamage(m_damage, m_damageType);
					if (health.IsAlive()) {
						Messenger.Broadcast<float, float>(GameEvents.CAMERA_SHAKE, m_cameraShakeTime, 0f);
					}

					if (_knockback > 0) {
						DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

						Vector3 knockBackDirection = dragonMotion.transform.position - _at;
						knockBackDirection.z = 0f;
						knockBackDirection.Normalize();

						dragonMotion.AddForce(knockBackDirection * _knockback);
					}
				}
			}
		}

		if (m_particleData != null) {
			if (m_particleData.IsValid()) {
				ParticleManager.Spawn(m_particleData, _at + m_particleData.offset);
			}
		}
	}
}
