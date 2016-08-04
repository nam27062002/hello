using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class EntityBg : MonoBehaviour, ISpawnable 
{
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------

	private GameCameraController m_camera;
	private GameCamera m_newCamera;


	/************/




	// Use this for initialization
	void Start () 
	{
		m_camera = Camera.main.GetComponent<GameCameraController>();
		m_newCamera = Camera.main.GetComponent<GameCamera>();
	}



	public void Disable(bool _destroyed) 
	{
		gameObject.SetActive(false);
	}

	public void Spawn(ISpawner _spawner) 
	{
	}

	/*****************/
	// Private stuff //
	/*****************/
	void LateUpdate() {
		// check camera to destroy this entity if it is outside view area
		if (
			(DebugSettings.newCameraSystem && m_newCamera.IsInsideBackgroundDeactivationArea(transform.position)) || 
			(!DebugSettings.newCameraSystem && m_camera.IsInsideBackgroundDeactivationArea(transform.position))
		) 
		{
			Disable(false);
		}
	}
}
