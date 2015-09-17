using UnityEngine;
using System.Collections;

public class VerticalMotion : MonoBehaviour {

	public static float DEFAULT_AMPLITUDE { get { return 100f; } }

	[SerializeField] private float m_amplitude = 100f;
	public float amplitude { set { m_amplitude = value; } get { return m_amplitude; } }

	[SerializeField] private float m_frequency = 1;
	public float frequency { set { m_frequency = value; } get { return m_frequency; } }

	private float m_time;
	private Vector3 m_originalPostion;

	void Start() {

		m_originalPostion = transform.position;
	}

	// Update is called once per frame
	void Update() {
	
		if (m_frequency > 0) {
			m_time += Time.deltaTime;

			Vector3 position = transform.position;
			position.y = m_originalPostion.y + (Mathf.Cos(m_time / m_frequency) * m_amplitude);
			transform.position = position;
		}
	}
}
