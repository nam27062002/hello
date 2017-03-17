using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ContainerBehaviour : MonoBehaviour 
{
	//-----------------------------------------------
	// Classes
	//-----------------------------------------------
	[Serializable]
	public class ContainerHit
	{
		public int m_numHits;
		public bool m_breaksWithoutTurbo;
	}
	[Serializable]
	public class SerializableInstance : SerializableDictionary<DragonTier, ContainerHit>
	{

	}

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	private float m_waitTimer = 0;
	[SerializeField]
	public SerializableInstance m_hits = new SerializableInstance();
	public string m_onBreakParticle;
	private ContainerHit m_currentHits;
	private DragonTier m_tier;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	IEnumerator Start () 
	{
		if ( Application.isPlaying )
		{
			while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
			{
				yield return null;
			}
			DragonPlayer player = InstanceManager.player.GetComponent<DragonPlayer>();
			m_tier = player.GetTierWhenBreaking();
			m_currentHits = new ContainerHit();
			ResetHits();
		}
	}

	public void Reset()
	{
		ResetHits();
		m_waitTimer = 0;
		gameObject.SetActive( true );
	}

	void ResetHits()
	{
		ContainerHit originalHits = m_hits.Get( m_tier );
		m_currentHits.m_numHits = originalHits.m_numHits;
		m_currentHits.m_breaksWithoutTurbo = originalHits.m_breaksWithoutTurbo;
	}

	// Update is called once per frame
	void Update () {
		m_waitTimer -= Time.deltaTime;
	}


	void OnCollisionEnter( Collision collision )
	{
		if ( collision.transform.CompareTag("Player") )
		{
			if ( m_currentHits.m_breaksWithoutTurbo )
			{
				Break();
			}
			else if (m_waitTimer <= 0)
			{
				GameObject go = collision.transform.gameObject;
				DragonBoostBehaviour boost = go.GetComponent<DragonBoostBehaviour>();	
				if ( boost.IsBoostActive() )
				{
					DragonMotion dragonMotion = go.GetComponent<DragonMotion>();	// Check speed is enough
					if (dragonMotion.lastSpeed >= (dragonMotion.absoluteMaxSpeed * 0.85f) )
					{
						m_waitTimer = 0.5f;
						// Check Min Speed
						m_currentHits.m_numHits--;
						if ( m_currentHits.m_numHits <= 0 )
							Break();
					}
				}
			}
		}
	}

	void Break()
	{
		// Spawn particle
		GameObject prefab = Resources.Load("Particles/" + m_onBreakParticle) as GameObject;
		if ( prefab != null )
		{
			GameObject go = Instantiate( prefab ) as GameObject;
			if ( go != null )
			{
				go.transform.position = transform.position;
				go.transform.rotation = transform.rotation;
			}
		}

		// Destroy
		gameObject.SetActive( false );

		// TODO: Tell inside edible entities thet they can be eaten
	}

}
