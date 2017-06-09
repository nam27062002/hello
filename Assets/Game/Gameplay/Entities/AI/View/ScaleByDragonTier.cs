using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleByDragonTier : MonoBehaviour {
	public float[] m_scaleByTier = new float[ (int) DragonTier.COUNT ];
	void Start () {
		if ( InstanceManager.player )
		{
			transform.localScale *= m_scaleByTier[ (int)InstanceManager.player.data.tier ];
		}
	}
	

}
