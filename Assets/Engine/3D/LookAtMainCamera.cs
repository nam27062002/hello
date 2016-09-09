using UnityEngine;
using System.Collections;

public class LookAtMainCamera : MonoBehaviour {

	void LateUpdate () 
	{
		if ( Camera.main != null )
			transform.LookAt(Camera.main.transform.position);
	}
}
