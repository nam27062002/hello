using UnityEngine;
using System.Collections;

public class DragonControlPlayer : DragonControl {


	TouchControlsDPad	touchControls = null;
    JoystickControls    joystickControls = null;
    bool joystickMoving = false;

	private Assets.Code.Game.Spline.BezierSpline m_followingSpline;
	private int m_testDirecton = 1;

    // Use this for initialization
    void Start () {
		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null) {
			touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
            joystickControls = gameInputObj.GetComponent<JoystickControls>();
        }

        if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST )
        {
        	// Search path
			// m_followingSpline = ;
			GameObject go = GameObject.Find("TestPath");
			if (go != null)
			{
				m_followingSpline = go.GetComponent<Assets.Code.Game.Spline.BezierSpline>();
			}
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
		if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST && m_followingSpline != null)
		{
			moving = true;
			action = true;
			return;
		}
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

		// if app mode is test -> input something else?
		if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST && m_followingSpline != null)
		{
			float m_followingClosestT;
			int m_followingClosestStep;
			m_followingSpline.GetClosestPointToPoint( transform.position, 100, out m_followingClosestT, out m_followingClosestStep);
			if ( (m_followingClosestT >= 1.0f && m_testDirecton > 0) || (m_followingClosestT <= 0f && m_testDirecton < 0))
			{
				// What to do?
				m_testDirecton *= -1;
			}

			m_followingClosestT += 0.01f * m_testDirecton;

			Vector3 target = m_followingSpline.GetPoint( m_followingClosestT );
			target.z = 0;
			Vector3 move = target - transform.position;
			return move.normalized * desiredVelocity;
		}	

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
