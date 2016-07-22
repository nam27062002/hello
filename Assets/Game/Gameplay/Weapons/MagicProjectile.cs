using UnityEngine;
using System.Collections;

public class MagicProjectile : MonoBehaviour, IProjectile {

	[SerializeField] private float m_damage = 5f;
	[SerializeField] private float m_damageDelay = 0f;

	[SerializeField] private EffectSettings m_effect;
	[SerializeField] private GameObject m_effectIdle;
	private Transform m_oldParent;

	private bool m_hasBeenShot;
	private bool m_isDragonHit;
	private float m_timer;


	void OnEnable() {
		m_effect.gameObject.SetActive(false);
		m_effectIdle.SetActive(false);
		m_hasBeenShot = false;
		m_isDragonHit = false;

		m_effect.CollisionEnter += OnCollisionEnter;
	}

	void OnDisable() {
		PoolManager.ReturnInstance(gameObject);
	}

	public void OnCollisionEnter(object _o, CollisionInfo _collision) {
		m_isDragonHit = true;
		m_timer = m_damageDelay;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_hasBeenShot) {
			if (m_isDragonHit) {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, transform, true);
					m_isDragonHit = false;
				}
			}
			if (!m_effect.gameObject.activeInHierarchy) {
				gameObject.SetActive(false);
			}
		}
	}

	public void AttachTo(Transform _parent) {
		m_oldParent = transform.parent;
		transform.parent = _parent;
		transform.position = Vector3.zero;
		transform.localPosition = Vector3.zero;
		m_effect.transform.position = Vector3.zero;
		m_effect.transform.localPosition = Vector3.zero;

		m_effect.gameObject.SetActive(false);
		m_effectIdle.SetActive(true);
	}

	public void Shoot(Transform _from, float _damage) {		
		transform.parent = m_oldParent;
		m_effect.Target = InstanceManager.player.gameObject;

		m_effectIdle.SetActive(false);
		m_effect.gameObject.SetActive(true);
		m_hasBeenShot = true;
	}
}
