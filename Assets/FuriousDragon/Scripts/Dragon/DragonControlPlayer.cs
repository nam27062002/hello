using UnityEngine;
using System.Collections;

public class DragonControlPlayer : DragonControl {


	TouchControlsDPad	touchControls;

	// Use this for initialization
	void Start () {

		touchControls = GameObject.Find("PF_GameInput").GetComponent<TouchControlsDPad>();
	}
	
	// Update is called once per frame
	void Update () {
	
		// Update touch controller
		touchControls.UpdateTouchControls();

		moving = touchControls.CurrentTouchState != TouchState.none ;
		action = touchControls.touchAction;
	}

	override public Vector3 GetImpulse(float desiredVelocity){

		if (touchControls != null){

			touchControls.CalcSharkDesiredVelocity(desiredVelocity, false);
			return new Vector3(touchControls.SharkDesiredVel.x, touchControls.SharkDesiredVel.y, 0f);
		}else{

			return Vector3.zero;
		}
	} 
}
