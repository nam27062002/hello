using UnityEngine;
using System.Collections;

public class VerticalFall : Initializable {

	public static float DEFAULT_AMPLITUDE { get { return 20f; } }

	[SerializeField] private float m_amplitude = 20f;
	public float amplitude { set { m_amplitude = value; } get { return m_amplitude; } }

	[SerializeField] private float m_frequency = 1;
	public float frequency { set { m_frequency = value; } get { return m_frequency; } }

	private float m_prevPositiony;
	private float m_time;
	private Vector3 m_originalPostion;
	public Vector3 originalPostion { get { return m_originalPostion; } }

	void Start() {
		m_originalPostion = transform.position;
		m_prevPositiony = m_originalPostion.y;
	}


	public override void Initialize() {		
		m_originalPostion = transform.position;
		m_prevPositiony = m_originalPostion.y;
	}


	// Update is called once per frame
	void Update() {

		if (m_frequency > 0) {
			m_time += Time.deltaTime;

			Vector3 position = transform.position;
			//position.x = m_originalPostion.x + (Mathf.Cos(m_time / m_frequency) * m_amplitude);
			position.y = position.y - (m_frequency * Time.deltaTime);
			transform.position = position;
			m_prevPositiony = position.y;
		}
	}
}