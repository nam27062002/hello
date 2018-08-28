using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
	using UnityEngine.XR.iOS;
#else
	using GoogleARCore;
#endif

public class ARKitCameraScaler : MonoBehaviour
{
	private GameObject m_ScaledContent = null;
	public Vector3 m_ScaledObjectOrigin;
	public float m_CameraScale = 1.0f;

	private Camera[] m_kCameras = null;

	public void SetScaledContent (GameObject kObject)
	{
		m_ScaledContent = kObject;

		m_kCameras = m_ScaledContent.transform.GetComponentsInChildren<Camera> ();
	}

	void Start ()
	{
		ARKitContentScaleManager.ContentScaleChangedEvent += ContentScaleChanged;
	}

	void ContentScaleChanged (float scale, float prevScale)
	{
		m_CameraScale = scale;
	}

	public void Update ()
	{
		if (m_ScaledContent != null && m_CameraScale > 0.0001f && m_CameraScale < 10000.0f)
		{

	#if (UNITY_IOS || UNITY_EDITOR_OSX)
			Matrix4x4 matrix = UnityARSessionNativeInterface.GetARSessionNativeInterface().GetCameraPose();

			float fInvScale = 1.0f / m_CameraScale;
			Vector3 kCameraPos = UnityARMatrixOps.GetPosition (matrix);
			Vector3 kVecAnchorToCamera = kCameraPos - m_ScaledObjectOrigin;
			m_ScaledContent.transform.localPosition = m_ScaledObjectOrigin + (kVecAnchorToCamera * fInvScale);
			m_ScaledContent.transform.localRotation = UnityARMatrixOps.GetRotation (matrix);

			if (m_kCameras != null && m_kCameras.Length > 0)
			{
				for (int i = 0; i < m_kCameras.Length; ++i)
				{
					m_kCameras[i].projectionMatrix = UnityARSessionNativeInterface.GetARSessionNativeInterface ().GetCameraProjection ();
				}
			}
	#else
		
		#if ARCORE_SDK_ENABLED
			Pose kCameraPose = Frame.Pose;

			float fInvScale = 1.0f / m_CameraScale;
			Vector3 kCameraPos = kCameraPose.position;
			Vector3 kVecAnchorToCamera = kCameraPos - m_ScaledObjectOrigin;
			m_ScaledContent.transform.localPosition = m_ScaledObjectOrigin + (kVecAnchorToCamera * fInvScale);
			m_ScaledContent.transform.localRotation = kCameraPose.rotation;

			if (m_kCameras != null && m_kCameras.Length > 0)
			{
				for (int i = 0; i < m_kCameras.Length; ++i)
				{
					m_kCameras[i].projectionMatrix = Frame.CameraImage.GetCameraProjectionMatrix (ARKitCameraNearFar.currentNearZ, ARKitCameraNearFar.currentFarZ);
				}
			}
		#endif
		
	#endif
		}
	}

}
