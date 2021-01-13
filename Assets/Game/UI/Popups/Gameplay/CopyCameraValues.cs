using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CopyCameraValues : MonoBehaviour {

	public Camera m_cameraToCopy;
	private Camera m_camera;
	void Awake()
	{
		m_camera = GetComponent<Camera>();
	}

	// Update is called once per frame
	void LateUpdate () {
		m_camera.enabled = m_cameraToCopy.enabled;
		m_camera.aspect = m_cameraToCopy.aspect;
		m_camera.pixelRect = m_cameraToCopy.pixelRect;
		if ( m_cameraToCopy.orthographic )
		{
			m_camera.orthographic = true;
			m_camera.orthographicSize = m_cameraToCopy.orthographicSize;
		}
		else
		{
			m_camera.fieldOfView = m_cameraToCopy.fieldOfView;
		}
	}
}
