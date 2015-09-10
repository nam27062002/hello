using UnityEngine;

public struct RectAreaBounds : AreaBounds {
	private Bounds m_bounds;

	public RectAreaBounds(Vector3 _center, Vector3 _size) {
		
		m_bounds = new Bounds(_center, _size);
	}

	public Bounds bounds { get { return m_bounds; } }

	public Vector3 randomInside() {
		Vector3 offset = Vector3.zero;
		offset.x = Random.Range(-m_bounds.extents.x, m_bounds.extents.x);
		offset.y = Random.Range(-m_bounds.extents.y, m_bounds.extents.y);
		offset.z = Random.Range(-m_bounds.extents.z, m_bounds.extents.z);

		return m_bounds.center + offset;
	}
}