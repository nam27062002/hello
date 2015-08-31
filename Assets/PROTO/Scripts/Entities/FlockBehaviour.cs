using UnityEngine;
using System.Collections;

public class FlockBehaviour : MonoBehaviour {


	Vector3 impulse;
	Vector3 follow;
	Vector3 avoid;
	Vector3 flee;
	Vector3 pos;
	Vector3 scale;
	Quaternion rotation;

	[HideInInspector]
	public FlockController flock;
	public float speed = 300f;
	public bool trackGround = true;
	public float fleeFactor = 1f;
	public bool faceDirection = false;
	public bool useFlee = true;

	GameObject player;
	int frame = 0;
	int groundMask;

	void Start () {

		pos = transform.position;
		scale = transform.localScale;
		rotation = transform.localRotation;
		player = GameObject.Find ("Player");
		groundMask = 1 << LayerMask.NameToLayer("Ground");

	}
	
	void Update () {

		if (flock != null) {

			// Follow the target
			follow = flock.followPos - pos;
			follow.Normalize();
			follow *= speed*Time.deltaTime;

			// Avoid other entities from the same flock
			avoid = Vector3.zero;
			Vector3 dist = Vector3.zero;
			foreach (GameObject obj in flock.m_entities){
				if (obj != this.gameObject){
					dist = pos-obj.transform.position;
					float m = dist.magnitude;
					if (m < 50f)
						avoid += dist.normalized*(50f-m);
				}
			}

			avoid.Normalize();
			avoid *= speed * Time.deltaTime;

			if (useFlee){
				flee = pos - player.transform.position;
				float d = flee.magnitude;
				if (d < speed)
					flee = flee.normalized * (speed - d * 0.5f) * fleeFactor * Time.deltaTime;
				else
					flee = Vector3.zero;

				impulse = Vector3.Lerp (impulse, follow * 0.5f + avoid * 0.4f + flee * 0.75f, 0.2f);
			}else{
				impulse = Vector3.Lerp (impulse, follow * 0.85f + avoid * 0.15f, 0.4f);
			}
			pos += impulse;

			frame++;
			Vector3 dir = impulse.normalized;
			if (trackGround && frame > 2) {

				// Don't go into the ground
				RaycastHit ground;
				if (Physics.Linecast(pos, pos+dir * 50f, out ground, groundMask)) {
					pos = new Vector3(ground.point.x, ground.point.y, 0f) - dir * 50f;
				} else {
					frame = 0;
				}
			}

			pos.z = 0f;
			transform.position = pos;

			if (faceDirection) {
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			} else {
				// Rotate so it faces the right direction (replaces 2D sprite flip)
				float fRotationSpeed = 2f;	// [AOC] Deg/sec?
				float fAngleY = 0f;

				if(impulse.x < 0f) {
					fAngleY = 180;
				}

				Quaternion q = Quaternion.Euler(0, fAngleY, 0);
				transform.localRotation = Quaternion.Slerp(transform.localRotation, q, Time.deltaTime * fRotationSpeed);
			}
		}
	}

	public void OnSpawn(Bounds bounds){

		pos = bounds.center;
		pos.x  += Random.Range (-300f,300f);
		pos.y  += Random.Range (-300f,300f);
		pos.z = 0f;

		transform.position = pos;
		transform.localScale = scale;
		transform.localRotation = rotation;
		impulse = Vector3.zero;

		GetComponent<GameEntity>().RestoreHealth();
	}
}
