using UnityEngine;
using System.Collections;

public class DragonHealthBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer m_dragon;
	private Animator m_animator;

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------

	// Use this for initialization
	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}
		
	// Update is called once per frame
	void Update() {
		m_dragon.AddLife(-Time.deltaTime * m_dragon.data.healthDrainPerSecond);	
	}

	public bool IsAlive() {
		return m_dragon.IsAlive();
	}

	public void ReceiveDamage(float _value, Transform _source = null) {
		if(enabled) {
			m_animator.SetTrigger("damage");// receive damage?
			m_dragon.AddLife(-_value);
			Messenger.Broadcast<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, _value, _source);
		}
	}

}
