using UnityEngine;
using System.Collections;

public class LookAtMainCamera : MonoBehaviour {

	void LateUpdate () 
	{
		transform.LookAt(Camera.main.transform.position);
	}
}
