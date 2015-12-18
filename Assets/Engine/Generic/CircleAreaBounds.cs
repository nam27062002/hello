using UnityEngine;

public struct CircleAreaBounds : AreaBounds {
	private Bounds m_bounds;

	public CircleAreaBounds(Vector3 _center, float _radius) {
	
		m_bounds = new Bounds(_center, new Vector3(_radius * 2f, _radius * 2f, _radius * 2f));
	}

	public Bounds bounds { get { return m_bounds; } }

	public Vector3 RandomInside() {
		Vector2 offset = Random.insideUnitCircle * m_bounds.extents.x;
		return m_bounds.center + (Vector3)offset;
	}

	public bool Contains(Vector3 _point) {
		_point.z = m_bounds.center.z;
		return m_bounds.Contains(_point);
	}

	public void DrawGizmo() {
		Color color = Color.yellow;
		color.a = 0.25f;

		Gizmos.color = color;
		Gizmos.DrawSphere(m_bounds.center, Mathf.Max(0.5f, m_bounds.extents.x));
	}
}