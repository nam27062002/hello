using UnityEngine;
using System.Collections;

public abstract class Steering : MonoBehaviour {
	
	protected PreyBehaviour m_prey;
	
	void Awake() {		
		m_prey = GetComponent<PreyBehaviour>();
	}
}
