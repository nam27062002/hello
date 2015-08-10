using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float movementSmoothing = 0.15f;
	public float forwardOffset = 300f;
	public float m_MaxZoom = -500f;
	public float[] limitX = new float[2];

	Vector3 targetPosition;

	Transform player;
	Transform dangerousEntity;

	Vector3 playerDirection = Vector3.right;
	Vector3 forward = Vector3.right;

	float shakeTimer = 0f;
	float shakeAmt = 10f;

	// Zoom
	float defaultZ;
	Interpolator zInterpolator;

	// Properties
	public float currentZoom {
		get { return transform.position.z - defaultZ; }
	}

	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player").transform;
		dangerousEntity = null;

		targetPosition = player.transform.position;

		defaultZ = transform.position.z;
		zInterpolator = new Interpolator();
	}
	
	// Update is called once per frame
	void Update () {

		forward = forward*0.95f + playerDirection*0.05f;

		if (dangerousEntity == null) {
			targetPosition = Vector3.Lerp(targetPosition, player.position, movementSmoothing * 1.5f);
		} else {
			targetPosition = Vector3.Lerp(targetPosition, (player.position + dangerousEntity.position) * 0.5f, movementSmoothing * 0.5f);
		}

		Vector3 pos = Vector3.Lerp (transform.position, targetPosition+forward*forwardOffset,movementSmoothing);

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
	/// <param name="_fOffset">The offset from the default camera's zoom level in world units.</param>
	/// <param name="_fDuration">The duration in seconds of the zoom animation.</param>
	public void Zoom(float _fOffset, float _fDuration) {
		// Override any previous zoom anim
		// Compute target Z adding offset to default position
		float targetZ = defaultZ + _fOffset;

		// Restart interpolator
		zInterpolator.Start(transform.position.z, targetZ, _fDuration);
	}

	/// <summary>
	/// Zoom to aspecific offset from camera's default zoom using speed rather than a fixed duration.
	/// </summary>
	/// <param name="_fOffset">The offset from the default camera's zoom level in world units.</param>
	/// <param name="_fSpeed">The speed of the zoom animation in world units per second.</param>
	public void ZoomAtSpeed(float _fOffset, float _fSpeed) {
		// Compute the actual distance to go
		float dist = _fOffset - currentZoom;

		// Compute the time required to go that distance at the given speed
		float duration = Mathf.Abs(dist)/_fSpeed;

		// Launch the zoom animation
		Zoom(_fOffset, duration);
	}

	public void Shake(){
		shakeTimer = 0.25f;
	}

	public void SetPlayerDirection(Vector3 direction){
		playerDirection = direction;
	}


	public void SetDangerousEntity(Transform _entity) {

		if (_entity == null) {
			Zoom(0, 0.5f);
		} else {
			Zoom(m_MaxZoom - currentZoom, 1f);
		}

		dangerousEntity = _entity;
	}
}
