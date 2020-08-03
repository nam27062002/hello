using UnityEngine;
using System.Collections;

public class Catapult : SimpleDevice {
	[System.Serializable]
	public class AmmoPrefab {
		public string name = "";
		public float chance = 100;

		public AmmoPrefab() {
			name = "";
			chance = 100;
		}
	}

	private enum State {		
		Reload = 0,
		Loaded,
		Toss
	}

	[System.Serializable]
	private class ExtraToss {
		public float vAngleOffset = 0f;
		public float hAngleOffset = 0f;
		public float initialVelocityOffset = 0f;
		public Vector3 initialPositionOffset = Vector3.zero;
	}

	[SerializeField] private float m_vAngleMin = 0f;
	[SerializeField] private float m_vAngleMax = 60f;
	[SerializeField] private float m_initialVelocity = 10f;
	[SerializeField] private Vector3 m_initialPosition = Vector3.zero;
	[SerializeField] private ExtraToss[] m_extraProjectiles;

	[SeparatorAttribute]
	[SerializeField] private Vector3 m_eyeOffset = Vector3.zero;
	[SerializeField] private float m_eyeRadius = 5f;

	[SeparatorAttribute]
	[SerializeField] private float m_damage = 20f;
	[SerializeField] private float m_tossDelay = 5f;
	[SerializeField] private AmmoPrefab[] m_ammoList = new AmmoPrefab[1];
	[SerializeField] private string m_ammoSpawnTransformName;

	[SeparatorAttribute("Audio")]
	[SerializeField] private string m_onTossAudio = "";

    [SeparatorAttribute]
    [SerializeField] private DestructibleDecoration m_destructibleTrigger = null;

    [SeparatorAttribute("Debug")]
	[SerializeField] private bool m_forcePreview = false;
	[SerializeField] private float m_previewStep = 1f;
	[SerializeField] private float m_previewMaxTime = 20f;
	[SerializeField] private Vector3 m_debugTarget = Vector3.zero;

	private float m_vAngle;
	private float m_hAngle;
	private float m_timer;


	private Transform m_transform;
	private Animator m_animator;
	private PreyAnimationEvents m_animEvents;

	private PoolHandler[] m_ammoPoolHandlers;
	private GameObject[] m_ammo;
	private Transform m_ammoTransform;

	private Transform m_target;

	private State m_state;

    // Use this for initialization
    protected override void Awake() {
        base.Awake();

		m_transform = transform;
		m_ammo = new GameObject[m_extraProjectiles.Length + 1];
		m_ammoPoolHandlers = new PoolHandler[m_ammoList.Length];

		float probFactor = 0;
		for (int i = 0; i < m_ammoList.Length; i++) {
			probFactor += m_ammoList[i].chance;
		}

		if (probFactor > 0f) {
			probFactor = 100f / probFactor;
			for (int i = 0; i < m_ammoList.Length; i++) {
				m_ammoList[i].chance *= probFactor;
			}

			//sort probs
			for (int i = 0; i < m_ammoList.Length; i++) {
				for (int j = 0; j < m_ammoList.Length - i - 1; j++) {
					if (m_ammoList[j].chance > m_ammoList[j + 1].chance) {
						AmmoPrefab temp = m_ammoList[j];
						m_ammoList[j] = m_ammoList[j + 1];
						m_ammoList[j + 1] = temp;
					}
				}
			}

			for (int i = 0; i < m_ammoList.Length; i++) {
				m_ammoPoolHandlers[i] = PoolManager.RequestPool(m_ammoList[i].name, 3);
			}
		}

		FindAmmoSpawnTransform();

		GetHorizontalAngle();

		m_target = InstanceManager.player.transform;

		m_timer = 0;

		m_animator	 = transform.FindComponentRecursive<Animator>();
		m_animEvents = transform.FindComponentRecursive<PreyAnimationEvents>();
		m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnLoadAmmo);
		m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnToss);
		m_animEvents.onAttackEnd		+= new PreyAnimationEvents.OnAttackEndDelegate(OnReload);

        m_destructibleTrigger.onDestroy += OnDestroy;

        m_timer = 0;
		m_state = State.Reload;
	}

	// Update is called once per frame
	protected override void ExtendedUpdate() {	
		if (m_state == State.Loaded) {

			// Catapult ammo may be eaten!
			for (int i = 0; i < m_ammo.Length; i++) {
				if (m_ammo[i] != null) {
					if (!m_ammo[i].activeInHierarchy) {
						m_ammo[i] = null;
					}
				}
			}

			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				if (m_target != null && Aim(m_target.position)) {
					if (!string.IsNullOrEmpty(m_onTossAudio))
						AudioController.Play(m_onTossAudio, transform.position);
					m_animator.SetBool( GameConstants.Animator.TOSS , true);
					m_operatorSpawner.OperatorDoActionB();
					m_state = State.Toss;
				}
				m_timer = 0f;
			}
		}
	}

	protected override void OnRespawning() {
        m_animator.speed = 1;
		for (int i = 0; i < m_ammo.Length; i++) {
			if (m_ammo[i] != null) {
				m_ammo[i].GetComponent<Projectile>().Explode(false);
				m_ammo[i] = null;
			}
		}
    }

    protected override void OnRespawn() {
        m_vAngle = (m_vAngleMax + m_vAngleMin) * 0.5f;

        m_timer = 0;
        m_animator.speed = 1;

        m_animator.SetBool(GameConstants.Animator.TOSS, false);

        OnReload();
    }

    protected override void OnOperatorDead() {
		m_animator.speed = 0;		
	}

	protected override void OnOperatorSpawned() {
		m_animator.speed = 1;
		if (m_state == State.Reload) { // reload time
			OnReload();
		}
	}

	private void OnLoadAmmo() {
		int ammoIndex = GetAmmoIndex();

		int i;
		for (i = 0; i < m_ammo.Length - 1; i++) {
			if (m_ammo[i] == null) {
				m_ammo[i] = m_ammoPoolHandlers[ammoIndex].GetInstance();

                if (m_ammo[i] != null) {
                    Projectile catapultAmmo = m_ammo[i].GetComponent<Projectile>();
                    catapultAmmo.AttachTo(m_ammoTransform, m_ammoTransform.rotation * m_extraProjectiles[i].initialPositionOffset);
                }
			}
		}

		if (m_ammo[i] == null) {
			m_ammo[i] = m_ammoPoolHandlers[ammoIndex].GetInstance();

            if (m_ammo[i] != null) {
                Projectile catapultAmmo = m_ammo[i].GetComponent<Projectile>();
                catapultAmmo.AttachTo(m_ammoTransform, m_ammoTransform.rotation * m_initialPosition);
            }
		}

		m_timer = m_tossDelay;

		m_animator.SetBool( GameConstants.Animator.RELOAD , false);
		m_operatorSpawner.OperatorDoIdle();

		m_state = State.Loaded;
	}

	private int GetAmmoIndex() {
		float rand = Random.Range(0f, 100f);
		float prob = 0;
		int i = 0;

		for (i = 0; i < m_ammoList.Length - 1; i++) {
			prob += m_ammoList[i].chance;

			if (rand <= prob) {
				break;
			} 

			rand -= prob;
		}

		return i;
	}

	private void OnToss() {
		int i;
		for (i = 0; i < m_ammo.Length - 1; i++) {
			if (m_ammo[i] != null) {
				Projectile catapultAmmo = m_ammo[i].GetComponent<Projectile>();
				Vector3 direction = DirectionFromAngles(m_vAngle + m_extraProjectiles[i].vAngleOffset, 
														m_hAngle + m_extraProjectiles[i].hAngleOffset);

				catapultAmmo.ShootTowards(direction, m_initialVelocity + m_extraProjectiles[i].initialVelocityOffset, m_damage, m_transform);

				m_ammo[i] = null;
			}
		}

		if (m_ammo[i] != null) {
			Projectile catapultAmmo = m_ammo[i].GetComponent<Projectile>();
			Vector3 direction = DirectionFromAngles(m_vAngle, m_hAngle);

			catapultAmmo.ShootTowards(direction, m_initialVelocity, m_damage, m_transform);

			m_ammo[i] = null;
		}

		m_animator.SetBool( GameConstants.Animator.TOSS , false);
	}

	private Vector3 DirectionFromAngles(float _vAngle, float _hAngle) {
		return Quaternion.AngleAxis(_hAngle, Vector3.up) * Quaternion.AngleAxis(_vAngle, Vector3.left) * Vector3.forward;
	}

	private void OnReload() {
		if (m_operatorAvailable) {
			m_animator.SetBool( GameConstants.Animator.RELOAD , true);
			m_operatorSpawner.OperatorDoActionA();
		}

		m_state = State.Reload;
	}

    private void OnDestroy() {
        if (m_operatorAvailable && m_operatorSpawner != null) {
            m_operatorSpawner.OperatorDoScared();
        }
    }

    private void FindAmmoSpawnTransform() {
		m_ammoTransform = transform.FindTransformRecursive(m_ammoSpawnTransformName);
	}

	private void GetHorizontalAngle() {
		m_hAngle = transform.rotation.eulerAngles.y;
	}

	private Vector3 GetTargetAt(float _t, float _vo, float _vAngle, float _hAngle) {
		Vector3 zero = Vector3.zero;

		zero.x = _vo * Mathf.Cos(_vAngle * Mathf.Deg2Rad) * Mathf.Sin(_hAngle * Mathf.Deg2Rad) * _t;
		zero.y = _vo * Mathf.Sin(_vAngle * Mathf.Deg2Rad) * _t - (9.8f * _t * _t * 0.5f);
		zero.z = _vo * Mathf.Cos(_vAngle * Mathf.Deg2Rad) * Mathf.Cos(_hAngle * Mathf.Deg2Rad) * _t;

		return zero;
	}

	private Vector3 Eye() {
		Vector3 eye = transform.position + m_eyeOffset.x * transform.forward + transform.forward * m_eyeRadius + m_eyeOffset.y * transform.up + m_eyeOffset.z * transform.right;
		eye.z = 0f;
		return eye;
	}

	private bool Aim(Vector3 _target) {		
		Vector3 eye = Eye();
		float sqrD = (_target - eye).sqrMagnitude;
		if (sqrD <= m_eyeRadius * m_eyeRadius) {
			float r = m_eyeRadius * 0.5f;
			float dY = (_target.y - eye.y + r) / (r * 2f);

			m_vAngle = m_vAngleMin + (m_vAngleMax - m_vAngleMin) * dY;
			if (m_vAngle > m_vAngleMax) m_vAngle = m_vAngleMax;
			else if (m_vAngle < m_vAngleMin) m_vAngle = m_vAngleMin;
							
			return true;
		}

		return false;
	}

	protected override void OnAreaExit() {
		base.OnAreaExit();
        if (m_animator != null) {
            m_animator.speed = 0;
        }
	}

	//-------------------------------------------------------------------
	// Debug
	//-------------------------------------------------------------------
	private void OnDrawGizmosSelected() {
		if (m_forcePreview || !Application.isPlaying) {
			FindAmmoSpawnTransform();
			GetHorizontalAngle();

			float maxTime = m_previewMaxTime;
			float step = Mathf.Max(0.5f, m_previewStep);

			//--------------------------------------------------------------------------------
			Aim(Eye() + m_debugTarget);
			DrawToss(Colors.magenta, m_initialPosition, m_initialVelocity, m_vAngle, m_hAngle, maxTime, step);

			if (m_extraProjectiles != null) {
				for (int i = 0; i < m_extraProjectiles.Length; i++) {
					DrawToss(	Colors.coral,
								m_extraProjectiles[i].initialPositionOffset,
								m_initialVelocity + m_extraProjectiles[i].initialVelocityOffset, 
								m_vAngle + m_extraProjectiles[i].vAngleOffset, 
								m_hAngle + m_extraProjectiles[i].hAngleOffset,
								maxTime, step);
				}
			}

			//--------------------------------------------------------------------------------
			DrawMinMaxToss(m_initialPosition, m_initialVelocity, m_vAngleMin, m_hAngle, maxTime, step);
			DrawMinMaxToss(m_initialPosition, m_initialVelocity, m_vAngleMax, m_hAngle, maxTime, step);

			//--------------------------------------------------------------------------------
			Gizmos.color = Colors.magenta;
			Gizmos.DrawSphere(Eye() + m_debugTarget, 1.25f);

			//--------------------------------------------------------------------------------
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Eye(), m_eyeRadius);
		}
	}

	private void DrawToss(Color _color, Vector3 _po, float _vo, float _vAngle, float _hAngle, float _maxTime, float _step) {
		Vector3 lastTarget = m_ammoTransform.position + _po;
		Vector3 target = lastTarget;

		Gizmos.color = _color;
		for (float time = _step; time < _maxTime ; time += _step) {
			target = m_ammoTransform.position + _po + GetTargetAt(time * 0.1f, _vo, _vAngle, _hAngle);

			RaycastHit hitInfo;
			if (Physics.Linecast(lastTarget, target, out hitInfo)) {
				Gizmos.color = Color.white;
				Gizmos.DrawLine(lastTarget, hitInfo.point);
				Gizmos.DrawWireSphere(hitInfo.point, 0.25f);
				break;
			}

			Gizmos.DrawLine(lastTarget, target);

			lastTarget = target;
		}

		Vector3 direction = DirectionFromAngles(_vAngle, _hAngle);
		Gizmos.DrawLine(m_ammoTransform.position + _po, m_ammoTransform.position + _po + (direction * 2f));
	}

	private void DrawMinMaxToss(Vector3 _po, float _vo, float _vAngle, float _hAngle, float _maxTime, float _step) {
		Vector3 lastTarget = m_ammoTransform.position + _po;
		Vector3 target = lastTarget;

		Gizmos.color = Color.cyan;
		for (float time = _step; time < _maxTime ; time += _step) {
			target = m_ammoTransform.position + _po + GetTargetAt(time * 0.1f, _vo, _vAngle, _hAngle);

			if (Physics.Linecast(lastTarget, target)) { break; }

			Gizmos.DrawSphere(target, 0.15f);
			lastTarget = target;
		}
	}
}
