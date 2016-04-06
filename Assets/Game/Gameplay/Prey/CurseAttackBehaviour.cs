using UnityEngine;
using System.Collections;

public class CurseAttackBehaviour : MonoBehaviour {
	
	[SerializeField] private float m_damage;
	[SerializeField] private float m_duration;


	private float m_timer;

	// Use this for initialization
	void Start () 
	{
		m_timer = 0;
	}

	void Update() 
	{
		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_timer = 0;
			}
		}
	}

	void OnTriggerStay(Collider _other) 
	{
		if (m_timer <= 0 && _other.tag == "Player") 
		{
			DragonHealthBehaviour dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
			if (dragon != null) 
			{
				dragon.Curse( m_damage, m_duration );
				m_timer = 1.0f;
			}
		}
	}
}
