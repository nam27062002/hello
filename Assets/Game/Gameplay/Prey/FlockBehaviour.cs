using UnityEngine;
using System.Collections;

public class FlockBehaviour : MonoBehaviour {



	// --------------------------------------------------------------- //
	private FlockController m_flock; // turn into flock controller

	private PreyMotion m_motion;
	private SpawnBehaviour m_spawn;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {
		m_motion = GetComponent<PreyMotion>();
		m_spawn = GetComponent<SpawnBehaviour>();
	}

	public Vector2 GetFlockTarget() {
		if (m_spawn != null) {
			return m_flock.GetTarget(m_spawn.index);
		} else {
			return m_flock.GetTarget(0);
		}
	}
}
