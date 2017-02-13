using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetReviveBall :  MonoBehaviour, IProjectile { 

	[SerializeField] private ParticleData m_explosionParticle;

	private Transform m_oldParent = null;
	private LayerMask m_colliderMask;
	private ProjectileMotion m_pMotion;


	// Use this for initialization
	void Start () 
	{
		m_colliderMask = LayerMask.GetMask("Player");
		m_pMotion = GetComponent<ProjectileMotion>();	
		if (m_pMotion) m_pMotion.enabled = false;
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

	}

	void OnCollisionEnter( Collision _collision )
	{
		// if the collision is ground -> Explode!!
		if(((1 << _collision.gameObject.layer) & m_colliderMask) > 0)
			Explode(true);
	}

	void OnTriggerEnter( Collider _other)
	{
		if(((1 << _other.gameObject.layer) & m_colliderMask) > 0)
			Explode(true);
	}

	public void Explode( bool _hitsDragon )
	{
		if (m_explosionParticle.IsValid()) {
			// Instantiate particle
			GameObject go = Resources.Load<GameObject>("Particles/" + m_explosionParticle.path + m_explosionParticle.name);
			GameObject instance = GameObject.Instantiate(go);
			instance.transform.position = transform.position + m_explosionParticle.offset;
		}

		InstanceManager.player.ResetStats(true, DragonPlayer.ReviveReason.FREE_REVIVE_PET);	// do it on next update?
		Messenger.Broadcast(GameEvents.PLAYER_FREE_REVIVE);

		gameObject.SetActive(false);
		Destroy( gameObject );
	}
}
