using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ProjectileBehaviour : MonoBehaviour, IProjectile {

	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);

	private float m_damage;
	private bool m_hasBeenShot;

	private Transform m_oldParent = null;

	private Vector2 m_targetCenter;
	private ProjectileMotion m_pMotion;

	public List<GameObject> m_activateOnShoot = new List<GameObject>();


	// Use this for initialization
	void Start () {		
		if (m_explosionPrefab != null) {
			PoolManager.CreatePool(m_explosionPrefab, 5, false);
		}

		m_pMotion = GetComponent<ProjectileMotion>();	
		if (m_pMotion) m_pMotion.enabled = false;
	
		m_hasBeenShot = false;
	}

	void OnDisable()
	{
		for( int i = 0; i<m_activateOnShoot.Count; i++ )
		{
			m_activateOnShoot[i].SetActive(false);
		}
	}

	public void AttachTo(Transform _parent) {

		//init stuff
		Initializable[] components = GetComponents<Initializable>();		
		foreach (Initializable component in components) {
			component.Initialize();
		}

		//save real parent to restore this when the arrow is shot
		m_oldParent = transform.parent;

		//reset transforms, so we don't have any displacement
		transform.parent = _parent;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		//disable everything
		if (m_pMotion) m_pMotion.enabled = false;

		//wait until the projectil is shot
		m_hasBeenShot = false;
	}

	public void Shoot(Transform _from, float _damage) {	
		m_targetCenter = InstanceManager.player.transform.position;

		if (m_oldParent) {
			transform.parent = m_oldParent;
			m_oldParent = null;
		}

		if (m_pMotion) m_pMotion.enabled = true;

		if (m_pMotion != null) {
			Vector3 pos = InstanceManager.player.GetComponent<DragonMotion>().head.position;
			m_pMotion.Shoot(pos);
		}

		m_damage = _damage;

		m_hasBeenShot = true;

		for( int i = 0; i<m_activateOnShoot.Count; i++ )
		{
			m_activateOnShoot[i].SetActive(true);
		}
	}

	void Update() {
		if (m_hasBeenShot) {
			// The dragon may eat this projectile, so we disable the explosion if that happens 
			float distanceToTargetSqr = (m_targetCenter - (Vector2)transform.position).sqrMagnitude;
			if (distanceToTargetSqr <= 0.5f) {
				Explode(false);
			}
		}
	}
		
	void OnTriggerEnter(Collider _other) {
		if (m_hasBeenShot) {
			if (_other.tag == "Player")  {
				Explode(true);
			} else if ((((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "GroundVisible")) > 0)) {
				Explode(false);
			}
		}
	}

	public void Explode(bool _hitDragon) {
		if (m_explosionPrefab != null) {
			GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);			
			if (explosion) {
				// Random position within range
				explosion.transform.position = transform.position;			
				// Random scale within range
				explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();			
				// Random rotation within range
				explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
			}
		}

		if (_hitDragon) {
			InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, DamageType.NORMAL);
		}

		gameObject.SetActive(false);
		PoolManager.ReturnInstance(gameObject);
	}
}
