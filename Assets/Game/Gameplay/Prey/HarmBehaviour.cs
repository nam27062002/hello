using UnityEngine;
using System.Collections;

public class HarmBehaviour : MonoBehaviour {

	private EdibleBehaviour m_edible;
	public float m_damage;
	private float m_waitTime = 0;

	void Start()
	{
		m_edible = GetComponent<EdibleBehaviour>();
	}

	void Update()
	{
		m_waitTime -= Time.deltaTime;	// Just in case
	}

	void OnTriggerEnter(Collider _other) 
	{
		if ( m_edible != null)
		{
			if (!m_edible.isBeingEaten && _other.tag == "Player" && m_waitTime <= 0) 
			{
				// Harm
				Harm();
				m_waitTime = 0.5f;
				// TODO (miguel): Do some animation
			}
		}
	}

	private void Harm()
	{
		InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage);
	}
}
