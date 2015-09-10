using UnityEngine;

public struct CircleAreaBounds : AreaBounds {
	private Bounds m_bounds;

	public CircleAreaBounds(Vector3 _center, float _radius) {
	
		m_bounds = new Bounds(_center, new Vector3(_radius * 2f, _radius * 2f, 0f));
	}

	public Bounds bounds { get { return m_bounds; } }

	public Vector3 randomInside() {
		Vector2 offset = Random.insideUnitCircle * m_bounds.extents.x;
		return m_bounds.center + (Vector3)offset;
	}
}