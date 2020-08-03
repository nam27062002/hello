using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
using UnityEngine.XR.iOS;
#endif

[RequireComponent(typeof(Camera))]
public class ARKitCameraNearFar : MonoBehaviour {

	private Camera attachedCamera;
	public static float currentNearZ;
	public static float currentFarZ;

	// Use this for initialization
	void Start () {
		attachedCamera = GetComponent<Camera> ();
		UpdateCameraClipPlanes ();
	}

	void UpdateCameraClipPlanes()
	{
		currentNearZ = attachedCamera.nearClipPlane;
		currentFarZ = attachedCamera.farClipPlane;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
		UnityARSessionNativeInterface.GetARSessionNativeInterface ().SetCameraClipPlanes (currentNearZ, currentFarZ);
#endif
	}
	
	// Update is called once per frame
	void Update () {
		if (currentNearZ != attachedCamera.nearClipPlane || currentFarZ != attachedCamera.farClipPlane) {
			UpdateCameraClipPlanes ();
		}
	}
}
