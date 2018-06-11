﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IProjectile {

	private enum MotionType {
		Linear = 0,
		Homing,
		Parabolic,
		FreeFall
	}

	private enum State {
		Idle = 0,
		Charging,
		Shot,
		Stuck_On_Player,
		Die
	}

	private enum RotationAxis {
		Up = 0,
		Right,
		Forward
	}

	//---------------------------------------------------------------------------------------

	[SeparatorAttribute("Motion")]
	[SerializeField] private MotionType m_motionType = MotionType.Linear;
	[SerializeField] private float m_chargeTime = 0f;
	[SerializeField] private float m_speed = 0f;
	[SerializeField] private float m_rotationSpeed = 0f;
	[SerializeField] private RotationAxis m_rotationAxis = RotationAxis.Up;
	[SerializeField] private float m_maxTime = 0f; // 0 infinite
	[SerializeField] private float m_scaleTime = 1f;
	[SerializeField] private bool m_stopAtTarget = false;
	[SerializeField] private bool m_dieOutsideFrustum = true;

	[SeparatorAttribute("Weapon")]
	[SerializeField] private float m_defaultDamage = 0f;
	[SerializeField] private DamageType m_damageType = DamageType.NORMAL;
	[SerializeField] private float m_radius = 0f;
	[SerializeField] private float m_knockback = 0f;
	[SerializeField] private DragonTier m_knockbackTier = DragonTier.TIER_4;

	[SeparatorAttribute("Visual")]
	[SerializeField] private List<GameObject> m_activateOnShoot = new List<GameObject>();
	[SerializeField] private ParticleData m_onAttachParticle;
	[SerializeField] private ParticleData m_onChargeParticle;
	[SerializeField] private ParticleData m_onHitParticle;
	[SerializeField] private ParticleData m_onEatParticle;
	[SerializeField] private ParticleData m_onBurnParticle;
	[SerializeField] private bool m_missHitSpawnsParticle = true;
	[SerializeField] private float m_stickOnDragonTime = 0f;
	[SerializeField] private float m_dieTime = 0f;

	[SeparatorAttribute("Audio")]
	[SerializeField] private string m_onHitAudio = "";

	//---------------------------------------------------------------------------------------

	private PoolHandler m_poolHandler;

	private Transform m_source;

	private Vector3 m_startPosition;
	private Vector3 m_lastPosition;
	private Vector3 m_position;
	public Vector3 position { get { return m_position; } }

	private Transform m_target;
	private Vector3 m_targetPosition;
	public Vector3 target { get { return m_targetPosition; } }

	private Vector3 m_direction;
	public Vector3 direction { get { return m_direction; } }

	private Vector3 m_velocity;
	public Vector3 velocity { get { return m_velocity; } }

	private Transform m_trasnform;
	public Vector3 upVector { get { return m_trasnform.up; } }


	private float m_distanceToTarget;
	private float m_elapsedTime;
	private float m_damage;

	private Explosive m_explosive;

	private State m_state;
	public bool hasBeenShot { get { return m_state == State.Shot; } }

	private float m_timer;
	private float m_homingTimer;

	private Transform m_oldParent;

	//This may be a machine
	private Entity m_entity;
	private AI.MachineProjectile m_machine;
	private Collider m_hitCollider;

	//-------------------------------------------------------------------------------------

	// Use this for initialization
	void Awake() {
		m_trasnform = transform;

		m_entity = GetComponent<Entity>();
		m_machine = GetComponent<AI.MachineProjectile>();

		m_state = State.Idle;
	}

	void Start() {
		if (m_damageType == DamageType.EXPLOSION || m_damageType == DamageType.MINE) {
			m_explosive = new Explosive(false, m_defaultDamage, m_radius, 0f, m_onHitParticle);
		} else {
			m_onHitParticle.CreatePool();
		}

		m_onChargeParticle.CreatePool();
		m_onAttachParticle.CreatePool();
		m_onEatParticle.CreatePool();
		m_onBurnParticle.CreatePool();

		m_poolHandler = PoolManager.GetHandler(gameObject.name);
	}

	void OnDisable() {
		for (int i = 0; i<m_activateOnShoot.Count; i++) {
			m_activateOnShoot[i].SetActive(false);
		}
	}

	public void AttachTo(Transform _parent) {		
		AttachTo(_parent, Vector3.zero);
	}

	public void AttachTo(Transform _parent, Vector3 _offset) {
		enabled = true;

		if (m_entity != null) {
			m_entity.Spawn(null);
			if (EntityManager.instance != null)	{
				EntityManager.instance.RegisterEntity(m_entity);
			}
		}

		//save real parent to restore this when the arrow is shot
		m_oldParent = m_trasnform.parent;

		if (m_machine != null) {
			m_machine.Spawn(null);
		}

		//reset transforms, so we don't have any displacement
		m_trasnform.parent = _parent;
		m_trasnform.localPosition = _offset;
		m_trasnform.localRotation = Quaternion.identity;
		m_trasnform.localScale = Vector3.one;

		m_damage = m_defaultDamage;

		if (m_explosive != null) {
			m_explosive.damage = m_defaultDamage;
		}

		m_onAttachParticle.Spawn(_parent, m_onAttachParticle.offset);

		//wait until the projectil is shot
		m_state = State.Idle;
	}

	public void Shoot(Transform _target, Vector3 _direction, float _damage, Transform _source) {
		m_source = _source;

		m_target = _target;
		m_targetPosition = m_target.position;

		if (m_motionType == MotionType.Homing) 	m_direction = _direction;
		else 									m_direction = _target.position - m_trasnform.position;

		m_distanceToTarget = m_direction.sqrMagnitude;
		m_direction.Normalize();

		DoShoot(m_speed, _damage);
	}

	public void ShootTowards(Vector3 _direction, float _speed, float _damage, Transform _source) {
		m_source = _source;

		m_direction = _direction;

		m_distanceToTarget = float.MaxValue;
		m_targetPosition = m_trasnform.position + m_direction * m_distanceToTarget;
		m_target = null;

		DoShoot(_speed, _damage);
	}

	// Shoots At world position _pos
	public void ShootAtPosition(Vector3 _target, Vector3 _direction, float _damage, Transform _source) {
		m_source = _source;

		m_target = null;
		m_targetPosition = _target;

		if (m_motionType == MotionType.Homing) 	m_direction = _direction;
		else 									m_direction = _target - m_trasnform.position;

		m_distanceToTarget = m_direction.sqrMagnitude;
		m_direction.Normalize();

		DoShoot(m_speed, _damage);
	}

	private void DoShoot(float _speed, float _damage) {
		if (m_oldParent) {
			m_trasnform.parent = m_oldParent;
			m_oldParent = null;
		}

		m_damage = _damage;
		if (m_explosive != null) {
			m_explosive.damage = m_damage;
		}

		m_velocity = m_direction * _speed;

		m_position = m_trasnform.position;
		m_lastPosition = m_position;
		m_startPosition = m_position;

		Vector3 newDir = Vector3.RotateTowards(Vector3.forward, -m_direction, 2f * Mathf.PI, 0.0f);
		m_trasnform.rotation = Quaternion.AngleAxis(90f, newDir) * Quaternion.LookRotation(newDir);
		m_trasnform.localScale = Vector3.one;

		//
		for (int i = 0; i < m_activateOnShoot.Count; i++) {
			m_activateOnShoot[i].SetActive(true);
		}

		m_homingTimer = m_maxTime / 20f; //0.25f;

		m_elapsedTime = 0f;
		if (m_chargeTime > 0f) {			
			m_onChargeParticle.Spawn(m_position);

			m_timer = m_chargeTime;
			m_state = State.Charging;
		} else {
			m_timer = m_maxTime;
			m_state = State.Shot;
		}

		if (m_motionType == MotionType.FreeFall) {
			m_velocity = GameConstants.Vector3.zero;
		}
	}

	// Update is called once per frame
	private void Update () {
		if (m_state == State.Charging) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_timer = 0f;
				m_state = State.Shot;
			}
		} else if (m_state == State.Shot) {
			if (m_machine == null || !m_machine.IsDying()) {
				// motion
				float dt = Time.deltaTime;
				m_elapsedTime += dt;

				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						Explode(false);
						return;
					}
				}

				if (m_dieOutsideFrustum) {
					if (InstanceManager.gameCamera != null) {
						bool rem = InstanceManager.gameCamera.IsInsideDeactivationArea(m_position);
						if (rem) {
							Explode(false);
							return;
						}
					}
				}

				if (m_motionType != MotionType.Linear) {
					if (m_position != m_lastPosition) {
						Vector3 dir = m_position - m_lastPosition;
						dir.Normalize();
						dir = Vector3.RotateTowards(Vector3.forward, -dir, 2f * Mathf.PI, 0.0f);
						m_trasnform.rotation = Quaternion.AngleAxis(90f, dir) * Quaternion.LookRotation(dir);
					}
				}

				if (m_rotationSpeed > 0f) {
					Vector3 axis = m_trasnform.up;

					if (m_rotationAxis == RotationAxis.Right) 			axis = m_trasnform.right;
					else if (m_rotationAxis == RotationAxis.Forward)	axis = m_trasnform.forward;

					m_trasnform.rotation = Quaternion.AngleAxis(m_elapsedTime * 240f * m_rotationSpeed, axis) * m_trasnform.rotation;
				}
			} else {
				Die();
			}
		} else if (m_state == State.Stuck_On_Player) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				Die();
			}
		} else if (m_state == State.Die) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_state = State.Idle;
				gameObject.SetActive(false);
				m_poolHandler.ReturnInstance(gameObject);
			}
		}
	}

	private void FixedUpdate () {
		if (m_state == State.Shot) {
			if (m_machine == null || !m_machine.IsDying()) {
				// motion
				float dt = Time.fixedDeltaTime * m_scaleTime;
				m_elapsedTime += dt;

				m_lastPosition = m_position;

				switch (m_motionType) {
					case MotionType.Linear:
						m_position += m_velocity * dt;
						break;

					case MotionType.Homing: {
							m_position += m_velocity * dt;

							m_homingTimer -= Time.deltaTime;
							if (m_homingTimer <= 0f) {
								m_homingTimer = 0f;

								Vector3 impulse = (m_target.position - m_position).normalized * m_speed;
								impulse = (impulse - m_velocity) / 15f; //mass
								m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_speed);
							}
						} break;

					case MotionType.Parabolic: 
						m_position = m_startPosition + (m_velocity + Vector3.down * 0.5f * 9.8f * m_elapsedTime) * m_elapsedTime;
						break;

					case MotionType.FreeFall: 
						m_position = m_startPosition + (m_velocity + Vector3.down * 0.5f * 0.98f * m_elapsedTime) * m_elapsedTime;
						break;
				}
				m_trasnform.position = m_position;

				// impact checks
				if (m_stopAtTarget) {
					float distanceToTarget = (m_targetPosition - m_position).sqrMagnitude;
					if (distanceToTarget > m_distanceToTarget) {
						Explode(false);
						return;
					}
					m_distanceToTarget = distanceToTarget;
				}
			}
		}
	}

	private void OnTriggerEnter(Collider _other) {
		if (m_state == State.Shot) {
			if (m_machine == null || !m_machine.IsDying()) {
				m_hitCollider = _other;
				if (_other.CompareTag("Player"))  {
					Explode(true);
				} else if ((((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "GroundVisible")) > 0)) {
					Explode(false);
				}
			}

			Debug.Log(name + " >> " + _other.name);
		}
	}

	public void OnEaten() {		
		m_onEatParticle.Spawn(m_position + m_onEatParticle.offset);

		if (m_entity != null) {
			if (EntityManager.instance != null)	{
				EntityManager.instance.UnregisterEntity(m_entity);
			}
		}

		m_state = State.Idle;
		gameObject.SetActive(false);
		m_poolHandler.ReturnInstance(gameObject);
	}

	public void OnBurned() {
		m_onBurnParticle.Spawn(m_position + m_onBurnParticle.offset);

		if (m_entity != null) {
			if (EntityManager.instance != null)	{
				EntityManager.instance.UnregisterEntity(m_entity);
			}
		}

		m_state = State.Idle;
		gameObject.SetActive(false);
		m_poolHandler.ReturnInstance(gameObject);
	}

	public void Explode(bool _triggeredByPlayer) {
		DragonPlayer player = InstanceManager.player;

		// dealing damage
		float actualKnockback = m_knockback;
		if (player.data.tier > m_knockbackTier) {
			actualKnockback = 0f;
		}

		if (m_damageType == DamageType.EXPLOSION || m_damageType == DamageType.MINE) {
			m_explosive.Explode(transform, actualKnockback, _triggeredByPlayer);
		} else {
			if (_triggeredByPlayer) {
				if (actualKnockback > 0) {
					DragonMotion dragonMotion = player.dragonMotion;

					Vector3 knockBackDirection = dragonMotion.transform.position - m_trasnform.position;
					knockBackDirection.z = 0f;
					knockBackDirection.Normalize();

					dragonMotion.AddForce(knockBackDirection * actualKnockback);
				}

				player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType, m_source);
			}

			if (m_missHitSpawnsParticle || _triggeredByPlayer) {				
				m_onHitParticle.Spawn(m_position + m_onHitParticle.offset, m_trasnform.rotation);
			}
		}

		if (!string.IsNullOrEmpty(m_onHitAudio))
			AudioController.Play(m_onHitAudio, m_trasnform.position);
		
		if (m_entity != null) {
			if (EntityManager.instance != null)	{
				EntityManager.instance.UnregisterEntity(m_entity);
			}
		}

		if (m_stickOnDragonTime > 0f && _triggeredByPlayer) {
			StickOnCollider();
		} else {
			Die();
		}
	}

	private void StickOnCollider() {
		m_state = State.Stuck_On_Player;

		for (int i = 0; i < m_activateOnShoot.Count; i++) {
			m_activateOnShoot[i].SetActive(false);
		}

		m_trasnform.parent = m_hitCollider.transform;
		m_timer = m_stickOnDragonTime;
	}

	private Transform SearchClosestHoldPoint(HoldPreyPoint[] holdPreyPoints) {
		float distance = float.MaxValue;
		Transform holdTransform = InstanceManager.player.transform;

		for (int i = 0; i < holdPreyPoints.Length; i++) {
			HoldPreyPoint point = holdPreyPoints[i];
			if (Vector3.SqrMagnitude(m_position - point.transform.position) < distance) {
				distance = Vector3.SqrMagnitude(m_position - point.transform.position);
				holdTransform = point.transform;
			}
		}

		return holdTransform;
	}

	private void Die() {
		m_timer = m_dieTime;
		m_state = State.Die;
	}
}
