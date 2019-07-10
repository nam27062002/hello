using UnityEngine;
using System.Collections;

public class HorizontalMotion_v2 : ISpawnable {

	public static float DEFAULT_AMPLITUDE { get { return 2f; } }

	[SerializeField] private float m_amplitude = 2f;
	public float amplitude { set { m_amplitude = value; } get { return m_amplitude; } }

	[SerializeField] private float m_frequency = 1;
	public float frequency { set { m_frequency = value; } get { return m_frequency; } }

	private float m_time;
	private Vector3 m_originalPostion;
	public Vector3 originalPostion { get { return m_originalPostion; } }

	void Start() {
		m_originalPostion = transform.position;
	}


	override public void Spawn(ISpawner _spawner) {
		m_time = 0f;
		m_originalPostion = transform.position;
	}

	override public void CustomUpdate() {}


	// Update is called once per frame
	void FixedUpdate() {

		if (m_frequency > 0) {
			Vector3 position = transform.position;
			position.x = m_originalPostion.x + (Mathf.Sin(m_time / m_frequency) * m_amplitude);
			transform.position = position;

			m_time += Time.fixedDeltaTime;
		}
	}
}