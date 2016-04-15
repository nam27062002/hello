using UnityEngine;

public abstract class Orientation : MonoBehaviour {
	public abstract void SetRotation(Quaternion _rotation);
	public abstract void SetDirection(Vector3 direction);
}
