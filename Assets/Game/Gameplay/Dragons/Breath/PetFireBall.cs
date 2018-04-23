using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetFireBall :  MonoBehaviour, IProjectile { 

	[SerializeField] private string m_viewParticle;
	[SerializeField] private ParticleData m_explosionParticle;
	[SerializeField] private DragonTier m_fireTier;

	CircleArea2D m_area;

	private Transform m_oldParent = null;
	private LayerMask m_colliderMask;
	private ProjectileMotion m_pMotion;
	private bool m_hasBeenShot;
	private Rect m_rect;

	private PoolHandler m_poolHandler;
	private ParticleSystem m_fireView;
	private Transform m_target;


	void Awake()
	{
		m_area = GetComponent<CircleArea2D>();
		m_rect = new Rect();
		m_colliderMask = LayerMask.GetMask("Ground", "Water", "GroundVisible", "WaterPreys", "GroundPreys", "AirPreys");
		m_pMotion = GetComponent<ProjectileMotion>();	
		if (m_pMotion) m_pMotion.enabled = false;
		m_hasBeenShot = false;

	}
	// Use this for initialization
	void Start () 
	{
		// View particle
		string version = "";
		switch(FeatureSettingsManager.instance.Particles)
		{
			default:
			case FeatureSettings.ELevel5Values.very_low:							
			case FeatureSettings.ELevel5Values.low:
					version = "Low/";
				break;
			case FeatureSettings.ELevel5Values.mid:
					version = "Master/";
				break;
			case FeatureSettings.ELevel5Values.very_high:
			case FeatureSettings.ELevel5Values.high:
					version = "High/";
				break;
		}

		string path = "Particles/" + version + m_viewParticle;

		GameObject prefab = Resources.Load<GameObject>(path);
		if ( prefab )
		{
			m_fireView = Instantiate<GameObject>(prefab).GetComponent<ParticleSystem>();
			if ( m_fireView )
			{
				// Anchor
				m_fireView.transform.SetParent(transform, true);
				m_fireView.transform.localPosition = GameConstants.Vector3.zero;
				m_fireView.transform.localRotation = GameConstants.Quaternion.identity;
			}
		}
		////
		m_explosionParticle.CreatePool();
		m_poolHandler = PoolManager.GetHandler(gameObject.name);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_hasBeenShot) 
		{
			if ( m_pMotion.m_moveType == ProjectileMotion.Type.Missile)
			{
				if (m_target != null)
				{
					// Update Target
					m_pMotion.target = m_target.position;
				}
				else
				{
					Explode(false);
				}
			}
			// Update Target?
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

	public void Shoot(Transform _target, Vector3 _direction, float _damage, Transform _source) {
		ShootAtPosition(_target.position, _direction, _damage, _source);
		m_target = _target;
	}

	public void ShootTowards(Vector3 _direction, float _speed, float _damage, Transform _source) {}

	public void ShootAtPosition(Vector3 _target, Vector3 _direction, float _damage, Transform _source) {
		if (m_oldParent) {
			transform.parent = m_oldParent;
			m_oldParent = null;
		}

		if (m_pMotion != null) {
			m_pMotion.enabled = true;
			m_pMotion.Shoot(_target);
		}
		if ( m_fireView != null )
			m_fireView.Play();
		m_target = null;
		m_hasBeenShot = true;
	}



	void OnCollisionEnter( Collision _collision )
	{
		// if the collision is ground -> Explode!!
		if(((1 << _collision.gameObject.layer) & m_colliderMask) > 0)
			Explode(false);
	}

	void OnTriggerEnter( Collider _other)
	{
		if(((1 << _other.gameObject.layer) & m_colliderMask) > 0)
			Explode(false);
	}

	public void Explode( bool _hitsDragon )
	{		
		if ( !m_hasBeenShot ) return;

		m_explosionParticle.Spawn(transform.position);

		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_area.center, m_area.radius * 3);
		for (int i = 0; i < preys.Length; i++) {
			if (preys[i].IsBurnable(m_fireTier)) {
				AI.IMachine machine =  preys[i].machine;
				if (machine != null) {
					machine.Burn(transform, IEntity.Type.PET);
				}
			}
		}

		m_rect.center = m_area.center;
		m_rect.height = m_rect.width = m_area.radius;
		FirePropagationManager.instance.FireUpNodes(m_rect, Overlaps, m_fireTier, DragonBreathBehaviour.Type.None, Vector3.zero, IEntity.Type.PET);

		m_hasBeenShot = false;
		if (m_pMotion != null)
			m_pMotion.enabled = false;
		m_fireView.Stop();
		StartCoroutine(DelayedDeactivate());
	}

	IEnumerator DelayedDeactivate()
	{
		yield return new WaitForSeconds(1.0f);
		gameObject.SetActive(false);
		m_poolHandler.ReturnInstance( gameObject );
	}

	bool Overlaps( CircleAreaBounds _fireNodeBounds )
	{
		return m_area.Overlaps( _fireNodeBounds.center, _fireNodeBounds.radius);
	}
}
