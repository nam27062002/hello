using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EggTest : MonoBehaviour {

	[SerializeField] private Animator m_anim = null;

	private int m_state = -1;
	private int m_rarity = -1;

	// Use this for initialization
	void Awake () {
		
	}
	
	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnStateChange() {
		m_state = (m_state + 1) % 4;

		if(m_state == 2) {
			m_rarity = Random.Range(1, 4);
		} else if(m_state == 0) {
			m_rarity = 0;
		}

		m_anim.SetInteger("egg_state", m_state);
		m_anim.SetInteger("egg_rarity", m_rarity);
	}
}
