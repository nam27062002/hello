using UnityEngine;
using System.Collections;

public class UICamera : MonoBehaviour {

	public bool automatic = false;
	public float manualHeight=800f;


	// Update is called once per frame
	void Update () {

		manualHeight = Mathf.Max(2, automatic ? Screen.height : manualHeight);
		
		float size = 2f / manualHeight;
		Vector3 ls = transform.localScale;
		
		if (!Mathf.Approximately(ls.x, size) ||
		    !Mathf.Approximately(ls.y, size) ||
		    !Mathf.Approximately(ls.z, size))
		{
			transform.localScale = new Vector3(size, size, size);
		}

	}
}
