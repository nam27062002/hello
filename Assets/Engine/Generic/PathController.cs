using UnityEngine;
using System.Collections.Generic;

public class PathController : MonoBehaviour {

	[SerializeField] private List<Vector3> m_points = new List<Vector3>();
	public List<Vector3> points { get { return m_points; } set { m_points = value; } }
	
	[SerializeField] private float m_smoothRadius = 0f;
	public float radius { get { return m_smoothRadius; } }
	
	private int m_index;
	private int m_direction;
	
	private RectAreaBounds m_bounds = new RectAreaBounds(Vector3.zero, Vector3.zero);
	public RectAreaBounds bounds { get { UpdateBounds(); return m_bounds; } }

	void Start() {
		m_index = 0;
		m_direction = 1;
	}

	private void UpdateBounds() {
		Vector3 min = Vector3.zero;
		Vector3 max = Vector3.zero;

		if (m_points.Count >= 2f) {
			min = m_points[0];
			max = m_points[0];

			for (int i = 1; i < m_points.Count; i++) {
				Vector3 point = m_points[i];

				if (min.x > point.x) min.x = point.x;
				if (min.y > point.y) min.y = point.y;
				if (min.z > point.z) min.z = point.z;

				if (max.x < point.x) max.x = point.x;
				if (max.y < point.y) max.y = point.y;
				if (max.z < point.z) max.z = point.z;
			}
		}
		
		m_bounds.SetMinMax(min + transform.position - Vector3.one * radius, max + transform.position + Vector3.one * radius);
	}
	
	public Vector3 GetRandom() {
		m_index = Random.Range(0, m_points.Count);
		return GetNext();
	}

	public Vector3 GetNearestTo(Vector3 _point) {
		float minD = 999999f;

		m_index = 0;
		for (int i = 0; i < m_points.Count; i++) {
			float d = (m_points[i] - _point).sqrMagnitude;
			if (d < minD) {
				minD = d;
				m_index = i;
			}
		}

		return GetNext();
	}

	public Vector3 GetNext() {
		int current = m_index;
		m_index = (m_index + m_direction) % m_points.Count;
		return m_points[current] + transform.position;
	}

	public void ChangeDirection() {
		m_direction *= -1;
	}
}
