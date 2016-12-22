﻿using UnityEngine;
using System.Collections;

public class BreakableBehaviour : MonoBehaviour 
{
	public DragonTier m_tierWithTurboBreak = 0;
	public DragonTier m_tierNoTurboBreak = 0;

	public string m_onBreakParticle;
	public string m_onBreakAudio;

	void OnCollisionEnter( Collision collision )
	{
		if ( collision.transform.CompareTag("Player") )
		{
			DragonPlayer player = collision.transform.gameObject.GetComponent<DragonPlayer>();
			DragonTier tier = player.GetTierWhenBreaking();
			if (tier >= m_tierNoTurboBreak)
			{
				Break();
			}
			else if (tier >= m_tierWithTurboBreak)
			{
				DragonBoostBehaviour boost = collision.transform.gameObject.GetComponent<DragonBoostBehaviour>();	
				if ( boost.IsBoostActive())
				{
					Break();
				}
				else
				{
					// Message : You need boost!
					Messenger.Broadcast( GameEvents.BREAK_OBJECT_NEED_TURBO );
				}
			}
			else
			{
				// Message: You need a bigger dragon
				Messenger.Broadcast( GameEvents.BREAK_OBJECT_BIGGER_DRAGON );	
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

		AudioController.Play( m_onBreakAudio );

		DragonMotion dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
		dragonMotion.NoDamageImpact();

		// Destroy
		gameObject.SetActive( false );
		Destroy( gameObject );
	}

	
}
