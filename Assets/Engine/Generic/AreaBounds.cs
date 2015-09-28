using UnityEngine;

public interface AreaBounds {
	Bounds bounds { get; }
	Vector3 RandomInside();
	bool Contains(Vector3 _point);
	void DrawGizmo();
}
