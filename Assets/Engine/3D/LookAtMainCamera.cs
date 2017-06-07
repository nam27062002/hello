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

	private Transform m_overrideCameraTransform;
	private Transform m_transform;

	// The position where the transform has to look at. It's calculated in LateUpdate() to be sure that the camera has the latest position, but it's applied in Update()
	// instead of in LateUpdate() for performance reasons as a long (2ms) Unity's WaitingForJob task because of ParticleSystems.Update() was found when profiling.
	private Vector3 m_lookAtPosition;

	void Awake() {
		m_transform = transform;
	}

	private void Update() {
		m_transform.LookAt(m_lookAtPosition);
	}

	/// <summary>
	/// Called every frame after all Updates have been called.
	/// </summary>
	private void LateUpdate () 
	{
		Transform targetCamera = GetTargetCameraTransform();
		if(targetCamera != null) {
			m_lookAtPosition = targetCamera.position;
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

			m_overrideCameraTransform = overrideCamera.transform;
		}

		return m_overrideCameraTransform;
	}
}
