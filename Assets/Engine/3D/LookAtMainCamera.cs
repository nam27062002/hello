using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LookAtMainCamera : MonoBehaviour {

	void LateUpdate () 
	{
		if (Application.isPlaying) {
			if (Camera.main != null)
				transform.LookAt(Camera.main.transform.position);
		} else {
			if (Camera.current != null)
				transform.LookAt(Camera.current.transform.position);
		}
	}
}
