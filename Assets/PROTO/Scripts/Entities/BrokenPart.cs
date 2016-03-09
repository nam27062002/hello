using UnityEngine;
using System.Collections;

public class BrokenPart : MonoBehaviour {


		public float launchForce = 10f;
		
		bool breaking = false;
		float timer = 0f;
		Rigidbody rbody;
		
		Vector3 originalPosition;
		Quaternion originalRotation;
		bool initialized = false;

		Vector3 pos;
		Vector3 force;
		Vector3 torque;
		
		bool customPhysics = false;

		void Start(){
			Init ();
		}

		void Init(){
			originalPosition = transform.position;
			originalRotation = transform.rotation;
			GetComponent<BoxCollider>().enabled = false;
			initialized = true;
		}
	
		
		
		public void Break(Vector3 objectCenter){
			
			if (!initialized)
				Init();


			if (!breaking){

				Vector3 dir = ((originalPosition -objectCenter).normalized + (new Vector3(0f,Random.Range (0f,1f),0f))).normalized;
				rbody = GetComponent<Rigidbody>();
				GetComponent<BoxCollider>().enabled = true;
				rbody.isKinematic = false;
				rbody.AddForce(dir.normalized*launchForce*100f);
				rbody.AddTorque(new Vector3(Random.value*1000f,Random.value*1000f,Random.value*1000f));
				
				breaking = true;
			}
		}

		public void OnSpawn(){

			if (initialized){
				transform.position = originalPosition;
				transform.rotation = originalRotation;
				GetComponent<Rigidbody>().isKinematic = true;
				GetComponent<BoxCollider>().enabled = false;
				GetComponent<MeshRenderer>().enabled = false;
				breaking = false;
			}
		}

}
