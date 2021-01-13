using UnityEngine;

public interface IGuideFunction {
	AreaBounds GetBounds();
	void ResetTime();
	Vector3 NextPositionAtSpeed(float _speed);
}
