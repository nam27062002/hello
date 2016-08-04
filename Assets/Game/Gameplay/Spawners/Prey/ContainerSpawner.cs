using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContainerSpawner : MonoBehaviour
{
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private float m_spawnTime;

	private Spawner[] m_spawners;
	private ContainerBehaviour m_container;

	// Area to check if I have to recontruct/reinitialize the container
	protected AreaBounds m_area;		
	private GameCameraController m_camera;
	private GameCamera m_newCamera;
	private float m_timer;

	// Use this for initialization
	void Start() 
	{
		m_camera = Camera.main.GetComponent<GameCameraController>();
		m_newCamera = Camera.main.GetComponent<GameCamera>();
		m_area = GetArea();
		// Search all spawners
		m_spawners = GetComponentsInChildren<Spawner>();
		// Register spawn event

		// Only one can be active
		SelectRandomSpawner();

		// Search container
		m_container = GetComponentInChildren<ContainerBehaviour>();

		// Register to on break event
	}

	void LateUpdate()
	{
		if ( !m_container.enabled || !m_container.isActiveAndEnabled )	// Check container state is broken
		{
			bool isInsideDeactivationArea = false;
			if ( DebugSettings.newCameraSystem )
			{
				isInsideDeactivationArea = m_newCamera.IsInsideDeactivationArea(transform.position);
			}
			else
			{
				isInsideDeactivationArea = m_camera.IsInsideDeactivationArea(transform.position);
			}


			if (isInsideDeactivationArea)
			{
				if (m_timer > 0) 
				{
					m_timer -= Time.deltaTime;
					if (m_timer < 0) 
					{
						m_timer = 0;
					}
				} 
				else 
				{
					Reinit();
				}
			}

		}
	}

	void Reinit()
	{
		// Reset Container
		m_container.Reset();

		// Change Spawner
		SelectRandomSpawner();
	}

	void SelectRandomSpawner()
	{
		int selected =  Random.Range(0, m_spawners.Length);
		for( int i = 0; i<m_spawners.Length; i++ )
		{
			m_spawners[i].enabled = i == selected;
			m_spawners[i].ResetSpawnTimer();
		}
	}

	protected virtual AreaBounds GetArea() {
		Area area = GetComponent<Area>();
		if (area != null) {
			return area.bounds;
		} else {
			// spawner for static objects with a fixed position
			return new CircleAreaBounds(transform.position, 0);
		}
	}

}
