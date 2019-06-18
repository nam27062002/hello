using System.Collections.Generic;
using UnityEngine;

public class Projectile : TriggerCallbackReceiver, IProjectile {

	public enum MotionType {
		Linear = 0,
		Homing,
		Parabolic,
		FreeFall
	}

	private enum RotationAxis {
		Up = 0,
		Right,
		Forward
	}

    protected enum State
    {
        Idle = 0,
        Charging,
        Shot,
        Stuck_On_Player,
        Die
    }

	//---------------------------------------------------------------------------------------

	[SeparatorAttribute("Motion")]
	[SerializeField] protected MotionType m_motionType = MotionType.Linear;
    public MotionType motionType 
    { 
        get { return m_motionType; }
        set { m_motionType = value; } 
    }
    [SerializeField] private float m_mass = 15f;
    [SerializeField] private float m_chargeTime = 0f;
	[SerializeField] private float m_speed = 0f;
    public float speed 
    { 
        get { return m_speed; }
        set { m_speed = value; } 
    }
	[SerializeField] private float m_rotationSpeed = 0f;
	[SerializeField] private RotationAxis m_rotationAxis = RotationAxis.Up;
	[SerializeField] private float m_maxTime = 0f; // 0 infinite
	[SerializeField] private float m_scaleTime = 1f;
    [Comment("Stop at targetPosition if target is null")]
	[SerializeField] private bool m_stopAtTarget = false;
	[SerializeField] private bool m_dieOutsideFrustum = true;
    [SerializeField] private bool m_dieOnHit = true;

	[SeparatorAttribute("Weapon")]
	[SerializeField] private float m_defaultDamage = 0f;
	[SerializeField] private DamageType m_damageType = DamageType.NORMAL;
	[SerializeField] protected float m_radius = 0f;
	[SerializeField] private float m_knockback = 0f;
	[SerializeField] private DragonTier m_knockbackTier = DragonTier.TIER_4;

	[SeparatorAttribute("Visual")]
	[SerializeField] private List<GameObject> m_activateOnShoot = new List<GameObject>();
	[SerializeField] private ParticleData m_onAttachParticle = null;
    [SerializeField] private ParticleData m_onChargeParticle = null;
    [SerializeField] protected ParticleData m_onHitParticle = null;
    [SerializeField] private ParticleData m_onEatParticle = null;
    [SerializeField] private ParticleData m_onBurnParticle = null;
	[SerializeField] protected bool m_missHitSpawnsParticle = true;
	[SerializeField] private float m_stickOnDragonTime = 0f;
	[SerializeField] private float m_dieTime = 0f;

	[SeparatorAttribute("Audio")]
	[SerializeField] private string m_onHitAudio = "";

	//---------------------------------------------------------------------------------------

	private PoolHandler m_poolHandler;

	private Transform m_source;

	private Vector3 m_startPosition;
	private Vector3 m_lastPosition;
	protected Vector3 m_position;
	public Vector3 position { get { return m_position; } }

	protected Transform m_target;
	private Vector3 m_targetPosition;
	public Vector3 target { get { return m_targetPosition; } }

	private Vector3 m_direction;
	public Vector3 direction { get { return m_direction; } }

	protected Vector3 m_velocity;
	public Vector3 velocity { get { return m_velocity; } set { m_velocity = value; } }

	protected Transform m_transform;
	public Vector3 upVector { get { return m_transform.up; } }


	private float m_distanceToTarget;
	private float m_elapsedTime;
	private float m_damage;

	private Explosive m_explosive;

    protected State m_state;
	public bool hasBeenShot { get { return m_state == State.Shot; } }

	private float m_timer;
	private float m_homingTimer;

	private Transform m_oldParent;

	//This may be a machine
    protected Entity m_entity;
	protected AI.MachineProjectile m_machine;
	protected Collider m_hitCollider;

	//-------------------------------------------------------------------------------------

	// Use this for initialization
	void Awake() {
		m_transform = transform;

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

        m_onEatParticle.CreatePool();
        m_onBurnParticle.CreatePool();
		m_onChargeParticle.CreatePool();
		m_onAttachParticle.CreatePool();
		
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
		m_oldParent = m_transform.parent;

		if (m_machine != null) {
			m_machine.Spawn(null);
		}

		//reset transforms, so we don't have any displacement
		m_transform.parent = _parent;
		m_transform.localPosition = _offset;
		m_transform.localRotation = Quaternion.identity;
		m_transform.localScale = Vector3.one;

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
		else 									m_direction = _target.position - m_transform.position;

		m_distanceToTarget = (_target.position - m_transform.position).sqrMagnitude;
		m_direction.Normalize();

		DoShoot(m_speed, _damage);
	}

	public void ShootTowards(Vector3 _direction, float _speed, float _damage, Transform _source) {
		m_source = _source;

		m_direction = _direction;

		m_distanceToTarget = float.MaxValue;
		m_targetPosition = m_transform.position + m_direction * m_distanceToTarget;
		m_target = null;

		DoShoot(_speed, _damage);
	}

	// Shoots At world position _pos
	public void ShootAtPosition(Vector3 _target, Vector3 _direction, float _damage, Transform _source) {
		m_source = _source;

		m_target = null;
		m_targetPosition = _target;

		if (m_motionType == MotionType.Homing) 	m_direction = _direction;
		else 									m_direction = _target - m_transform.position;

		m_distanceToTarget = (_target - m_transform.position).sqrMagnitude;
		m_direction.Normalize();

		DoShoot(m_speed, _damage);
	}

	private void DoShoot(float _speed, float _damage) {
		if (m_oldParent) {
			m_transform.parent = m_oldParent;
			m_oldParent = null;
		}

		m_damage = _damage;
		if (m_explosive != null) {
			m_explosive.damage = m_damage;
		}

		m_velocity = m_direction * _speed;

		m_position = m_transform.position;
		m_lastPosition = m_position;
		m_startPosition = m_position;

		Vector3 newDir = Vector3.RotateTowards(Vector3.forward, -m_direction, 2f * Mathf.PI, 0.0f);
		m_transform.rotation = Quaternion.AngleAxis(90f, newDir) * Quaternion.LookRotation(newDir);
		m_transform.localScale = Vector3.one;

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
	protected virtual void Update () {
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
						m_transform.rotation = Quaternion.AngleAxis(90f, dir) * Quaternion.LookRotation(dir);
					}
				}

				if (m_rotationSpeed > 0f) {
					Vector3 axis = m_transform.up;

					if (m_rotationAxis == RotationAxis.Right) 			axis = m_transform.right;
					else if (m_rotationAxis == RotationAxis.Forward)	axis = m_transform.forward;

					m_transform.rotation = Quaternion.AngleAxis(m_elapsedTime * 240f * m_rotationSpeed, axis) * m_transform.rotation;
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
                // Update target position
                if ( m_target != null )
                    m_targetPosition = m_target.position;
                                    
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
							if (m_homingTimer <= 0f ) {
								m_homingTimer = 0f;
								Vector3 impulse = (m_targetPosition - m_position).normalized * m_speed;
								impulse = (impulse - m_velocity) / m_mass; //mass
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
				m_transform.position = m_position;

				// impact checks
				if (m_stopAtTarget || ( m_target == null && m_motionType == MotionType.Homing )) {
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

	public override void OnTriggerEnter(Collider _other) {
		if (m_state == State.Shot) {
			if (m_machine == null || !m_machine.IsDying()) {
				m_hitCollider = _other;
				if (_other.CompareTag("Player"))  {
					Explode(true);
                } else if ((((1 << _other.gameObject.layer) & GameConstants.Layers.GROUND) > 0)) {
					Explode(false);
				}
			}

			//Debug.Log(name + " >> " + _other.name);
		}
	}

    public override void OnTriggerStay(Collider _other) { }
    public override void OnTriggerExit(Collider _other) { }

    public void OnBite() {
        m_state = State.Idle;

        GameObject go = m_onEatParticle.Spawn(m_position + m_onEatParticle.offset);

        if (go != null) {
            FollowTransform ft = go.GetComponent<FollowTransform>();
            if (ft != null) {
                ft.m_follow = InstanceManager.player.dragonEatBehaviour.mouth;
                ft.m_offset = m_onEatParticle.offset;
            }
        }
    }

	public void OnEaten() {		
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

	public void OnDestoyed(){
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

	public void Explode(bool _dealDamage) {
		if (m_damageType == DamageType.EXPLOSION || m_damageType == DamageType.MINE) {
            DealExplosiveDamage(_dealDamage);
		} else {
			if (_dealDamage) {
                DealDamage();
			}

			if (m_missHitSpawnsParticle || _dealDamage) {				
				m_onHitParticle.Spawn(m_position + m_onHitParticle.offset, m_transform.rotation);
			}
		}

		if (!string.IsNullOrEmpty(m_onHitAudio))
			AudioController.Play(m_onHitAudio, m_transform.position);

        if (m_dieOnHit || !_dealDamage) {
            if (m_entity != null) {
                if (EntityManager.instance != null) {
                    EntityManager.instance.UnregisterEntity(m_entity);
                }
            }

            if (m_stickOnDragonTime > 0f && _dealDamage) {
                StickOnCollider();
            } else {
                Die();
            }
        }
	}

    protected virtual void DealExplosiveDamage(bool _triggeredByPlayer) {
        DragonPlayer player = InstanceManager.player;

        // dealing damage
        float actualKnockback = m_knockback;
        if (player.data.tier > m_knockbackTier)
        {
            actualKnockback = 0f;
        }

        m_explosive.Explode(transform, actualKnockback, _triggeredByPlayer);
    }

    protected virtual void DealDamage() {
        DragonPlayer player = InstanceManager.player;

        // dealing damage
        float actualKnockback = m_knockback;
        if (player.data.tier > m_knockbackTier) {
            actualKnockback = 0f;
        }

        if (actualKnockback > 0) {
            DragonMotion dragonMotion = player.dragonMotion;

            Vector3 knockBackDirection = dragonMotion.transform.position - m_transform.position;
            knockBackDirection.z = 0f;
            knockBackDirection.Normalize();

            dragonMotion.AddForce(knockBackDirection * actualKnockback);
        }

        player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType, m_source);
    }

	private void StickOnCollider() {
		m_state = State.Stuck_On_Player;

		for (int i = 0; i < m_activateOnShoot.Count; i++) {
			m_activateOnShoot[i].SetActive(false);
		}

		m_transform.parent = m_hitCollider.transform;
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

	protected void Die() {
		m_timer = m_dieTime;
		m_state = State.Die;
	}
}
