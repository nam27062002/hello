using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {
	

	void Start() {


		ExtendedStart();
	}



	virtual protected void ExtendedStart() {}
	virtual public void Fire(Vector3 direction) {}


}
