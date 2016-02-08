using UnityEngine;
using System.Collections;

public class BreakableBehaviour : MonoBehaviour 
{
	public DragonTier m_tierWithTurboBreak = 0;
	public DragonTier m_tierNoTurboBreak = 0;

	public string m_onBreakParticle;

	void OnCollisionEnter( Collision collision )
	{
		if ( collision.transform.tag == "Player" )
		{
			DragonPlayer player = collision.transform.gameObject.GetComponent<DragonPlayer>();
			if (player.data.def.tier >= m_tierNoTurboBreak)
			{
				Break();
			}
			else
			{
				DragonBoostBehaviour boost = collision.transform.gameObject.GetComponent<DragonBoostBehaviour>();	
				if ( boost.IsBoostActive() && player.data.def.tier >= m_tierWithTurboBreak )
				{
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
	}

	
}
