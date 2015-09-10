using UnityEngine;

public interface AreaBounds {
	Bounds bounds { get; }
	Vector3 randomInside();
}
