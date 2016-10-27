using UnityEngine;
using System;
using System.Collections;

public class Catapult : Initializable {

	[Serializable]
	private class ExtraToss {
		public float vAngleOffset = 0f;
		public float hAngleOffset = 0f;
		public float initialVelocityOffset = 0f;
	}

	[SerializeField] private float m_vAngle = 45f;
	[SerializeField] private float m_initialVelocity = 10;
	[SerializeField] private ExtraToss[] m_extraProjectiles;

	[SeparatorAttribute]
	[SerializeField] private float m_vAngleMin = 0f;
	[SerializeField] private float m_vAngleMax = 60f;
	[SerializeField] private Vector3 m_eyeOffset = Vector3.zero;
	[SerializeField] private float m_eyeRadius = 5f;

	[SeparatorAttribute]
	[SerializeField] private float m_tossDelay = 5f;
	[SerializeField] private string m_ammoName;
	[SerializeField] private string m_ammoSpawnTransformName;

	[SeparatorAttribute]
	[SerializeField] private bool m_forcePreview = false;
	[SerializeField] private float m_previewStep = 1f;
	[SerializeField] private float m_previewMaxTime = 20f;


	private float m_hAngle;
	private float m_timer;

	private bool m_toss;

	private Animator m_animator;
	private PreyAnimationEvents m_animEvents;
	private AutoSpawnBehaviour m_autoSpawner;

	private GameObject m_ammo;
	private Transform m_ammoTransform;

	private Transform m_target;


	// Use this for initialization
	void Start () {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();

		GetHorizontalAngle();

		m_ammo = null;
		GameObject projectilePrefab = Resources.Load<GameObject>("Game/Projectiles/" + m_ammoName);
		PoolManager.CreatePool(projectilePrefab, 3, true);

		FindAmmoSpawnTransform();

		m_target = InstanceManager.player.transform;

		m_timer = 0;
		m_toss = false;

		m_animator	 = transform.FindComponentRecursive<Animator>();
		m_animEvents = transform.FindComponentRecursive<PreyAnimationEvents>();
		m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnLoadAmmo);
		m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnToss);
	}

	public override void Initialize() {
		m_timer = 0;
		m_toss = false;
	}

	// Update is called once per frame
	void Update () {
		if (m_autoSpawner == null)
			return;

		if (m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning) {	// if respawning we wait
			if (m_ammo != null) {
				m_ammo.GetComponent<CatapultAmmo>().Explode(false);
				m_ammo = null;
			}
			return;
		}

		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_toss = true;
				m_timer = 0f;
			}
		}

		if (m_toss) {
			if (Aim()) {
				m_animator.SetTrigger("toss");
				m_toss = false;
			}
		}
	}

	private void OnLoadAmmo() {
		m_ammo = PoolManager.GetInstance(m_ammoName);

		if (m_ammo != null) {
			CatapultAmmo catapultAmmo = m_ammo.GetComponent<CatapultAmmo>();
			catapultAmmo.AttachTo(m_ammoTransform);
		}

		m_timer = m_tossDelay;
	}

	private void OnToss() {
		if (m_ammo != null) {
			CatapultAmmo catapultAmmo = m_ammo.GetComponent<CatapultAmmo>();
			catapultAmmo.Toss(m_initialVelocity, m_vAngle, m_hAngle);

			if (m_extraProjectiles != null) {				
				for (int i = 0; i < m_extraProjectiles.Length; i++) {
					GameObject extraAmmo = PoolManager.GetInstance(m_ammoName);
					catapultAmmo = extraAmmo.GetComponent<CatapultAmmo>();
					catapultAmmo.AttachTo(m_ammoTransform);

					catapultAmmo.Toss(	m_initialVelocity + m_extraProjectiles[i].initialVelocityOffset, 
										m_vAngle + m_extraProjectiles[i].vAngleOffset, 
										m_hAngle + m_extraProjectiles[i].hAngleOffset
									 );
				}
			}
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
		zero.y = _vo * Mathf.Sin(_vAngle * Mathf.Deg2Rad) * _t - (9.8f * _t * _t * 0.5f);;
		zero.z = _vo * Mathf.Cos(_vAngle * Mathf.Deg2Rad) * Mathf.Cos(_hAngle * Mathf.Deg2Rad) * _t;;

		return zero;
	}

	private Vector3 Eye() {
		return m_ammoTransform.position + m_eyeOffset.x * transform.forward + transform.forward * m_eyeRadius + m_eyeOffset.y * transform.up + m_eyeOffset.z * transform.right;
	}

	private bool Aim() {
		if (m_target != null) {
			Vector3 eye = Eye();
			float sqrD = (m_target.position - eye).sqrMagnitude;
			if (sqrD <= m_eyeRadius * m_eyeRadius) {
				float r = m_eyeRadius * 0.5f;
				float dY = (m_target.position.y - eye.y + r) / (r * 2f);

				m_vAngle = m_vAngleMin + (m_vAngleMax - m_vAngleMin) * dY;
				if (m_vAngle > m_vAngleMax) m_vAngle = m_vAngleMax;
				else if (m_vAngle < m_vAngleMin) m_vAngle = m_vAngleMin;
								
				return true;
			}
		}

		return false;
	}

	// Tools and Debug
	private void OnDrawGizmosSelected() {
		if (m_forcePreview || !Application.isPlaying) {
			FindAmmoSpawnTransform();
			GetHorizontalAngle();

			float maxTime = m_previewMaxTime;
			float step = Mathf.Max(0.5f, m_previewStep);

			//--------------------------------------------------------------------------------
			DrawToss(m_initialVelocity, m_vAngle, m_hAngle, maxTime, step);

			if (m_extraProjectiles != null) {
				for (int i = 0; i < m_extraProjectiles.Length; i++) {
					DrawToss(	m_initialVelocity + m_extraProjectiles[i].initialVelocityOffset, 
								m_vAngle + m_extraProjectiles[i].vAngleOffset, 
								m_hAngle + m_extraProjectiles[i].hAngleOffset,
								maxTime, step);
				}
			}

			//--------------------------------------------------------------------------------
			DrawMinMaxToss(m_initialVelocity, m_vAngleMin, m_hAngle, maxTime, step);
			DrawMinMaxToss(m_initialVelocity, m_vAngleMax, m_hAngle, maxTime, step);

			//--------------------------------------------------------------------------------
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Eye(), m_eyeRadius);
		}
	}

	private void DrawToss(float _vo, float _vAngle, float _hAngle, float _maxTime, float _step) {
		Vector3 lastTarget = m_ammoTransform.position + Vector3.zero;
		Vector3 target = lastTarget;

		Gizmos.color = Color.white;
		for (float time = _step; time < _maxTime ; time += _step) {
			target = m_ammoTransform.position + GetTargetAt(time * 0.1f, _vo, _vAngle, _hAngle);

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
	}

	private void DrawMinMaxToss(float _vo, float _vAngle, float _hAngle, float _maxTime, float _step) {
		Vector3 lastTarget = m_ammoTransform.position + Vector3.zero;
		Vector3 target = lastTarget;

		Gizmos.color = Color.cyan;
		for (float time = _step; time < _maxTime ; time += _step) {
			target = m_ammoTransform.position + GetTargetAt(time * 0.1f, _vo, _vAngle, _hAngle);

			if (Physics.Linecast(lastTarget, target)) { break; }

			Gizmos.DrawSphere(target, 0.05f);
			lastTarget = target;
		}
	}
}
