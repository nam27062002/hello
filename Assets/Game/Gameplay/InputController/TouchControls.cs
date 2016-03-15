using UnityEngine;
using System.Collections;

abstract public class TouchControls : MonoBehaviour {
	
	// INSPECTOR VARIABLES
	public bool m_boostWithRadiusCheck = false;
	public bool m_boostWithSecondTouch = true;
	public bool m_boostWithHardPush = true;

	// PROTECTED MEMBERS
	protected TouchControlsType m_type;
	
	protected TouchState m_currentTouchState = TouchState.none;
	protected Vector3 m_currentTouchPos = Vector3.zero;
	protected Vector3 m_initialTouchPos = Vector3.zero;
	protected Vector3 m_initialTouchPosWorldSpace = Vector3.zero;
	protected Vector3 m_diffVecNorm = Vector3.zero;
	protected Vector3 m_diffVec = Vector3.zero;
	protected Vector2 m_sharkDesiredVel = Vector2.zero;
	protected bool m_decelerate = false;
	
	protected TouchState m_currentTouchState2 = TouchState.none;
	
	// Touch Rendering on screen
	protected bool m_isTouchObjsRendering = false;
	
	// ACCESSORS
	public TouchControlsType TouchType { get { return m_type; } }
	public Vector2 SharkDesiredVel { get { return m_sharkDesiredVel; } }
	public TouchState CurrentTouchState { get { return this.m_currentTouchState; } }
	public Vector3 CurrentTouchPos { get { return m_currentTouchPos; } }

	public bool touchAction = false;


	
	// Use this for initialization
	virtual public void Start () {
	
		m_isTouchObjsRendering = false;
		
		// Need to do this, as Unity doesn't seem to clear previous mouse clicks until the first query (i.e. GetMouseButton...())
		// e.g. you clicked button '0' in the front end, and then during the game queried for GetMouseButtonDown(1) during
		// the Update() method... the first time would also return a positive for GetMouseButtonDown(0)...
		if(m_boostWithSecondTouch)
		{
			Input.GetMouseButtonDown(0);
			Input.GetMouseButtonDown(1);
			Input.GetMouseButton(0);
			Input.GetMouseButton(1);
			Input.GetMouseButtonUp(0);
			Input.GetMouseButtonUp(1);
		}
	}
	
	private void ResetTouchValues()
	{
		m_currentTouchState = TouchState.none;
		m_currentTouchPos = Vector3.zero;
		m_initialTouchPos = Vector3.zero;
		m_initialTouchPosWorldSpace = Vector3.zero;
		m_diffVecNorm = Vector3.zero;
		m_diffVec = Vector3.zero;
		m_sharkDesiredVel = Vector2.zero;
		m_decelerate = false;
		
		m_currentTouchState2 = TouchState.none;
	}
	
	virtual public void SetTouchObjRendering(bool on)
	{
		if(m_isTouchObjsRendering != on)
		{
			m_isTouchObjsRendering = on;
			SetRender(on);
		}
	}
	
	protected void RefreshCurrentTouchPos()
	{
		m_currentTouchPos.x = GameInput.touchPosition[0].x;
		m_currentTouchPos.y = GameInput.touchPosition[0].y;
		m_currentTouchPos.z = 0;
	}
	
	virtual public void SetRender(bool enable)
	{
		// not marking this abstract as you could have both touch controls without any rendering...
	}
	
	virtual public void ReleaseTouch()
	{
		if(m_currentTouchState != TouchState.none)
		{
			ResetTouchValues();
			SetTouchObjRendering(false);
		}
	}
	
	abstract public bool OnTouchPress();
	abstract public bool OnTouchHeld();
	abstract public bool OnTouchRelease();
	abstract public void CalcSharkDesiredVelocity(float speed, bool disableDecceleration = false);
	
	virtual public void UpdateTouchControls() 
	{
		if(GameInput.m_controlMethod == ControlMethod.touch)
		{
			touchAction = false;
			//Debug.Log("ABOUT TO CHECK TOUCH STATE FOR 0..!!!");
			TouchState touchState = GameInput.CheckTouchState(0);
			//Debug.Log("Got touchState 0 = " + touchState.ToString()); 		// NO TOUCH STATE IS BEING RECEIVED AFTER APP COMES BACK
			if(touchState == TouchState.pressed)
			{
				if(m_currentTouchState == TouchState.none)
				{
					if(OnTouchPress())
					{
						m_currentTouchState = TouchState.pressed;
						m_decelerate = false;
						SetTouchObjRendering(true);
					}
				}
			}
			else if(touchState == TouchState.held)
			{
				if((m_currentTouchState == TouchState.pressed) || (m_currentTouchState == TouchState.held))
				{
					if(OnTouchHeld())
					{							
						m_currentTouchState = TouchState.held;
						m_decelerate = false;
						SetTouchObjRendering(true);
					}
				}

				if (m_currentTouchState == TouchState.held)
				{
					if (m_boostWithHardPush)
					{
						if ( GameInput.touchPressure[0] > 0.5f )
							touchAction = true;
					}
				}
			}
			else if(touchState == TouchState.released)
			{
				// plan the deceleration at this point of release... 
				if((m_currentTouchState == TouchState.pressed) || (m_currentTouchState == TouchState.held))
				{
					m_currentTouchState = TouchState.released;
					SetTouchObjRendering(false);
					
					if(OnTouchRelease())
					{
						m_decelerate = true;
					}
				}
				
#if UNITY_ANDROID
				// on android, when releasing touch 0, release touch 1 as well... otherwise it becomes touch 0
				// or set touch 0 with position and state as the earlier touch 1
				if(m_currentTouchState2 != TouchState.none)
				{
					m_currentTouchState2 = TouchState.released;
					
					SetTouchObjRendering(false);
					
					if(m_boostWithSecondTouch)
					{
						// GameLogic.player.SetAction(false);
					}
				}
#endif
			}
			else if(touchState == TouchState.none)
			{
				m_currentTouchState = TouchState.none;
				SetTouchObjRendering(false);
			}
			
			if(m_boostWithSecondTouch)
			{
				//Debug.Log("ABOUT TO CHECK TOUCH STATE FOR 1..!!!");
				TouchState touchState2 = GameInput.CheckTouchState(1);
				//Debug.Log("Got touchState 1 = " + touchState2.ToString()); 		// NO TOUCH STATE IS BEING RECEIVED AFTER APP COMES BACK
				if(touchState2 == TouchState.pressed)
				{
					if(m_currentTouchState2 == TouchState.none)
					{
						m_currentTouchState2 = TouchState.pressed;
					}
					if ( m_currentTouchState2 == TouchState.pressed )
						touchAction = true;
				}
				else if(touchState2 == TouchState.held)
				{
					if((m_currentTouchState2 == TouchState.pressed) || (m_currentTouchState2 == TouchState.held))
					{
						m_currentTouchState2 = TouchState.held;
						touchAction = true;
					}
				}
				else if(touchState2 == TouchState.released)
				{
					if((m_currentTouchState2 == TouchState.pressed) || (m_currentTouchState2 == TouchState.held))
					{
						m_currentTouchState2 = TouchState.released;
					}
				}
				else if(touchState2 == TouchState.none)
				{
					m_currentTouchState2 = TouchState.none;
				}
			}

		}
	}
	
	virtual public void ClearBoost( bool forceDecceleration )
	{
	}
}
