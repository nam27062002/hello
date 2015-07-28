using UnityEngine;
using System.Collections;

public class GrabableBehaviour : MonoBehaviour {


	public delegate void GrabDelegate();
	public GrabDelegate grabDelegate;

	public delegate void ReleaseDelegate(Vector3 momentum);
	public ReleaseDelegate releaseDelegate;

	Transform grabPoint;
	Transform footPoint;
	Quaternion rotation;
	DragonPlayer player;

	float timer = 0f;

	bool grabbed = false;

	// Use this for initialization
	void Start () {

		grabPoint = transform.FindChild("GrabPoint");
		player = GameObject.Find ("Player").GetComponent<DragonPlayer>();
		footPoint = player.transform.FindSubObjectRecursive("leg_fing_R_002");
		rotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (grabbed){
			timer  += Time.deltaTime;
			if (timer < 0.5f){
				transform.position = Vector3.Lerp (transform.position,footPoint.position-(grabPoint.position - transform.position),0.3f);
			}else{
				transform.position = footPoint.position-(grabPoint.position - transform.position);
			}

			transform.rotation = Quaternion.Lerp (transform.rotation,rotation*footPoint.rotation*Quaternion.AngleAxis(90f,Vector3.right),0.2f);
		}else{
		
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

		if (!grabbed){
			if (grabDelegate != null)
				grabDelegate();

			GetComponent<BoxCollider>().enabled = false;
			grabbed = true;
			timer = 0f;
		}
	}

	public void Release(Vector3 momentum){

		if (grabbed){

			if (releaseDelegate != null)
				releaseDelegate(momentum);

			GetComponent<BoxCollider>().enabled = false;

			grabbed = false;
		}
	}
}
