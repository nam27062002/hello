﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetFireBall :  MonoBehaviour, IProjectile { 

	[SerializeField] private ParticleData m_explosionParticle;
	[SerializeField] private DragonTier m_fireTier;

	CircleArea2D m_area;

	private Transform m_oldParent = null;
	private LayerMask m_colliderMask;
	private ProjectileMotion m_pMotion;
	private bool m_hasBeenShot;
	private Rect m_rect;


	// Use this for initialization
	void Start () 
	{
		m_area = GetComponent<CircleArea2D>();
		m_rect = new Rect();
		if (m_explosionParticle.IsValid()) {
			ParticleManager.CreatePool(m_explosionParticle, 5);
		}
		m_colliderMask = LayerMask.GetMask("Ground", "Water", "GroundVisible", "WaterPreys", "GroundPreys", "AirPreys");

		m_pMotion = GetComponent<ProjectileMotion>();	
		if (m_pMotion) m_pMotion.enabled = false;
		m_hasBeenShot = false;
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

	public void Shoot(Vector3 _target, float _damage = 0f) {

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

	public void ShootAtPosition( Transform _from, float _damage, Vector3 _pos){

		if (m_oldParent) {
			transform.parent = m_oldParent;
			m_oldParent = null;
		}

		if (m_pMotion != null) {
			m_pMotion.enabled = true;
			m_pMotion.Shoot(_pos);
		}
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
		if (m_explosionParticle.IsValid()) {
			GameObject explosion = ParticleManager.Spawn( m_explosionParticle, transform.position);
			/*
			if (explosion) {
				explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();			
				explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
			}
			*/
		}

		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_area.center, m_area.radius * 3);
		for (int i = 0; i < preys.Length; i++) 
		{
			if ( preys[i].IsBurnable(m_fireTier))
			{
				AI.Machine machine =  preys[i].GetComponent<AI.Machine>();
				if (machine != null) {
					machine.Burn(transform);
				}
			}
		}

		m_rect.center = m_area.center;
		m_rect.height = m_rect.width = m_area.radius;
		FirePropagationManager.instance.FireUpNodes( m_rect, Overlaps, m_fireTier, Vector3.zero);

		gameObject.SetActive(false);
		PoolManager.ReturnInstance( gameObject );
	}

	bool Overlaps( CircleAreaBounds _fireNodeBounds )
	{
		return m_area.Overlaps( _fireNodeBounds.center, _fireNodeBounds.radius);
	}
}
