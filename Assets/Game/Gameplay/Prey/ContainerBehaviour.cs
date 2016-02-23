using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ContainerBehaviour : MonoBehaviour 
{
	private float m_waitTimer = 0;
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

	[SerializeField]
	public SerializableInstance m_hits = new SerializableInstance();

	public string m_onBreakParticle;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		m_waitTimer -= Time.deltaTime;
	}


	void OnCollisionEnter( Collision collision )
	{
		
		if ( collision.transform.tag == "Player" )
		{
			GameObject go = collision.transform.gameObject;
			DragonPlayer player = go.GetComponent<DragonPlayer>();
			// ContainerHit hit = m_hits[ player.data.def.tier ];
			ContainerHit hit = m_hits.Get(player.data.def.tier);
			if ( hit.m_breaksWithoutTurbo )
			{
				Break();
			}
			else if (m_waitTimer <= 0)
			{
				DragonBoostBehaviour boost = go.GetComponent<DragonBoostBehaviour>();	
				if ( boost.IsBoostActive() )
				{
					DragonMotion dragonMotion = go.GetComponent<DragonMotion>();	// Check speed is enough

					Debug.Log( dragonMotion.m_lastSpeed );
					Debug.Log( dragonMotion.absoluteMaxSpeed * 0.85f );

					if (dragonMotion.m_lastSpeed >= (dragonMotion.absoluteMaxSpeed * 0.85f) )
					{
						m_waitTimer = 0.5f;
						// Check Min Speed
						Debug.Log("HIT!!!!");
						hit.m_numHits--;
						if ( hit.m_numHits <= 0 )
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
		Destroy( gameObject );

		// TODO: Tell inside edible entities thet they can be eaten
	}

}
