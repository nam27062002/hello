using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ContainerBehaviour : MonoBehaviour 
{
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
	
	}


	void OnCollisionEnter( Collision collision )
	{
		
		if ( collision.transform.tag == "Player" )
		{
			DragonPlayer player = collision.transform.gameObject.GetComponent<DragonPlayer>();
			// ContainerHit hit = m_hits[ player.data.def.tier ];
			ContainerHit hit = m_hits.Get(player.data.def.tier);
			if ( hit.m_breaksWithoutTurbo )
			{
				Break();
			}
			else
			{
				DragonBoostBehaviour boost = collision.transform.gameObject.GetComponent<DragonBoostBehaviour>();	
				if ( boost.IsBoostActive() )
				{
					hit.m_numHits--;
					if ( hit.m_numHits <= 0 )
						Break();
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
