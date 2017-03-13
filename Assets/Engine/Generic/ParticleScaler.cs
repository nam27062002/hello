using UnityEngine;
using System.Collections;


[RequireComponent( typeof(ParticleSystem) )]
public class ParticleScaler : MonoBehaviour 
{

	public float m_scale = 1;
	public bool m_useDragonSize;
	// Use this for initialization
	void Start () 
	{
		if (m_useDragonSize)
		{
			Scale( InstanceManager.player.data.scale );
		}
		else
		{
			Scale( m_scale );
		}
	}

	void Scale( float scale )
	{	
		m_scale = scale;
		// transform.localScale *= scale;
		ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
		foreach( ParticleSystem p in childs )
			ScaleParticle( p, scale );
	}
	
	void ScaleParticle( ParticleSystem ps, float scale)
	{
		ParticleSystem.MainModule mainModule = ps.main;
		mainModule.startSizeMultiplier *= scale;
		mainModule.gravityModifierMultiplier *= scale;
		mainModule.startSpeedMultiplier *= scale;
		mainModule.startLifetimeMultiplier *= scale;
		// ps.main = mainModule;
	}
	
}
