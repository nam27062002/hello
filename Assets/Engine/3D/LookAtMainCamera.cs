using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LookAtMainCamera : MonoBehaviour {
	// Optionally define a camera to look at instead of the main
	[SerializeField] private Camera m_overrideCamera = null;
	public Camera overrideCamera {
		get { return m_overrideCamera; }
		set { m_overrideCamera = value; }
	}

	/// <summary>
	/// Called every frame after all Updates have been called.
	/// </summary>
	private void LateUpdate () 
	{
		Camera targetCamera = GetTargetCamera();
		if(targetCamera != null) {
			transform.LookAt(targetCamera.transform.position);
		}
	}

	/// <summary>
	/// Check all conditions and define the target camera to look at.
	/// </summary>
	/// <returns>The camera to look at.</returns>
	private Camera GetTargetCamera() {
		if(m_overrideCamera != null) {
			return m_overrideCamera;
		} else if(Application.isPlaying) {
			return Camera.main;
		} else {
			return Camera.current;
		}
		return null;
	}
}
