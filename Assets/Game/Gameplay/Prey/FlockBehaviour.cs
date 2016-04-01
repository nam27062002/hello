using UnityEngine;
using System.Collections;

public class FlockBehaviour : MonoBehaviour {

	//---------------------------------------------------------------
	// Attributes
	//---------------------------------------------------------------
	[SerializeField] private Range m_flockAvoidRadiusRange;
	private float m_flockAvoidRadiusSqr;


	// --------------------------------------------------------------- //
	private FlockController m_flock; // turn into flock controller
	private float m_flockAvoidRadius;

	private PreyMotion m_motion;
	private SpawnBehaviour m_spawn;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {
		m_motion = GetComponent<PreyMotion>();
		m_spawn = GetComponent<SpawnBehaviour>();
	}

	void OnEnable() {		
		m_flockAvoidRadius = m_flockAvoidRadiusRange.GetRandom();
		m_flockAvoidRadiusSqr = m_flockAvoidRadius * m_flockAvoidRadius;
	}

	void OnDisable() {
		AttachFlock(null);
	}

	public void AttachFlock(FlockController _flock) {		
		if (m_flock != null) {
			m_flock.Remove(gameObject);
		}

		m_flock = _flock;

		if (m_flock != null) {
			m_flock.Add(gameObject);
		}
	}

	public Vector2 GetFlockTarget() {
		if (m_spawn != null) {
			return m_flock.GetTarget(m_spawn.index);
		} else {
			return m_flock.GetTarget(0);
		}
	}
	
	void Update () {
		Vector2 avoid = Vector2.zero;
		Vector2 direction = Vector2.zero;

		for (int i = 0; i < m_flock.entities.Length; i++) {			
			GameObject entity = m_flock.entities[i];

			if (entity != null && entity != gameObject) {
				direction = m_motion.position - (Vector2)entity.transform.position;
				float distanceSqr = direction.sqrMagnitude;

				if (distanceSqr < m_flockAvoidRadiusSqr) {
					float distance = distanceSqr * m_flockAvoidRadius / m_flockAvoidRadiusSqr;
					avoid += direction.normalized * (m_flockAvoidRadius - distance);
				}
			}
		}

		m_motion.FlockSeparation(avoid);
	}
}
