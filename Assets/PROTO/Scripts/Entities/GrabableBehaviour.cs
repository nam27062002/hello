using UnityEngine;
using System.Collections;

public class GrabableBehaviour : MonoBehaviour {

	public float weight = 1.5f;
	public bool limitRotation  = false;

	public delegate void GrabDelegate();
	public GrabDelegate grabDelegate;

	public delegate void ReleaseDelegate(Vector3 momentum);
	public ReleaseDelegate releaseDelegate;

	Transform grabPoint;
	Transform footPoint;
	Quaternion rotation;
	DragonGrabBehaviour player;

	float timer = 0f;

	enum State{
		IDLE,
		GRABBED,
		RELEASED
	}

	State state = State.IDLE;

	// Use this for initialization
	void Start () {

		grabPoint = transform.FindChild("GrabPoint");
		player = GameObject.Find ("Player").GetComponent<DragonGrabBehaviour>();
		footPoint = player.transform.FindSubObjectRecursive("leg_fing_R_002");
		rotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (state == State.GRABBED){
			timer  += Time.deltaTime;
			if (timer < 0.5f){
				transform.position = Vector3.Lerp (transform.position,footPoint.position-(grabPoint.position - transform.position),0.3f);
			}else{
				transform.position = footPoint.position-(grabPoint.position - transform.position);
			}

			if (!limitRotation)
				transform.rotation = Quaternion.Lerp (transform.rotation,rotation*footPoint.rotation*Quaternion.AngleAxis(90f,Vector3.right),0.2f);
			else{
			
			}

		
		}else if (state == State.IDLE){
		
			Vector3 p = footPoint.position;
			p.z = 0f;
			Vector3 p2 = grabPoint.position;
			p2.z = 0f;
			if ((p-p2).sqrMagnitude < 10000){
				player.Grab(this);
			}
		}
	}

	public void Grab(){

		if (state == State.IDLE){
			if (grabDelegate != null)
				grabDelegate();

			GetComponent<BoxCollider>().enabled = false;
			state = State.GRABBED;
			timer = 0f;
		}
	}

	public void Release(Vector3 momentum){

		if (state == State.GRABBED){

			if (releaseDelegate != null)
				releaseDelegate(momentum);

			GetComponent<BoxCollider>().enabled = false;

			state = State.RELEASED;
		}
	}

	public void ResetGrab(){
		GetComponent<BoxCollider>().enabled = true;
		state = State.IDLE;
	}
}
