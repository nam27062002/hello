using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleByDragonTier : MonoBehaviour, ISpawnable {
	public float[] m_scaleByTier = new float[ (int) DragonTier.COUNT ];
	public void Spawn(ISpawner _spawner)
	{
		if ( InstanceManager.player )
		{
			transform.localScale *= m_scaleByTier[ (int)InstanceManager.player.data.tier ];
		}
	}
	public void CustomUpdate(){}
	

}
