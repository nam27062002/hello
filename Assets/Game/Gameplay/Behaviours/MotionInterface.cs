using UnityEngine;

public interface MotionInterface {
	Vector2 position { get; }
	Vector2 direction { get; }
	Vector2 velocity { get; }
	float maxSpeed { get; }
}
