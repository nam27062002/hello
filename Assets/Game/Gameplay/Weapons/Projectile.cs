using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IProjectile {

	private enum MotionType {
		Linear = 0,
		Projectile
	}

	//---------------------------------------------------------------------------------------

	[SeparatorAttribute("Motion")]
	[SerializeField] private MotionType m_motionType = MotionType.Linear;
	[SerializeField] private float m_speed = 0f;
	[SerializeField] private float m_maxTime = 0f; // 0 infinite
	[SerializeField] private bool m_stopAtTarget = false;

	[SeparatorAttribute("Weapon")]
	[SerializeField] private float m_defaultDamage = 0f;
	[SerializeField] private DamageType m_damageType = DamageType.NORMAL;
	[SerializeField] private float m_radius = 0f;
	[SerializeField] private float m_knockback = 0f;

	[SeparatorAttribute("Visual")]
	[SerializeField] private List<GameObject> m_activateOnShoot = new List<GameObject>();
	[SerializeField] private ParticleData m_onHitParticle;

	//---------------------------------------------------------------------------------------

	private Vector3 m_position;
	private Vector3 m_target;
	private Vector3 m_direction;
	private float m_distanceToTarget;

	private float m_damage;

	private Explosive m_explosive;

	private bool m_hasBeenShot;
	private float m_timer;

	private Transform m_oldParent;


	//-------------------------------------------------------------------------------------

	// Use this for initialization
	void Start() {
		if (m_damageType == DamageType.EXPLOSION || m_damageType == DamageType.MINE) {
			m_explosive = new Explosive(false, m_defaultDamage, m_radius, 0f, m_onHitParticle);
		} else {
			if (m_onHitParticle.IsValid()) {
				ParticleManager.CreatePool(m_onHitParticle, 5);
			}
		}

		m_hasBeenShot = false;
	}

	void OnDisable() {
		for (int i = 0; i<m_activateOnShoot.Count; i++) {
			m_activateOnShoot[i].SetActive(false);
		}
	}

	public void AttachTo(Transform _parent) {		
		//save real parent to restore this when the arrow is shot
		m_oldParent = transform.parent;

		//reset transforms, so we don't have any displacement
		transform.parent = _parent;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		m_damage = m_defaultDamage;

		//wait until the projectil is shot
		m_hasBeenShot = false;
	}

	public void Shoot(Vector3 _target, float _damage = 0f) {
		if (_damage > 0f) {
			m_damage = _damage;

			if (m_explosive != null) {
				m_explosive.damage = m_damage;
			}
		}

		m_target = _target;

		if (m_oldParent) {
			transform.parent = m_oldParent;
			m_oldParent = null;
		}

		m_position = transform.position;

		m_direction = m_target - m_position;
		m_distanceToTarget = m_direction.sqrMagnitude;
		m_direction.Normalize();

		Vector3 newDir = Vector3.RotateTowards(Vector3.forward, -m_direction, 2f * Mathf.PI, 0.0f);
		transform.rotation = Quaternion.AngleAxis(90f, m_direction) * Quaternion.LookRotation(newDir);

		for (int i = 0; i < m_activateOnShoot.Count; i++) {
			m_activateOnShoot[i].SetActive(true);
		}

		m_timer = m_maxTime;

		m_hasBeenShot = true;
	}

	// change this
	public void ShootAtPosition( Transform _from, float _damage, Vector3 _pos) {
		Shoot(_pos, _damage);
	}

	// Update is called once per frame
	private void Update () {
		if (m_hasBeenShot) {
			// motion
			switch (m_motionType) {
				case MotionType.Linear:
					m_position += m_direction * m_speed * Time.deltaTime;
					break;

				case MotionType.Projectile:

					break;
			}
			transform.position = m_position;

			// impact checks
			if (m_stopAtTarget) {
				float distanceToTarget = (m_target - m_position).sqrMagnitude;
				if (distanceToTarget > m_distanceToTarget) {
					Impact(false);
					return;
				}
				m_distanceToTarget = distanceToTarget;
			}

			if (m_timer > 0f) {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					Impact(false);
					return;
				}
			}

			if (InstanceManager.gameCamera != null) {
				bool rem = InstanceManager.gameCamera.IsInsideDeactivationArea(m_position);
				if (rem) {
					Impact(false);
					return;
				}
			}
		}
	}

	private void OnTriggerEnter(Collider _other) {
		if (m_hasBeenShot) {
			if (_other.CompareTag("Player"))  {
				Impact(true);
			} else if ((((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "GroundVisible")) > 0)) {
				Impact(false);
			}
		}
	}

	private void Impact(bool _triggeredByPlayer) {
		// dealing damage
		if (m_damageType == DamageType.EXPLOSION || m_damageType == DamageType.MINE) {
			m_explosive.Explode(m_position, m_knockback, _triggeredByPlayer);
		} else {
			if (_triggeredByPlayer) {
				if (m_knockback > 0) {
					DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

					Vector3 knockBackDirection = dragonMotion.transform.position - transform.position;
					knockBackDirection.z = 0f;
					knockBackDirection.Normalize();

					dragonMotion.AddForce(knockBackDirection * m_knockback);
				}

				InstanceManager.player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType);
			}

			if (m_onHitParticle.IsValid()) {
				ParticleManager.Spawn(m_onHitParticle, m_position + m_onHitParticle.offset);
			}
		}

		m_hasBeenShot = false;

		gameObject.SetActive(false);
		PoolManager.ReturnInstance(gameObject);
	}
}
