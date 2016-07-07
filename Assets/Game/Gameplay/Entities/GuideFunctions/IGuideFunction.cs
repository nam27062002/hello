using UnityEngine;

public interface IGuideFunction {
	Bounds GetBounds();
	void ResetTime();
	Vector3 NextPositionAtSpeed(float _speed);
}
