using UnityEngine;
using System.Collections;

public class DragonControlPlayer : MonoBehaviour {

	[HideInInspector] public bool moving = false;
	[HideInInspector] public bool action = false;

	TouchControlsDPad	touchControls = null;
    JoystickControls    joystickControls = null;
    bool 				joystickMoving = false;
    TiltControls		tiltControls = null;
    bool 				tiltMoving = false;

	private Assets.Code.Game.Spline.BezierSpline m_followingSpline;
	private int m_testDirecton = 1;



    // Use this for initialization
    void Start () {
		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null) {
			touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
			// tiltControls = gameInputObj.GetComponent<TiltControls>();
			if ( tiltControls != null )
				tiltControls.Calibrate();
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
        }

        if ( tiltControls != null )
        	tiltControls.enabled = true;

#if UNITY_EDITOR
		if ( joystickControls != null )
			joystickControls.enabled = true;
#endif

    }

	void OnDisable() {
		if(touchControls != null) {
			touchControls.SetTouchObjRendering(false);
			touchControls.enabled = false;
		
        }

        if ( tiltControls != null )
			tiltControls.enabled = false;

#if UNITY_EDITOR
		if ( joystickControls != null )
            joystickControls.enabled = false;
#endif

		moving = false;
		action = false;

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

		if ( tiltControls != null )
		{
			tiltControls.UpdateTiltControls();
			tiltMoving = tiltControls.IsMoving();
			moving = moving || tiltMoving;
			action = action || tiltControls.getAction();

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

	public void GetImpulse(float desiredVelocity, ref Vector3 impulse){

		impulse = Vector3.zero;

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
			impulse = move.normalized * desiredVelocity;
		}	

#if UNITY_EDITOR
        if (joystickControls != null && joystickMoving)
        {
             joystickControls.CalcSharkDesiredVelocity(desiredVelocity);
			impulse.x = joystickControls.SharkDesiredVel.x;
			impulse.y = joystickControls.SharkDesiredVel.y;
			impulse.z = 0;
			return;
        }
#endif
		if ( tiltControls != null && tiltMoving )
        {
        	tiltControls.CalcSharkDesiredVelocity( desiredVelocity );
			impulse.x = tiltControls.SharkDesiredVel.x;
			impulse.y = tiltControls.SharkDesiredVel.y;
			impulse.z = 0;
			return;
        }

        if (touchControls != null && moving)
        {
            touchControls.CalcSharkDesiredVelocity(desiredVelocity);
			impulse.x = touchControls.SharkDesiredVel.x;
			impulse.y = touchControls.SharkDesiredVel.y;
			impulse.z = 0;
			return;
        }


    } 
}
