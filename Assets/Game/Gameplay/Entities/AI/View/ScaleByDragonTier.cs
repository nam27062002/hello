using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleByDragonTier : ISpawnable {
	public float[] m_scaleByTier = new float[ (int) DragonTier.COUNT ];
	override public void Spawn(ISpawner _spawner)
	{
		if ( InstanceManager.player )
		{
			transform.localScale *= m_scaleByTier[ (int)InstanceManager.player.data.tier ];
		}
	}
	override public void CustomUpdate(){}
	

}
