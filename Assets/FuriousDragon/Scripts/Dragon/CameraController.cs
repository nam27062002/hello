using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float movementSmoothing = 0.15f;
	public float forwardOffset = 300f;
	public float[] limitX = new float[2];

	Transform target;
	Vector3 playerDirection = Vector3.right;
	Vector3 forward = Vector3.right;

	float shakeTimer = 0f;
	float shakeAmt = 10f;

	// Zoom
	float defaultZ;
	Interpolator zInterpolator;

	// Use this for initialization
	void Start () {
		target = GameObject.Find("Player").transform;
		defaultZ = transform.position.z;
		zInterpolator = new Interpolator();
	}
	
	// Update is called once per frame
	void Update () {

		forward = forward*0.95f + playerDirection*0.05f;
		Vector3 pos = Vector3.Lerp (transform.position, target.position+forward*forwardOffset,movementSmoothing);

		if (pos.x < limitX[0])
			pos.x = limitX[0];

		if (pos.x > limitX[1])
			pos.x = limitX[1];

		if (shakeTimer > 0f){
			shakeTimer -= Time.deltaTime;
			pos.y += Random.value*shakeAmt*2 - shakeAmt;
		}

		// Z is defined by the zoom
		if(zInterpolator.IsFinished()) {
			pos.z = transform.position.z;	// Don't move
		} else {
			pos.z = zInterpolator.GetExponential();
		}

		transform.position = pos;
	}

	/// <summary>
	/// Zoom to a specific offset from camera's default zoom.
	/// </summary>
	/// <param name="_fOffset">The offset from the default camera's zoom level.</param>
	/// <param name="_fDuration">The duration in seconds of the zoom animation.</param>
	public void Zoom(float _fOffset, float _fDuration) {
		// Override any previous zoom anim
		// Compute target Z adding offset to default position
		float targetZ = defaultZ + _fOffset;

		// Restart interpolator
		zInterpolator.Start(transform.position.z, targetZ, _fDuration);
	}

	public void Shake(){
		shakeTimer = 0.25f;
	}

	public void SetPlayerDirection(Vector3 direction){
		playerDirection = direction;
	}

}
