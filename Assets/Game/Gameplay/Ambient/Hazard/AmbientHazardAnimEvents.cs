using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientHazardAnimEvents : MonoBehaviour {

	public delegate void OnAnimVoidEvent();
	public OnAnimVoidEvent onOpenEvent; 


	void OpenEvent()
	{
		if (onOpenEvent != null)
			onOpenEvent();
	}

}
