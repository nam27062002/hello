using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class EntityBg : IEntity
{
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	
	private ISpawner m_spawner;

	/************/
	void OnDestroy() {
		if (EntityManager.instance != null) {
			EntityManager.instance.UnregisterEntityBg(this);
		}
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
}
