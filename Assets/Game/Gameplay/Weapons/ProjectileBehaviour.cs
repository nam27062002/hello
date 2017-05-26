using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ProjectileBehaviour : MonoBehaviour, IProjectile {

	[SerializeField] private ParticleData m_explosionParticle;
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);
	[SerializeField] private float m_knockback = 0;
	[SerializeField] private DamageType m_damageType = DamageType.NORMAL;

	private float m_damage;
	private bool m_hasBeenShot;

	private Transform m_oldParent = null;

	// private Vector2 m_targetCenter;
	private ProjectileMotion m_pMotion;

	public List<GameObject> m_activateOnShoot = new List<GameObject>();

	private PoolHandler m_poolHandler;


	// Use this for initialization
	void Start () {		
		if (m_explosionParticle.IsValid()) {
			ParticleManager.CreatePool(m_explosionParticle);
		}

		m_pMotion = GetComponent<ProjectileMotion>();	
		if (m_pMotion) m_pMotion.enabled = false;
	
		m_hasBeenShot = false;

		m_poolHandler = PoolManager.GetHandler(gameObject.name);
	}

	void OnDisable()
	{
		for( int i = 0; i<m_activateOnShoot.Count; i++ )
		{
			m_activateOnShoot[i].SetActive(false);
		}
	}

	public void AttachTo(Transform _parent) {		
		AttachTo(_parent, Vector3.zero);
	}

	public void AttachTo(Transform _parent, Vector3 _offset) {
		//init stuff
		Initializable[] components = GetComponents<Initializable>();		
		foreach (Initializable component in components) {
			component.Initialize();
		}

		//save real parent to restore this when the arrow is shot
		m_oldParent = transform.parent;

		//reset transforms, so we don't have any displacement
		transform.parent = _parent;
		transform.localPosition = _offset;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		//disable everything
		if (m_pMotion) m_pMotion.enabled = false;

		//wait until the projectil is shot
		m_hasBeenShot = false;
	}

	public void Shoot(Transform _target, Vector3 _direction, float _damage) {
		// m_targetCenter = InstanceManager.player.transform.position;
		ShootAtPosition(_target.position, _direction, _damage);
	}

	public void ShootTowards(Vector3 _direction, float _speed, float _damage) {}

	public void ShootAtPosition(Vector3 _target, Vector3 _direction, float _damage) {
		if (m_oldParent) {
			transform.parent = m_oldParent;
			m_oldParent = null;
		}

		if (m_pMotion) m_pMotion.enabled = true;

		if (m_pMotion != null) {
			Vector3 pos = InstanceManager.player.dragonMotion.head.position;
			m_pMotion.Shoot(pos);
		}

		EndShot(_damage);
	}

	private void EndShot( float _damage )
	{
		m_damage = _damage;
		m_hasBeenShot = true;
		for( int i = 0; i<m_activateOnShoot.Count; i++ )
		{
			m_activateOnShoot[i].SetActive(true);
		}
	}

	void Update() {
		if (m_hasBeenShot) {
			if (InstanceManager.gameCamera != null) {
				bool rem = InstanceManager.gameCamera.IsInsideDeactivationArea( transform.position );
				if (rem)
					Explode(false);	
			}
		}
	}
		
	void OnTriggerEnter(Collider _other) {
		if (m_hasBeenShot) {
			if (_other.CompareTag("Player"))  {
					Explode(true);
			} else if ((((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "GroundVisible")) > 0)) {
				Explode(false);
			}
		}
	}

	public void Explode(bool _hitDragon) {
		if (m_explosionParticle.IsValid()) {
			GameObject explosion = ParticleManager.Spawn( m_explosionParticle, transform.position);
			if (explosion) {
				// Random position within range
				// explosion.transform.position = transform.position;
				// Random scale within range
				explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();			
				// Random rotation within range
				explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
			}
		}

		if (_hitDragon) {
			if (m_knockback > 0) {
				DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

				Vector3 knockBack = dragonMotion.transform.position - transform.position;
				knockBack.z = 0f;
				knockBack.Normalize();

				knockBack *= m_knockback;

				dragonMotion.AddForce(knockBack);
			}

			InstanceManager.player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType, transform);
		}

		gameObject.SetActive(false);
		m_poolHandler.ReturnInstance(gameObject);
	}
}
