using UnityEngine;
using System.Collections;

public class BreakableBehaviour : MonoBehaviour 
{
	public DragonTier m_tierWithTurboBreak = 0;
	public DragonTier m_tierNoTurboBreak = 0;

	public string m_onBreakParticle;
	public string m_onBreakAudio;

	public float m_maxShakeForce = 1;
	public float m_maxShakeDuration = 1;
	public float m_maxPushForce = 2;
	private float m_shakeTimer = 0;
	private float m_shakeDuration = 0;
	private float m_shakeForce = 0;

	public Transform m_view;

	void Start()
	{
		if ( m_view == null )
			transform.FindChild("view");
	}

	void OnCollisionEnter( Collision collision )
	{
		if ( collision.transform.CompareTag("Player") )
		{
			DragonPlayer player = collision.transform.gameObject.GetComponent<DragonPlayer>();
			DragonTier tier = player.GetTierWhenBreaking();
			float value = Mathf.Max(0.1f, Vector3.Dot( collision.contacts[0].normal, player.dragonMotion.direction));
			if (tier >= m_tierNoTurboBreak)
			{
				Break( Vector3.zero );
			}
			else if (tier >= m_tierWithTurboBreak)
			{
				DragonBoostBehaviour boost = collision.transform.gameObject.GetComponent<DragonBoostBehaviour>();	
				if ( boost.IsBoostActive())
				{
					Break(-collision.contacts[0].normal * value);
				}
				else
				{
					// Message : You need boost!
					Messenger.Broadcast( GameEvents.BREAK_OBJECT_NEED_TURBO );
					Shake( m_maxShakeForce * value, m_maxShakeDuration * value);
				}
			}
			else
			{
				// Message: You need a bigger dragon
				Messenger.Broadcast( GameEvents.BREAK_OBJECT_BIGGER_DRAGON );	
				value *= 0.5f;
				Shake( m_maxShakeForce * value, m_maxShakeDuration * value);
			}
		}
	}

	void Break( Vector3 pushVector )
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

		if ( pushVector != Vector3.zero )
		{
			DragonMotion dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
			pushVector *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude, 2f));
			dragonMotion.AddForce( pushVector );
		}



		// Destroy
		gameObject.SetActive( false );
		Destroy( gameObject );
	}

	public void Shake( float force, float duration )
	{
		if ( m_shakeTimer <= 0 )
			StartCoroutine("Shaking");
		m_shakeTimer = m_shakeDuration = Mathf.Max( m_shakeTimer, duration);
		m_shakeForce = Mathf.Max( m_shakeForce, force);
	}


	IEnumerator Shaking ()
	{
		yield return null;
		Vector3 startPos = m_view.position;
		while( m_shakeTimer > 0 && m_shakeDuration > 0)
		{
			m_shakeTimer -= Time.deltaTime;	
			m_view.position = startPos + Random.insideUnitSphere * m_shakeForce * (m_shakeTimer / m_shakeDuration);
			yield return null;
		}
		m_shakeForce = 0;
		m_shakeTimer = 0;
		m_shakeDuration = 0;
		m_view.position = startPos;
	}

}
