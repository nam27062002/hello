using UnityEngine;

public class SplineMotion : MonoBehaviour {

	private enum SplineWalkerMode {
		Once,
		Loop,
		PingPong
	}

	[SerializeField] private BSpline.BezierSpline m_spline;
	[SerializeField] private float m_speed = 10f;
	[SerializeField] private SplineWalkerMode m_mode = SplineWalkerMode.Loop;

	private Transform m_transform;

	private float m_acceleration;

	private bool m_goingForward = true;
	private float m_distance;

	private void Start() {
		m_transform = transform;
		m_distance = 0f;
		m_acceleration = 0f;
	}

	private void Update() {
		if (m_goingForward) {
			m_distance += (m_speed + m_acceleration) * Time.deltaTime;
			if (m_distance > m_spline.length) {
				if (m_mode == SplineWalkerMode.Once) {
					m_distance = m_spline.length;
				} else if (m_mode == SplineWalkerMode.Loop) {
					m_distance -= m_spline.length;
				} else {
					m_distance = 2f * m_spline.length - m_distance;
					m_goingForward = false;
				}
			}
		} else {
			m_distance -= (m_speed + m_acceleration) * Time.deltaTime;
			if (m_distance < 0f) {
				m_distance = -m_distance;
				m_goingForward = true;
			}
		}

		Vector3 dir = Vector3.zero;
		Vector3 up = Vector3.zero;
		Vector3 right = Vector3.zero;
		Vector3 position = m_spline.GetPointAtDistance(m_distance, ref dir, ref up, ref right, true, false);

		if (!m_goingForward) {
			dir *= -1;
		}

		if (dir.y < -0.15f) 	m_acceleration = Mathf.MoveTowards(m_acceleration, m_speed * 2f, m_speed * 0.1f * Mathf.Abs(dir.y));
		else if (dir.y > 0.15f)	m_acceleration = Mathf.MoveTowards(m_acceleration, -m_speed * 0.25f, m_speed * 0.05f);
		else 					m_acceleration = Mathf.MoveTowards(m_acceleration, 0f, m_speed * 0.05f);

		m_transform.localPosition = position;
		m_transform.localRotation = Quaternion.LookRotation(dir, up);
	}
}