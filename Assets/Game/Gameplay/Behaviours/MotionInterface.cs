using UnityEngine;

public interface MotionInterface {
	Vector3 position 	{ get; set; }
	Vector3 direction 	{ get; }
	Vector3 velocity 	{ get; }
	Vector3 angularVelocity { get; }
}
