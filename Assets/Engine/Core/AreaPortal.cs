using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaPortal : MonoBehaviour {

	public string m_areaPortal = "";

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Player") )
		{
			GameSceneController gameController = InstanceManager.GetSceneController<GameSceneController>();
			if ( gameController != null )
			{
				gameController.SwitchArea( m_areaPortal );
			}
		}
	}

}
