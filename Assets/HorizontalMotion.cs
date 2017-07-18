using UnityEngine;
using System.Collections;

public class HorizontalMotion : Initializable {

	public static float DEFAULT_AMPLITUDE { get { return 20f; } }

	[SerializeField] private float m_amplitude = 20f;
	public float amplitude { set { m_amplitude = value; } get { return m_amplitude; } }

	[SerializeField] private float m_frequency = 1;
	public float frequency { set { m_frequency = value; } get { return m_frequency; } }

	private bool m_activateMovement;
	private float m_prevPositiony;
	private Vector3 m_originalPostion;
	public Vector3 originalPostion { get { return m_originalPostion; } }

	void Start() {
		m_originalPostion = transform.position;
		m_prevPositiony = m_originalPostion.y;
		m_activateMovement = false;
	}


	public override void Initialize() {		
		m_originalPostion = transform.position;
		m_prevPositiony = m_originalPostion.y;
		m_activateMovement = false;
	}


	// Update is called once per frame
	void Update() {

		if (m_frequency > 0) {
			Vector3 position = transform.position;
						if (m_activateMovement || m_prevPositiony == position.y) {
				position.x = position.x + (12f) * Time.deltaTime;
				m_activateMovement = true;
			}
			transform.position = position;
			m_prevPositiony = position.y;
		}
	}
}