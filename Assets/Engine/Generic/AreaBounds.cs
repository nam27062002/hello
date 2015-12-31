using UnityEngine;

public interface AreaBounds {
	void UpdateBounds(Vector3 _center, Vector3 _size);

	Bounds bounds { get; }
	Vector3 center { get; }

	float sizeX { get; }
	float sizeY { get; }

	float extentsX { get; }
	float extentsY { get; }

	Vector3 RandomInside();
	bool Contains(Vector3 _point);
	void DrawGizmo();
}