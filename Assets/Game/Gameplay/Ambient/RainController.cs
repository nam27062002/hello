using UnityEngine;
using System.Collections;

public class RainController : MonoBehaviour 
{
	public float m_rainLocalMove = 2;
	ParticleSystem m_rainParticle;
	DragonMotion m_dragonMotion;
	Transform m_transform;

	// Use this for initialization
	void Start () 
	{
		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
		m_transform = transform;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 vel = m_dragonMotion.GetVelocity();
		Vector3 local = m_transform.localPosition;
		if ( vel.x  > 0 )
		{
			local.x = m_rainLocalMove;
		}
		else if ( vel.x < 0 )
		{
			local.x = -m_rainLocalMove;
		}
		else
		{
			local.x = 0;
		}


		m_transform.localPosition = local;
	}

}
