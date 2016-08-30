using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class EntityBg : IEntity
{
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------

	private GameCamera m_newCamera;
	private ISpawner m_spawner;

	/************/

	// Use this for initialization
	void Start () 
	{
		m_newCamera = Camera.main.GetComponent<GameCamera>();
	}



	public override void Disable(bool _destroyed) 
	{
		base.Disable( _destroyed );
		if ( m_spawner != null )
			m_spawner.RemoveEntity(gameObject, _destroyed);
	}

	public override void Spawn(ISpawner _spawner) 
	{
		m_spawner = _spawner;
	}

	/*****************/
	// Private stuff //
	/*****************/
	void LateUpdate() {
		// check camera to destroy this entity if it is outside view area
		if (m_newCamera.IsInsideBackgroundDeactivationArea(transform.position))
		{
			Disable(false);
		}
	}
}
