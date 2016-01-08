using UnityEngine;
using System.Collections;

public class RectArea2D : MonoBehaviour, Area {

	public Vector2 offset;
	public Vector2 size = new Vector2(50f, 50f);

	public Color color = new Color(0.76f, 0.23f, 0.13f, 0.2f);

	private RectAreaBounds m_bounds;

	public AreaBounds bounds {
		get {
			if (m_bounds == null) {
				m_bounds = new RectAreaBounds(center, size);
			}

			return m_bounds;
		}
	}

	public Vector3 center { get { return transform.position + (Vector3)offset; } }

	public float width  { get { return size.x; } }
	public float height { get { return size.y; } }
}
