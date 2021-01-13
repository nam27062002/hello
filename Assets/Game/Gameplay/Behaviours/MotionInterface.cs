using UnityEngine;

public interface IMotion {
	Quaternion orientation 	{ get; set; }
	Vector3 position 		{ get; set; }
	Vector3 direction 		{ get; }
	Vector3 groundDirection { get; }
	Vector3 velocity 		{ get; }
	Vector3 angularVelocity { get; }
}
