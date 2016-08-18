using UnityEngine;
using System.Collections;

public class DragonControlPlayer : DragonControl {


	TouchControlsDPad	touchControls = null;
    JoystickControls    joystickControls = null;
    bool joystickMoving = false;

    // Use this for initialization
    void Start () {
		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null) {
			touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
            joystickControls = gameInputObj.GetComponent<JoystickControls>();
        }
	}

	void OnEnable() {
		if(touchControls != null) {
			touchControls.enabled = true;
#if UNITY_EDITOR
            joystickControls.enabled = true;
#endif
        }
    }

	void OnDisable() {
		if(touchControls != null) {
			touchControls.SetTouchObjRendering(false);
			touchControls.enabled = false;
			moving = false;
			action = false;
#if UNITY_EDITOR
            joystickControls.enabled = false;
#endif
        }
    }
	
	// Update is called once per frame
	void Update () {
        // Update touch controller
        if (touchControls != null) {
			touchControls.UpdateTouchControls();

			moving = touchControls.CurrentTouchState != TouchState.none;
			action = touchControls.touchAction;
		}
#if UNITY_EDITOR
        if (joystickControls != null)
        {
            joystickControls.UpdateJoystickControls();
            joystickMoving = joystickControls.isMoving();
            moving = moving || joystickMoving;
            action = action || joystickControls.getAction();
        }
#endif
    }

	override public Vector3 GetImpulse(float desiredVelocity){
#if UNITY_EDITOR
        if (joystickControls != null && joystickMoving)
        {
             joystickControls.CalcSharkDesiredVelocity(desiredVelocity);
             return new Vector3(joystickControls.SharkDesiredVel.x, joystickControls.SharkDesiredVel.y, 0f);
        }
#endif
        if (touchControls != null && moving)
        {
            touchControls.CalcSharkDesiredVelocity(desiredVelocity);
            return new Vector3(touchControls.SharkDesiredVel.x, touchControls.SharkDesiredVel.y, 0f);
        }          

        return Vector3.zero;
    } 
}
