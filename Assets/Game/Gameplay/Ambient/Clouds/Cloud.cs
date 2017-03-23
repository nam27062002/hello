using UnityEngine;
using System.Collections;

public class Cloud : MonoBehaviour {

	void OnTriggerExit(Collider other) 
	{
		DragonPlayer player = other.GetComponent<DragonPlayer>();
		if ( player != null && other is SphereCollider)
		{
			GameObject go = ParticleManager.Spawn("CloudSmoke", player.transform.position);
			if (go != null) {
				go.transform.rotation = player.transform.rotation;
				go.transform.Rotate( -90, 0, 0, Space.Self);
			}
		}
	}

}
