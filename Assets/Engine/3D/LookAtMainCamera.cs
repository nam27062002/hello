using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LookAtMainCamera : MonoBehaviour {

	// Optionally define a camera to look at instead of the main
	[SerializeField] private Camera m_overrideCamera = null;
	public Camera overrideCamera {
		get { return m_overrideCamera; }
		set { m_overrideCamera = value; m_overrideCameraTransform = null; }
	}

	private Transform m_overrideCameraTransform;
	private Transform m_transform;	

	void Awake() {
		m_transform = transform;
	}

	private void Update() {        
        Transform targetCamera = GetTargetCameraTransform();
        if (targetCamera != null) {            
            // This is not done in LateUpdate() because rotating a game object that is the parent of a particle system prevents this
            // stuff from being parallelized with particle system updates introducing a Unity WaitingForJob task that takes 1-2ms. 
            // This camera position might be not the latest one but we consider that the magnitude of the error is negligible
            m_transform.LookAt(targetCamera.position);
        }        
	}	

	/// <summary>
	/// Check all conditions and define the target camera to look at.
	/// </summary>
	/// <returns>The camera to look at.</returns>
	private Transform GetTargetCameraTransform() {
		if(m_overrideCameraTransform == null) {
			if(m_overrideCamera == null) {
				m_overrideCamera = (Application.isPlaying) ? Camera.main : Camera.current;				
			}

			if (m_overrideCamera != null) {
				m_overrideCameraTransform = overrideCamera.transform;
			}
		}

		return m_overrideCameraTransform;
	}
}
