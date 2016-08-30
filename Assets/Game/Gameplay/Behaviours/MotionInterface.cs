using UnityEngine;

public interface MotionInterface {
	Vector2 position { get; }
	Vector2 direction { get; }
	Vector2 velocity { get; }
	Vector2 angularVelocity { get; }

	float maxSpeed { get; }
}
