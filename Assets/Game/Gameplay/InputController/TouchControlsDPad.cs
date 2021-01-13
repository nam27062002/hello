using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TouchControlsDPad : TouchControls {

    private const bool DEBUG = false;

	// [AOC] Different modes
	public enum Mode {
		FIXED,
		FOLLOW_TOUCH,
		FOLLOW_TOUCH_SMOOTH,
		FOLLOW_CUSTOM
	};
	
	// INSPECTOR VARIABLES
	[Space]
	public GameObject m_dpadObj;
	public GameObject m_dpadDotObj;
	public GameObject m_dpadBoost;

	// PRIVATE VARIABLES - DPAD SPECIFIC
	private float m_radiusToCheck = 40.0f;
	private float m_boostRadiusToCheck = 50.0f;// another 10 pixels 
	
	private const float m_decelerationTimeLimit = 0.25f; // 0.5 seconds to come to a halt
	private float m_decelerationTimer = 0.0f;
	
	// DPAD Rendering
	private bool m_isInitialTouchPosSet = false;
		
	// [AOC] D-Pad References
	private RectTransform m_dPadContainerRectTransform = null;
	private RectTransform m_dPadRectTransform = null;
	private RectTransform m_dPadDotRectTransform = null;

	private RectTransform m_dPadBoostContainerRectTransform = null;
	private RectTransform m_dPadBoostRectTransform = null;

	private UIColorFX m_dPadBoostColorFX = null;
	private bool m_dPadBoostFollow = true;

	// [AOC] D-Pad setup and logic
	private Mode m_dPadMode = Mode.FIXED; 
	private float m_dPadThreshold = 4.5f;
	private float m_dPadSmoothFactor = 0.5f;
	private float m_dPadBreakTolerance = 0.1f*5;
	private bool m_dPadClampDot = true;
	private bool m_dPadMoving = false;	// Internal logic, is the D-Pad moving?

	private bool m_disableDecceleration = false;
	private float m_decelerationMult = 1.0f;
	private float m_speedDampenMult = 1;
	private int m_frameCounter = 0;
	private const int m_numFramesForDirChange = 10;
	private Vector3 m_prevDiffVector = Vector3.zero;
	private float m_tolerance = 0.4f;
	private bool m_directionChanged;
	public bool directionChanged
	{
		get{ return m_directionChanged; }
	}

	private float m_lastArrowAngle = 0f;
    private float m_arrowDistance = 0f;
    public float arrowDistance{ get{ return m_arrowDistance; } set{ m_arrowDistance = value; } }

	// [AOC] Debug
	private TextMeshProUGUI m_debugText = null;

	// Use this for initialization
	override public void Start () 
    {
		// [AOC] Init references
		m_dPadRectTransform = m_dpadObj.transform as RectTransform;
		m_dPadDotRectTransform = m_dpadDotObj.transform as RectTransform;
		m_dPadContainerRectTransform = m_dPadRectTransform.parent as RectTransform;
		m_dPadContainerRectTransform.anchoredPosition = Vector2.zero;	// Make sure it's centered to its anchors, which we will be moving around!

		m_dPadBoostRectTransform = m_dpadBoost.transform as RectTransform;
		m_dPadBoostContainerRectTransform = m_dPadBoostRectTransform.parent as RectTransform;
		m_dPadBoostContainerRectTransform.anchoredPosition = Vector2.zero;	// Make sure it's centered to its anchors, which we will be moving around!

		m_dPadBoostColorFX = m_dpadBoost.FindComponentRecursive<UIColorFX>();
		OnBoost(false);

		m_debugText = m_dPadContainerRectTransform.FindComponentRecursive<TextMeshProUGUI>();	// Optional
        if (!DEBUG && m_debugText != null) {                        
            m_debugText.enabled = false;
            m_debugText = null;            
        }

		base.Start();
		
		m_type = TouchControlsType.dpad;

		// [AOC] Init some math aux vars
		CanvasScaler parentCanvasScaler = m_dPadRectTransform.GetComponentInParent<CanvasScaler>();
		if(parentCanvasScaler != null) {
			m_radiusToCheck = (m_dPadRectTransform.rect.width * 0.45f) * Screen.width / parentCanvasScaler.referenceResolution.x;   // Half width of the D-Pad applying the ratio between the retina-ref resolution our canvas is using and the actual screen size
		}
		m_boostRadiusToCheck = m_radiusToCheck * 1.2f;

		// [AOC] Load current setup
		m_dPadMode = (Mode)DebugSettings.Prefs_GetIntPlayer(DebugSettings.DPAD_MODE, (int)m_dPadMode);
		m_dPadThreshold = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.DPAD_THRESHOLD, m_dPadThreshold);
		m_dPadSmoothFactor = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.DPAD_SMOOTH_FACTOR, m_dPadSmoothFactor);
		m_dPadClampDot = DebugSettings.Prefs_GetBoolPlayer(DebugSettings.DPAD_CLAMP_DOT, m_dPadClampDot);
		m_dPadBoostFollow = DebugSettings.Prefs_GetBoolPlayer(DebugSettings.BOOST_PAD_FOLLOW, m_dPadBoostFollow);
			
		// Start hidden
		m_dpadObj.SetActive(false);
		m_dpadDotObj.SetActive(false);
		m_dpadBoost.SetActive(false);
		if(m_debugText != null) m_debugText.gameObject.SetActive(false);		

		m_lastArrowAngle = 0f;

		// Subscribe to external events
		 Broadcaster.AddListener(BroadcastEventType.BOOST_TOGGLED, this);
	}	

	override public void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.BOOST_TOGGLED, this);

		base.OnDestroy();
	}
    
    override public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.BOOST_TOGGLED:
            {
                ToggleParam toggleParam = (ToggleParam)broadcastEventInfo;
                OnBoost(toggleParam.value); 
            }break;
			default:
			{
				base.OnBroadcastSignal( eventType, broadcastEventInfo );
			}break;
        }
		
    }
    
	
	override public void SetRender(bool enable)
	{
		base.SetRender(enable);
		if (!m_paused)
		{
			m_dpadObj.SetActive(enable);
			m_dpadDotObj.SetActive(enable);
		}

		#if UNITY_EDITOR
		if(m_debugText != null) m_debugText.gameObject.SetActive(enable);
		#endif
	}
	
	override public void SetTouchObjRendering(bool on)
	{
		base.SetTouchObjRendering(on);

		// [AOC]
		if(on) {
			// Leave DPad static and move parent instead (which contains both the DPad and the Dot)
			// Using the anchors allows us to directly set relative position [0..1] within the parent
			// Since the parent of the container is directly the full-screen canvas, 
			// we just have to compute the relative pos of the touch in relation to the screen and apply it directly

			// Some aux vars
			float delta = m_diffVec.magnitude/m_radiusToCheck;

			// Compute whole D-Pad position
			// All modes share the same logic, just using different param values
			float threshold = m_dPadThreshold;
			float smoothFactor = m_dPadSmoothFactor;
			bool clampDot = m_dPadClampDot;
			switch(m_dPadMode) {
				// D-Pad remains fixed at initial touch position, only Dot moves
				case Mode.FIXED: {
					threshold = 0f;
					smoothFactor = 0f;
				} break;

				// D-Pad follows the touch if dot exits the check radius
				case Mode.FOLLOW_TOUCH: {
					threshold = 0f;
					smoothFactor = 1f;
				} break;

				// D-Pad slowly follows the touch if dot exits the check radius
				case Mode.FOLLOW_TOUCH_SMOOTH: {
					threshold = 0f;
					smoothFactor = 0.15f;
				} break;

				// Smooth variant with custom parameters
				case Mode.FOLLOW_CUSTOM: {
					// Nothing to change, using all pref settings
				} break;
			}

			// Compute new D-Pad pos!
			// Threshold reached?
			Vector3 dPadPos = m_initialTouchPos;
			if(m_dPadMoving) threshold = m_dPadBreakTolerance;	// When moving, ignore threshold (aka move until the current touch pos is reached) (actually make it a bit more generous, otherwise we never stop moving!)
			if(delta > 1f + threshold) {
				// Move in the direction of the current touch pos, proportional to the distance but applying a speed factor (0 min speed multiplier, 1 instant)
				Vector3 targetDistance = m_diffVec - (m_diffVecNorm * m_radiusToCheck);	// Stick dot to the edge!
				dPadPos.x += targetDistance.x * smoothFactor;
				dPadPos.y += targetDistance.y * smoothFactor;
				m_dPadMoving = true;
			} else {
				m_dPadMoving = false;
			}

			// Fit to screen and save it as new initial touch pos
			FitInScreen(ref dPadPos);
			m_initialTouchPos = dPadPos;

			// Transform from touch coords to relative [0..1] and apply
			Vector2 correctedDPadPos = new Vector2(
				(dPadPos.x / Screen.width),
				(dPadPos.y / Screen.height)
			);
			m_dPadContainerRectTransform.anchorMin = correctedDPadPos;
			m_dPadContainerRectTransform.anchorMax = correctedDPadPos;

			// Compute Dot position relative to the parent
			// Behave differently based on current mode
			switch(m_dPadMode) {
				// Same behaviour for both modes
				case Mode.FIXED:
				case Mode.FOLLOW_TOUCH:
				case Mode.FOLLOW_TOUCH_SMOOTH:
				case Mode.FOLLOW_CUSTOM: {
					float angle = m_lastArrowAngle;
					if (m_diffVecNorm != Vector3.zero) {
						angle = Vector2.Angle(Vector2.left, m_diffVecNorm); // the arrow graphics is facing left
						if (m_diffVecNorm.y > 0) angle *= -1;
						m_lastArrowAngle = angle;
					}
					m_dPadDotRectTransform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    if ( m_arrowDistance <= 0 )
                    {
                        m_dPadDotRectTransform.SetLocalPosX(0);
                        m_dPadDotRectTransform.SetLocalPosY(0);
                    }
                    else
                    {
                        m_dPadDotRectTransform.SetLocalPosX(-Mathf.Cos(Mathf.Deg2Rad * angle) * m_arrowDistance);
                        m_dPadDotRectTransform.SetLocalPosY(-Mathf.Sin(Mathf.Deg2Rad * angle) * m_arrowDistance);
                    }    
				} break;
			}

			// Debug text
			if(m_debugText != null) {
				m_debugText.text = delta.ToString() + "\n" + (m_dPadMoving ? "moving true" : "moving false") + "\n" + threshold;
			}
		}
	}

	override public void SetTouch2ObjRendering(bool on, bool is3dTouch)
	{
		base.SetTouch2ObjRendering(on, is3dTouch);

		if ( is3dTouch )
		{
			m_dPadBoostContainerRectTransform.anchorMin = m_dPadContainerRectTransform.anchorMin;
			m_dPadBoostContainerRectTransform.anchorMax = m_dPadContainerRectTransform.anchorMax;
		} 
		else 
		{
			Vector3 touchPos;
			if(m_dPadBoostFollow) {
				touchPos = m_currentTouch2Pos;		// [AOC] Follow touch position
			} else {
				touchPos = m_initialTouch2Pos;	// [AOC] Stay in initial position
			}
			FitInScreen(ref touchPos);
			Vector2 correctedDPadPos = new Vector2(
				(touchPos.x / Screen.width),
				(touchPos.y / Screen.height)
			);

			m_dPadBoostContainerRectTransform.anchorMin = correctedDPadPos;
			m_dPadBoostContainerRectTransform.anchorMax = correctedDPadPos;
		}
	}

	override public void Set2Render(bool enable, bool is3dTouch)
	{
		if (is3dTouch)
		{
			m_dpadObj.SetActive(!enable);
		}
		m_dpadBoost.SetActive(enable);
	}
	
	public void SetInitialTouchPos()
	{
		m_initialTouchPos.x = Screen.width * 0.5f;
		m_initialTouchPos.y = Screen.height * 0.5f;
		m_initialTouchPos.z = 0f;

		SetTouchObjRendering(true);
	}
	
	private void RefreshDiffVec()
	{
		Vector3 diff = m_currentTouchPos - m_initialTouchPos;
		
		m_diffVec.x = diff.x;
		m_diffVec.y = diff.y;
		m_diffVec.z = diff.z;

		m_diffVecNorm.x = diff.x;
		m_diffVecNorm.y = diff.y;
		m_diffVecNorm.z = diff.z;
		m_diffVecNorm.Normalize();
	}
	
	override public bool OnTouchPress()
	{
		// ensure touch is within the borders
		//if ( App.inGame )
		{
			m_initialTouchPos = GameInput.touchPosition[0];
			FitInScreen(ref m_initialTouchPos);
		}			
		return true;
	}

	override public bool OnTouchHeld()
	{
		RefreshCurrentTouchPos();
		RefreshDiffVec();

		if( m_boostWithRadiusCheck )
		{
			float radiusCovered = m_diffVec.magnitude;
			if(radiusCovered >= m_boostRadiusToCheck)
				touchAction = true;
		}

		return true;
	}
	
	override public bool OnTouchRelease()
	{
		RefreshDiffVec();
		return true;
	}
	
	override public void CalcSharkDesiredVelocity(float speed)
	{
		// normalize the distance of the click in world units away from the shark, by the max click distance
		m_sharkDesiredVel.x = m_diffVecNorm.x * speed * m_speedDampenMult * m_decelerationMult;
		m_sharkDesiredVel.y = m_diffVecNorm.y * speed * m_speedDampenMult * m_decelerationMult;
	}

	override public void UpdateTouchControls() 
	{
		base.UpdateTouchControls();

		m_decelerationMult = 1.0f;
		float radiusCovered = m_diffVec.magnitude;

		if( !m_decelerate )
		{
			m_decelerationMult = 1.0f;
		}
		else
		{
			// need to get to touch position somehow... slow down to it
			m_decelerationTimer += Time.deltaTime;
			if( (m_decelerationTimer >= m_decelerationTimeLimit) || m_disableDecceleration )
			{
				m_diffVecNorm.x = 0f;
				m_diffVecNorm.y = 0f;
				m_decelerate = false;
				
				m_decelerationTimer = 0.0f;
			}
			else
			{
				m_decelerationMult = (m_decelerationTimeLimit - m_decelerationTimer) / m_decelerationTimeLimit;
				m_decelerationMult = Mathf.Clamp(m_decelerationMult, 0.0f, 0.85f);
			}
		}

        // m_speedDampenMult = radiusCovered / m_radiusToCheck;
        // m_speedDampenMult = Mathf.Clamp(m_speedDampenMult, 0.0f, 1.0f);
        m_speedDampenMult = 1.0f;
		float change2 = (m_diffVecNorm - m_prevDiffVector).sqrMagnitude;
        if((change2 > (m_tolerance * m_tolerance)) && m_frameCounter >= m_numFramesForDirChange)
        {
			m_directionChanged = true;
            m_frameCounter = 0;
        }
        else
        {
			m_directionChanged = false;
            m_frameCounter++;
        }
		m_prevDiffVector = m_diffVecNorm;

	}
	
	override public void ClearBoost( bool forceDecceleration )
	{
		base.ClearBoost( forceDecceleration );

		if( forceDecceleration )
		{
			m_decelerate = true;
		}
		else
		{
			m_decelerate = false;
		}
	}

	/// <summary>
	/// Validates the position given and corrects it so it fits within the screen's borders.
	/// </summary>
	/// <param name="_pos">Position.</param>
	private void FitInScreen(ref Vector3 _pos) {
		// Tolerance distance
		float radius = 1.25f * m_radiusToCheck;

		// Check X
		if(_pos.x < radius) {
			_pos.x = radius;
		} else if(_pos.x > (Screen.width - radius)) {
			_pos.x = Screen.width - radius;
		} else {
			_pos.x = _pos.x;
		}

		// Do the same for y
		if(_pos.y < radius) {
			_pos.y = radius;
		} else if(_pos.y > (Screen.height - radius)) {
			_pos.y = Screen.height - radius;
		} else {
			_pos.y = _pos.y;
		}

		// Z is always 0
		_pos.z = 0;
	}

	/// <summary>
	/// A CP pref has been changed.
	/// </summary>
	/// <param name="_prefId">Preference identifier.</param>
	protected override void OnPrefChangedExtended(string _prefId) {
		// We only care about some prefs
		if(_prefId == DebugSettings.DPAD_MODE) {
			m_dPadMode = (Mode)Prefs.GetIntPlayer(DebugSettings.DPAD_MODE, (int)m_dPadMode);
		}

		else if(_prefId == DebugSettings.DPAD_THRESHOLD) {
			m_dPadThreshold = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.DPAD_THRESHOLD, m_dPadThreshold);
		}

		else if(_prefId == DebugSettings.DPAD_SMOOTH_FACTOR) {
			m_dPadSmoothFactor = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.DPAD_SMOOTH_FACTOR, m_dPadSmoothFactor);
		}

		else if(_prefId == DebugSettings.DPAD_BREAK_TOLERANCE) {
			m_dPadBreakTolerance = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.DPAD_BREAK_TOLERANCE, m_dPadBreakTolerance);
		}

		else if(_prefId == DebugSettings.DPAD_CLAMP_DOT) {
			m_dPadClampDot = DebugSettings.Prefs_GetBoolPlayer(DebugSettings.DPAD_CLAMP_DOT, m_dPadClampDot);
		}
	}

	/// <summary>
	/// Boost has been toggled.
	/// </summary>
	/// <param name="_boostOn">Boost on or off?.</param>
	protected void OnBoost(bool _boostOn) {
		// Set touch color
		m_dPadBoostColorFX.saturation = _boostOn ? 0f : -1f;
	}
}
