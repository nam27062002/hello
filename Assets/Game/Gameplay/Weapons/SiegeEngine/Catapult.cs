﻿using UnityEngine;
using System;
using System.Collections;

public class Catapult : Initializable {

	[Serializable]
	private class ExtraToss {
		public float vAngleOffset = 0f;
		public float hAngleOffset = 0f;
		public float initialVelocityOffset = 0f;
		public Vector3 initialPositionOffset = Vector3.zero;
	}

	[SerializeField] private float m_vAngle = 45f;
	[SerializeField] private float m_initialVelocity = 10;
	[SerializeField] private Vector3 m_initialPosition = Vector3.zero;
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

	[SeparatorAttribute("Audio")]
	[SerializeField] private string m_onTossAudio = "";

	[SeparatorAttribute("Debug")]
	[SerializeField] private bool m_forcePreview = false;
	[SerializeField] private float m_previewStep = 1f;
	[SerializeField] private float m_previewMaxTime = 20f;
	[SerializeField] private Vector3 m_debugTarget = Vector3.zero;


	private float m_hAngle;
	private float m_timer;

	private bool m_toss;

	private Animator m_animator;
	private PreyAnimationEvents m_animEvents;
	private AutoSpawnBehaviour m_autoSpawner;
	private SiegeEngineOperatorSpawner m_operatorSpawner;

	private bool m_operatorAvailable;
	private GameObject[] m_ammo;
	private Transform m_ammoTransform;

	private Transform m_target;


	// Use this for initialization
	void Awake() {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponent<SiegeEngineOperatorSpawner>();

		m_ammo = new GameObject[m_extraProjectiles.Length + 1];
		GameObject projectilePrefab = Resources.Load<GameObject>("Game/Projectiles/" + m_ammoName);
		PoolManager.CreatePool(projectilePrefab, 3, true);

		FindAmmoSpawnTransform();
	}


	void Start () {
		GetHorizontalAngle();

		m_target = InstanceManager.player.transform;

		m_timer = 0;
		m_toss = false;

		m_animator	 = transform.FindComponentRecursive<Animator>();
		m_animEvents = transform.FindComponentRecursive<PreyAnimationEvents>();
		m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnLoadAmmo);
		m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnToss);
		m_animEvents.onAttackEnd		+= new PreyAnimationEvents.OnAttackEndDelegate(OnReload);
	}

	public override void Initialize() {
		m_timer = 0;
		m_toss = false;

		if (m_operatorSpawner.IsOperatorDead())
			m_operatorSpawner.Respawn();

		m_operatorAvailable = false;
	}

	// Update is called once per frame
	void Update () {
		if (m_autoSpawner == null)
			return;

		if (m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning) {	// if respawning we wait
			for (int i = 0; i < m_ammo.Length; i++) {
				if (m_ammo[i] != null) {
					m_ammo[i].GetComponent<CatapultAmmo>().Explode(false);
					m_ammo[i] = null;
				}
			}
			m_operatorAvailable = false;
			return;
		}

		if (m_operatorSpawner.IsOperatorDead()) {
			m_operatorAvailable = false;
			return;
		} else if (!m_operatorAvailable) {
			m_operatorAvailable = true;

			bool empty = true;
			for (int i = 0; i < m_ammo.Length && empty; i++) {
				empty = m_ammo[i] == null;
			}

			if (empty) { // reload time
				OnReload();
			}
		}

		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_toss = true;
				m_timer = 0f;
			}
		}

		if (m_toss) {
			if (m_target != null && Aim(m_target.position)) {
				if (!string.IsNullOrEmpty(m_onTossAudio) )
					AudioController.Play(m_onTossAudio, transform.position);
				m_animator.SetBool("toss", true);
				m_operatorSpawner.OperartorDoShoot();
				m_toss = false;
			}
		}
	}

	private void OnLoadAmmo() {
		int i;
		for (i = 0; i < m_ammo.Length - 1; i++) {
			if (m_ammo[i] == null) {
				m_ammo[i] = PoolManager.GetInstance(m_ammoName);

				CatapultAmmo catapultAmmo = m_ammo[i].GetComponent<CatapultAmmo>();
				catapultAmmo.AttachTo(m_ammoTransform);
				catapultAmmo.transform.localPosition = m_ammoTransform.rotation * m_extraProjectiles[i].initialPositionOffset;
			}
		}

		if (m_ammo[i] == null) {
			m_ammo[i] = PoolManager.GetInstance(m_ammoName);

			CatapultAmmo catapultAmmo = m_ammo[i].GetComponent<CatapultAmmo>();
			catapultAmmo.AttachTo(m_ammoTransform);
			catapultAmmo.transform.localPosition = m_ammoTransform.rotation * m_initialPosition;
		}

		m_timer = m_tossDelay;

		m_animator.SetBool("reload", false);
	}

	private void OnToss() {
		int i;
		for (i = 0; i < m_ammo.Length - 1; i++) {
			if (m_ammo[i] != null) {
				CatapultAmmo catapultAmmo = m_ammo[i].GetComponent<CatapultAmmo>();
				catapultAmmo.Toss(	m_initialVelocity + m_extraProjectiles[i].initialVelocityOffset, 
									m_vAngle + m_extraProjectiles[i].vAngleOffset, 
									m_hAngle + m_extraProjectiles[i].hAngleOffset
								 );

				m_ammo[i] = null;
			}
		}

		if (m_ammo[i] != null) {
			CatapultAmmo catapultAmmo = m_ammo[i].GetComponent<CatapultAmmo>();
			catapultAmmo.Toss(m_initialVelocity, m_vAngle, m_hAngle);
			m_ammo[i] = null;
		}

		m_animator.SetBool("toss", false);
	}

	private void OnReload() {
		m_animator.SetBool("reload", true);
		m_operatorSpawner.OperatorDoReload();
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
		return transform.position + m_eyeOffset.x * transform.forward + transform.forward * m_eyeRadius + m_eyeOffset.y * transform.up + m_eyeOffset.z * transform.right;
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
			float oldVAngle = m_vAngle;
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
			m_vAngle = oldVAngle;

			//--------------------------------------------------------------------------------
			DrawMinMaxToss(m_initialPosition, m_initialVelocity, m_vAngleMin, m_hAngle, maxTime, step);
			DrawMinMaxToss(m_initialPosition, m_initialVelocity, m_vAngleMax, m_hAngle, maxTime, step);

			//--------------------------------------------------------------------------------
			Gizmos.color = Colors.magenta;
			Gizmos.DrawWireSphere(Eye() + m_debugTarget, 0.5f);

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
