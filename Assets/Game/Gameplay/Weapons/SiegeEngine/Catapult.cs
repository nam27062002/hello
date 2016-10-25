using UnityEngine;
using System.Collections;

public class Catapult : MonoBehaviour {


	[SerializeField] private float m_vAngle = 45f;
	[SerializeField] private float m_initialVelocity = 10;

	[SeparatorAttribute]
	[SerializeField] private float m_tossDelay = 5f;
	[SerializeField] private string m_ammoName;

	[SeparatorAttribute]
	[SerializeField] private bool m_forcePreview = false;
	[SerializeField] private float m_previewStep = 1f;
	[SerializeField] private float m_previewMaxTime = 20f;


	private float m_hAngle;
	private float m_timer;

	private GameObject m_ammo;


	// Use this for initialization
	void Start () {
		GetHorizontalAngle();

		m_ammo = null;
		GameObject projectilePrefab = Resources.Load<GameObject>("Game/Projectiles/" + m_ammoName);
		PoolManager.CreatePool(projectilePrefab, 2, true);

		LoadAmmo();

		m_timer = m_tossDelay;
	}
	
	// Update is called once per frame
	void Update () {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			if (m_ammo != null) {
				CatapultAmmo catapultAmmo = m_ammo.GetComponent<CatapultAmmo>();
				catapultAmmo.Toss(m_initialVelocity, m_vAngle, m_hAngle);
			}

			LoadAmmo();
			m_timer = m_tossDelay;
		}
	}

	private void LoadAmmo() {
		m_ammo = PoolManager.GetInstance(m_ammoName);

		if (m_ammo != null) {
			CatapultAmmo catapultAmmo = m_ammo.GetComponent<CatapultAmmo>();
			catapultAmmo.AttachTo(transform);
		}
	}

	private void GetHorizontalAngle() {
		m_hAngle = transform.rotation.eulerAngles.y;
	}

	private float GetX(float _t) {
		return m_initialVelocity * Mathf.Cos(m_vAngle * Mathf.Deg2Rad) * Mathf.Sin(m_hAngle * Mathf.Deg2Rad) * _t;
	}

	private float GetY(float _t) {
		return m_initialVelocity * Mathf.Sin(m_vAngle * Mathf.Deg2Rad) * _t - (9.8f * _t * _t * 0.5f);
	}

	private float GetZ(float _t) {
		return m_initialVelocity * Mathf.Cos(m_vAngle * Mathf.Deg2Rad) * Mathf.Cos(m_hAngle * Mathf.Deg2Rad) * _t;
	}

	// 
	private void OnDrawGizmosSelected() {
		if (m_forcePreview || !Application.isPlaying) {
			GetHorizontalAngle();

			float time = 0;
			float maxTime = m_previewMaxTime;
			float step = Mathf.Max(0.5f, m_previewStep);

			Vector3 lastTarget = transform.position + Vector3.zero;
			Vector3 target = lastTarget;

			Gizmos.color = Color.white;
			for (time = step; time < maxTime ; time += step) {
				target = transform.position;
				target.x += GetX(time * 0.1f);
				target.y += GetY(time * 0.1f);
				target.z += GetZ(time * 0.1f);

				RaycastHit hitInfo;
				if (Physics.Linecast(lastTarget, target, out hitInfo)) {
					Gizmos.DrawLine(lastTarget, hitInfo.point);
					Gizmos.DrawWireSphere(hitInfo.point, 0.25f);
					break;
				}
				Gizmos.DrawLine(lastTarget, target);
				lastTarget = target;
			}
		}
	}
}
