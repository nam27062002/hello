using UnityEngine;
using System.Collections;

public class DragonOrientation : MonoBehaviour {


	Vector3 dir;
	Quaternion targetDir;
	int groundMask;
	float angle;
	float timer;

	enum State{

		PLAYING,
		DYING,
		DEAD
	};


	State state = State.PLAYING;

	// Use this for initialization
	void Start () {
	
		targetDir = transform.rotation;
		groundMask = 1 << LayerMask.NameToLayer("Ground");
	}
	
	// Update is called once per frame
	void Update () {
	
		if (state == State.PLAYING){

			transform.rotation = Quaternion.Lerp (transform.rotation, targetDir, 0.12f);

		}else if (state == State.DYING){

			transform.rotation = Quaternion.Lerp (transform.rotation, targetDir, 0.1f);
			targetDir *= Quaternion.AngleAxis(200f*Time.deltaTime, Vector3.down);

			timer += Time.deltaTime;
			if (timer > 3f)
				state = State.DEAD;
		}else{

			transform.rotation = Quaternion.Lerp (transform.rotation, targetDir, 0.1f);
		}
	}


	public void SetDirection(Vector3 direction){

		dir = direction.normalized;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		//RaycastHit ground;
		/*
		if (angle > 0 && Physics.Linecast( transform.position, transform.position+dir*200f, out ground, groundMask)){
			targetDir = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle*0.5f, Vector3.left);
		}else{
			targetDir = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle, Vector3.left);
		}
		*/

		targetDir = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle, Vector3.left);
		Camera.main.GetComponent<CameraController_OLD>().SetPlayerDirection(dir);
	}

	public void OnDeath(){
		targetDir = Quaternion.AngleAxis(0f, Vector3.forward)*Quaternion.AngleAxis(0f, Vector3.left);
		state = State.DYING;
		timer = 0f;
	}
}
