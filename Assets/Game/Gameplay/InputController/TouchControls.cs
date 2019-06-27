using UnityEngine;
using System.Collections;

abstract public class TouchControls : MonoBehaviour, IBroadcastListener {

    // INSPECTOR VARIABLES
    public bool m_boostWithRadiusCheck = false;
	public bool m_boostWithSecondTouch = true;

	protected bool m_boostWithHardPush = false;
    private float m_boostWithHardPushThreshold = 0.85f;

    // PROTECTED MEMBERS
    protected TouchControlsType m_type;
	
	protected TouchState m_currentTouchState = TouchState.none;
	protected Vector3 m_currentTouchPos = Vector3.zero;
	protected Vector3 m_initialTouchPos = Vector3.zero;
	protected Vector3 m_diffVecNorm = Vector3.zero;
	protected Vector3 m_diffVec = Vector3.zero;
	protected Vector2 m_sharkDesiredVel = Vector2.zero;
	protected bool m_decelerate = false;

	protected Vector3 m_currentTouch2Pos = Vector3.zero;
	protected Vector3 m_initialTouch2Pos = Vector3.zero;
	
	protected TouchState m_currentTouchState2 = TouchState.none;
	
	// Touch Rendering on screen
	protected bool m_isTouchObjsRendering = false;
	protected bool m_isTouch2ObjsRendering = false;
	
	// ACCESSORS
	public TouchControlsType TouchType { get { return m_type; } }
	public Vector2 SharkDesiredVel { get { return m_sharkDesiredVel; } }
	public TouchState CurrentTouchState { get { return this.m_currentTouchState; } }
	public Vector3 CurrentTouchPos { get { return m_currentTouchPos; } }

	public bool touchAction = false;

	private bool m_registerFirstTouch = true; //first touch

	protected bool m_paused = false;
	protected bool m_render = false;

	// Use this for initialization
	virtual public void Start () {
	
		m_isTouchObjsRendering = false;
		m_isTouch2ObjsRendering = false;

		m_registerFirstTouch = true;
		
		// Need to do this, as Unity doesn't seem to clear previous mouse clicks until the first query (i.e. GetMouseButton...())
		// e.g. you clicked button '0' in the front end, and then during the game queried for GetMouseButtonDown(1) during
		// the Update() method... the first time would also return a positive for GetMouseButtonDown(0)...
		#if UNITY_EDITOR || UNITY_PC
		if(m_boostWithSecondTouch)
		{
			Input.GetMouseButtonDown(0);
			Input.GetMouseButtonDown(1);
			Input.GetMouseButton(0);
			Input.GetMouseButton(1);
			Input.GetMouseButtonUp(0);
			Input.GetMouseButtonUp(1);
		}
		#endif

        // Subscribe to external events
        Messenger.AddListener<string>(MessengerEvents.CP_PREF_CHANGED, OnPrefChanged);
		Broadcaster.AddListener(BroadcastEventType.GAME_PAUSED, this);
    }
	
    public virtual void OnDestroy() {
        // Unsubscribe from external events
        Messenger.RemoveListener<string>(MessengerEvents.CP_PREF_CHANGED, OnPrefChanged);      
		Broadcaster.RemoveListener(BroadcastEventType.GAME_PAUSED, this);
    }
    
    public virtual void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
		{
			case BroadcastEventType.GAME_PAUSED:
			{
				OnPause((broadcastEventInfo as ToggleParam).value);
			}break;
		}
    }
    

	private void ResetTouchValues()
	{
		m_currentTouchState = TouchState.none;
		m_currentTouchPos = Vector3.zero;
		m_initialTouchPos = Vector3.zero;
		m_diffVecNorm = Vector3.zero;
		m_diffVec = Vector3.zero;
		m_sharkDesiredVel = Vector2.zero;
		m_decelerate = false;

		m_currentTouch2Pos = Vector3.zero;
		m_initialTouch2Pos = Vector3.zero;
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

	virtual public void SetTouch2ObjRendering(bool on, bool is3dTouch)
	{
		if(m_isTouch2ObjsRendering != on)
		{
			m_isTouch2ObjsRendering = on;
			Set2Render(on, is3dTouch);
		}
	}


	protected void RefreshCurrentTouchPos()
	{
		m_currentTouchPos.x = GameInput.touchPosition[0].x;
		m_currentTouchPos.y = GameInput.touchPosition[0].y;

		m_currentTouch2Pos.x =  GameInput.touchPosition[1].x;
		m_currentTouch2Pos.y =  GameInput.touchPosition[1].y;
	}
	
	virtual public void SetRender(bool enable)
	{
		m_render = enable;
		// not marking this abstract as you could have both touch controls without any rendering...
	}

	virtual public void Set2Render(bool enable, bool is3dTouch)
	{
		// not marking this abstract as you could have both touch controls without any rendering...
	}

	abstract public bool OnTouchPress();
	abstract public bool OnTouchHeld();
	abstract public bool OnTouchRelease();
	abstract public void CalcSharkDesiredVelocity(float speed);
	
	virtual public void UpdateTouchControls() 
	{
		if(GameInput.m_controlMethod == ControlMethod.touch)
		{
			touchAction = false;
			bool touchActionIs3DTouch = false;
			// Debug.Log("ABOUT TO CHECK TOUCH STATE FOR 0..!!!");
			TouchState touchState = GameInput.CheckTouchState(0);            
			// Debug.Log("Got touchState 0 = " + touchState.ToString()); 		// NO TOUCH STATE IS BEING RECEIVED AFTER APP COMES BACK
			if(touchState == TouchState.pressed)
			{                
                if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN) && m_registerFirstTouch) {
					HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._03_game_started);
					m_registerFirstTouch = false;
				}

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
						if ( GameInput.touchPressure[0] > m_boostWithHardPushThreshold) {
							touchAction = true;
							touchActionIs3DTouch = true;
						} 
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
			}
			else if(touchState == TouchState.none)
			{
				m_currentTouchState = TouchState.none;
				SetTouchObjRendering(false);
			}
			
			if(m_boostWithSecondTouch)
			{
				// Debug.Log("ABOUT TO CHECK TOUCH STATE FOR 1..!!!");
				TouchState touchState2 = GameInput.CheckTouchState(1);                
                // Debug.Log("Got touchState 1 = " + touchState2.ToString()); 		// NO TOUCH STATE IS BEING RECEIVED AFTER APP COMES BACK
                if (touchState2 == TouchState.pressed)
				{
					if(m_currentTouchState2 == TouchState.none)
					{
						m_currentTouchState2 = TouchState.pressed;

						m_initialTouch2Pos.x = GameInput.touchPosition[1].x;
						m_initialTouch2Pos.y = GameInput.touchPosition[1].y;
						m_initialTouch2Pos.z = 0;

						m_currentTouch2Pos.x = GameInput.touchPosition[1].x;
						m_currentTouch2Pos.y = GameInput.touchPosition[1].y;
						m_currentTouch2Pos.z = 0;

					}
					if ( m_currentTouchState2 == TouchState.pressed ) {
						touchAction = true;
						touchActionIs3DTouch = false;
					}
				}
				else if(touchState2 == TouchState.held)
				{
					if((m_currentTouchState2 == TouchState.pressed) || (m_currentTouchState2 == TouchState.held))
					{
						m_currentTouch2Pos.x = GameInput.touchPosition[1].x;
						m_currentTouch2Pos.y = GameInput.touchPosition[1].y;

						m_currentTouchState2 = TouchState.held;
						touchAction = true;
						touchActionIs3DTouch = false;
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
			SetTouch2ObjRendering(touchAction, touchActionIs3DTouch);
		}
	}
	
	virtual public void ClearBoost( bool forceDecceleration )
	{
	}

    private void OnPrefChanged(string id)
    {     
        switch (id)            
        {
            default:
                OnPrefChangedExtended(id);
                break;
        }
    }

	/// <summary>
    /// Subclasses of this class just have to override this method to handle change of preferences
    /// </summary>
    /// <param name="id"></param>
    protected virtual void OnPrefChangedExtended(string id) {}

	private void OnPause(bool _pause) {
		if(_pause) {
			ResetTouchValues();
			SetRender(false);
			this.gameObject.SetActive(false);
			m_paused = true;
		} else {
			m_paused = false;
			SetRender(m_render);
			this.gameObject.SetActive(true);
		}
	}

    public void Set3DTouch( bool use3DTouch, float pressure )
    {
    	m_boostWithHardPush = use3DTouch;
    	m_boostWithHardPushThreshold = pressure;
    }

}
