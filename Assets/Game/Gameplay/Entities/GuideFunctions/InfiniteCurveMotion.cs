using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteCurveMotion : MonoBehaviour {

	[SerializeField] private float m_a = 1f;
	[SerializeField] private float m_timeScale = 1f;

	private Transform m_transform;
	private Vector3 m_position;

	private float m_time;

	// Use this for initialization
	void Start() {
		m_transform = transform;
		m_position = m_transform.localPosition;
	}

	void OnEnable() {
		m_time = 0f;
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		float cosT = Mathf.Cos(m_time);
		float sinT = Mathf.Sin(m_time);
		float sin2T = sinT * sinT;

		float aSqrt2 = m_a * 1.1414f; //a * Sqrt(2f)

		Vector3 offset = Vector3.zero;
		offset.x = (aSqrt2 * cosT) / (sin2T + 1f);
		offset.y = (aSqrt2 * cosT * sinT) / (sin2T + 1f);

		m_transform.localPosition = m_position + offset;

		m_time += Time.fixedDeltaTime * m_timeScale;
	}
}
