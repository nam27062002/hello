using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetStunProjectile :  MonoBehaviour, IProjectile { 

	[SerializeField] private ParticleData m_explosionParticle;
	[SerializeField] private float m_stunDuration;
	private Transform m_oldParent = null;
	private LayerMask m_colliderMask;
	private ProjectileMotion m_pMotion;
	private bool m_hasBeenShot;
	private PoolHandler m_poolHandler;

	public DragonTier m_maxValidTier = DragonTier.TIER_4;
	public DragonTier m_minValidTier = DragonTier.TIER_0;


	// Use this for initialization
	void Start () 
	{
		m_colliderMask = LayerMask.GetMask("Ground", "Water", "GroundVisible", "WaterPreys", "GroundPreys", "AirPreys");
		m_pMotion = GetComponent<ProjectileMotion>();	
		if (m_pMotion) m_pMotion.enabled = false;
		m_hasBeenShot = false;

		m_poolHandler = PoolManager.GetHandler(gameObject.name);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_hasBeenShot) 
		{
			if (InstanceManager.gameCamera != null)
			{
				bool rem = InstanceManager.gameCamera.IsInsideDeactivationArea( transform.position );
				if (rem)
					Explode(false);	
			}
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
		ShootAtPosition(_target.position, _direction, _damage);
	}

	public void ShootTowards(Vector3 _direction, float _speed, float _damage) {}

	public void ShootAtPosition(Vector3 _target, Vector3 _direction, float _damage) {
		if (m_oldParent) {
			transform.parent = m_oldParent;
			m_oldParent = null;
		}

		if (m_pMotion != null) {
			m_pMotion.enabled = true;
			m_pMotion.Shoot(_target);
		}
		m_hasBeenShot = true;
	}

	void OnCollisionEnter( Collision _collision )
	{
		// if the collision is ground -> Explode!!
		if(((1 << _collision.gameObject.layer) & m_colliderMask) > 0){
			AI.Machine machine = _collision.collider.GetComponent<AI.Machine>();

			OnCommonEnter( machine );
		}
	}

	void OnTriggerEnter( Collider _collider ){
		if(((1 << _collider.gameObject.layer) & m_colliderMask) > 0){
			AI.Machine machine = _collider.GetComponent<AI.Machine>();

			OnCommonEnter( machine );
		}
	}

	void OnCommonEnter( AI.Machine machine )
	{
		Explode(false);
		if ( machine )
		{
			Entity entity = machine.GetComponent<Entity>();
			if ( entity && entity.edibleFromTier >= m_minValidTier && entity.edibleFromTier <= m_maxValidTier)
			{
				machine.Stun(m_stunDuration);
			}
		}
	}

	public void Explode(bool _hitsDragon)
	{		
		m_explosionParticle.Spawn(transform.position);
		gameObject.SetActive(false);
		m_poolHandler.ReturnInstance( gameObject );
	}
}
