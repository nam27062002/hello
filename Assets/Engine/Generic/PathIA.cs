using UnityEngine;
using System.Collections.Generic;

public class PathIA : MonoBehaviour {

	[SerializeField] private List<Vector3> m_points = new List<Vector3>();
	public List<Vector3> points { get { return m_points; } set { m_points = value; } }

	private int m_index;
	private int m_direction;

	private Bounds m_bounds = new Bounds();
	public Bounds bounds { get { UpdateBounds(); return m_bounds; } }

	void Start() {
		m_index = 0;
		m_direction = 1;
	}

	private void UpdateBounds() {
		Vector3 min = Vector3.zero;
		Vector3 max = Vector3.zero;

		if (m_points.Count >= 2) {
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
		
		m_bounds.SetMinMax(min + transform.position, max + transform.position);
	}

	public Vector3 GetNext() {
		int current = m_index;
		m_index = (m_index + m_direction) % m_points.Count;
		return m_points[m_index] + transform.position;
	}

	public void ChangeDirection() {
		m_direction *= -1;
	}
}
