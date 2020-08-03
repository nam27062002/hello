using UnityEngine;
using System.Collections;

public class RainController : MonoBehaviour 
{
	public float m_rainLocalMove = 2;
	ParticleSystem m_rainParticle;
	DragonMotion m_dragonMotion;
	Transform m_transform;
	int m_lastIntensity = 0;
	// Use this for initialization
	void Start () 
	{
		m_rainParticle = GetComponent<ParticleSystem>();
		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
		m_transform = transform;
		SetIntensity( m_lastIntensity );
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 vel = m_dragonMotion.velocity;
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

	public void SetIntensity( float intensity )
	{
		if ( m_rainParticle != null )
		{
			ParticleSystem.EmissionModule emission = m_rainParticle.emission;
			emission.enabled = intensity >= 1;

			ParticleSystem.MinMaxCurve curve = emission.rate;
			curve.constantMin = intensity;
			curve.constantMax = intensity;
			m_lastIntensity = (int)intensity;

			emission.rate = curve;
		}
	}

}
