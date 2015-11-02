using UnityEngine;
using System.Collections;

public class DragonControlPlayer : DragonControl {


	TouchControlsDPad	touchControls = null;

	// Use this for initialization
	void Start () {
		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null) {
			touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
		}
	}

	void OnEnable() {
		if(touchControls != null) {
			touchControls.enabled = true;
		}
	}

	void OnDisable() {
		if(touchControls != null) {
			touchControls.SetTouchObjRendering(false);
			touchControls.enabled = false;
			moving = false;
			action = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		// Update touch controller
		if(touchControls != null) {
			touchControls.UpdateTouchControls();

			moving = touchControls.CurrentTouchState != TouchState.none;
			action = touchControls.touchAction;
		}
	}

	override public Vector3 GetImpulse(float desiredVelocity){

		if (touchControls != null && moving){

			touchControls.CalcSharkDesiredVelocity(desiredVelocity, false);
			return new Vector3(touchControls.SharkDesiredVel.x, touchControls.SharkDesiredVel.y, 0f);
		}else{

			return Vector3.zero;
		}
	} 
}
