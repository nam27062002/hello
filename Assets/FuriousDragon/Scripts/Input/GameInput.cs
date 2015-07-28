//--------------------------------------------------------------------------------
// GameInput
//--------------------------------------------------------------------------------
//#define TEST_KEYBOARD_IN_EDITOR	// MattG: this bypasses some new control stuff to allow us to run on PC with arrow keys and ctrl to boost, as before

using UnityEngine;
using System.Collections;
using System.IO;

public enum TouchState
{
	none,
	pressed,
	held,
	released
}
public enum TouchControlsType
{
	leash,
	dpad
}

public enum ControlMethod 
{
	tilt, 
	touch, 
	joystick,
	mouse,
	keyboard
}

public class GameInput : MonoBehaviour
{
	// SET DEFAULT CONTROL METHOD HERE! NO LONGER EXPOSING IT THROUGH SCRIPT. Too many King/App problems!
	
	#if UNITY_STANDALONE && FGOL_DESKTOP
	public static ControlMethod m_defaultControlMethod = ControlMethod.mouse;
	#else
	public static ControlMethod m_defaultControlMethod = ControlMethod.touch;
	#endif
	
	public static ControlMethod m_controlMethod = m_defaultControlMethod;
	
	// editor settings
	public float			m_defaultTiltSensitivity = 0.5f;
	public float			m_tiltSensitivityScaleMin = 8.0f;
	public float			m_tiltSensitivityScaleMax = 16.0f;
	public float			m_dodgyCalibratePosition = 0.475f;
	public float			m_tiltFilter = 0.1f;
	
	// accessable vars
	private bool			m_isTiltAvailable;			// if we don't detect an accelerometer, we'll have to use touch
	private bool			m_isUsingGyroscope;
	private bool			m_isDodgyCalibratePosition;	// set if device is tilted to a position where it is illegal to calibrate
	private float			m_tiltX;					// -1 means steering hard left, 0 is centre, +1 is hard right
	private float			m_tiltY;					// -1 means pitching up to maximum level, +1 is pitching max down
	private float			m_tiltSensitivity;			// 0 = min, 1.0 = max
	
	private float			m_mouseSpeedDampen;
	
	private const int 		m_maxTouches = 2;
	
	// making this static so we can test in WIP levels
	private static Vector2[]		m_touchPosition = new Vector2[m_maxTouches];
	private static int				m_touchID;
	
	public bool				isTiltAvailable				{get{return m_isTiltAvailable;}}
	public bool				isUsingGyroscope			{get{return m_isUsingGyroscope;}}
	public bool				isDodgyCalibratePosition	{get{return m_isDodgyCalibratePosition;}}
	public float			tiltX						{get{return m_tiltX;}}
	public float			tiltY						{get{return m_tiltY;}}
	public float			tiltSensitivity				{set{if(0 <= value && value <= 1) m_tiltSensitivity = value;} get{return m_tiltSensitivity;}}
	
	public float			mouseSpeedDampen			{get{return m_mouseSpeedDampen;}}
	
	// making this static so we can test in WIP levels
	public static Vector2[]		touchPosition				{get{return m_touchPosition;}} 
	public static int			touchID						{get{return m_touchID;}}
	public TouchControlsType	touchControlsType { get; set; }
	
	// private stuff
	private Matrix4x4		m_tiltRaw;					// the tilt orientation as read from the device
	private Matrix4x4		m_tiltCalibrated;			// the position that we calibrated at
	private Matrix4x4		m_tiltInverseCalibrated;	// the inverse of the calibrated position, use to correct the raw position
	private Matrix4x4		m_tiltCorrected;			// this is the raw orientation relative to the calibrated one, this is usable
	
	private float			m_lastAccX = 0.0f;			// sensor readings from previous update, use for filtering
	private float			m_lastAccY = 0.0f;
	private float			m_lastAccZ = -1.0f;
	
	private bool			m_screenFlipped = false;	// set if device has been tilted upside down (by default uses landscape left)
	private bool			m_orientationLocked = false;
	//--------------------------------------------------------------------------------
	
	// Use this for initialization
	void Awake()
	{
		SetDefaults();
		CalibrateTilt();
	}
	
	// Update is called once per frame
	void Update()
	{
		#if (TEST_KEYBOARD_IN_EDITOR && UNITY_EDITOR) || (UNITY_STANDALONE && !UNITY_EDITOR)
		m_controlMethod = ControlMethod.joystick;
		#endif
		
		
		#if UNITY_IPHONE
		// NB: Kindle Fire HD - gets orientations upside down, so don't flip until Unity fix it.
		UpdateDeviceOrientation();
		#endif // UNITY_IPHONE
		
		// get current tilt values, corrected for calibration
		UpdateRawTilt();
		m_tiltCorrected = m_tiltInverseCalibrated * m_tiltRaw;
		
		// get corrected/calibrated sensor values, and convert to euler pitch/roll using arctan
		float vx = m_tiltCorrected.m10;
		float vy = m_tiltCorrected.m11;
		float vz = m_tiltCorrected.m12;
		
		float delta0 = Mathf.Atan2(-vx, vy);
		float delta1 = Mathf.Atan2(-vz, vy);
		delta0 = Util.FixAnglePlusMinusRadians(delta0);
		delta1 = Util.FixAnglePlusMinusRadians(delta1);
		// convert these angles into +/-1 range, where -1 means -90 degrees and +1 means +90 degrees
		float tx = delta0 / Mathf.PI;
		float ty = delta1 / Mathf.PI;
		
		// apply a bit of a deadzone to filter out noisy inputs when we're trying to keep the device still
		float dzLen = Mathf.Sqrt(tx*tx + ty*ty);
		float deadZone = 0.01f;
		// scale
		if(dzLen<deadZone)
		{
			float s = dzLen/deadZone;
			tx *= s;
			ty *= s;
		}
		
		// Tilt range of +/- 1 represents tilting the device by 90 degrees.
		// We now want to reduce that based on the tilt sensitivity value.
		// Magnify the tilt values and then clamp to -1 to +1 range.
		float sensScale = Mathf.Lerp(m_tiltSensitivityScaleMin, m_tiltSensitivityScaleMax, m_tiltSensitivity);
		tx *= sensScale;
		ty *= sensScale;
		float len = Mathf.Sqrt(tx*tx + ty*ty);	// instead of just clamping to +/-1, get the 2D vector length and clamp if out of range.
		if(len > 1.0f)							// This way we are clamping the tilt range to a "circle" rather than a "square".
		{
			tx /= len;
			ty /= len;
		}
		
		// final usable values.
		// todo: maybe rename this from 'tilt', same value to use with virtual stick?
		m_tiltX = tx;
		m_tiltY = ty;
		
		#if UNITY_IPHONE
		/* [PAC]
		if( FGOLControllerMapper.Instance.ControllerConnected )
		{
			float xInput = FGOLControllerMapper.Instance.GetAxis( "AXIS_LEFT_STICK_HORZ" );
			float yInput = FGOLControllerMapper.Instance.GetAxis( "AXIS_LEFT_STICK_VERT" );
			
			len = Mathf.Sqrt(xInput*xInput + yInput*yInput);
			if(len > 1.0f)						// clamp to circle
			{
				xInput /= len;
				yInput /= len;
			}
			
			m_tiltX = xInput;
			m_tiltY = yInput;
		}
		*/
		#endif
		
		// if using joystick, just trash over tilt values with joystick values
		if(m_controlMethod == ControlMethod.joystick || m_controlMethod == ControlMethod.keyboard)
		{
			#if (TEST_KEYBOARD_IN_EDITOR && UNITY_EDITOR) || (UNITY_STANDALONE && !UNITY_EDITOR)
			float ix = Input.GetAxis("Horizontal");
			float iy = Input.GetAxis("Vertical");
			#else
			float ix = Input.GetAxis( "Left_Stick_Horz" );
			float iy = Input.GetAxis( "Left_Stick_Vert" );

            //float ix = -Input.GetAxis( "Axis5" );
            //float iy = -Input.GetAxis( "Axis6" );
			#endif
			
			len = Mathf.Sqrt(ix*ix + iy*iy);
			if(len > 1.0f)						// clamp to circle
			{
				ix /= len;
				iy /= len;
			}
			
			m_tiltX = ix;
			m_tiltY = iy;
		}
		else if(m_controlMethod == ControlMethod.mouse)
		{
			Vector3 currentTouchPos = Input.mousePosition;
			
			float diffX = currentTouchPos.x - Screen.width / 2;
			float diffY = currentTouchPos.y - Screen.height / 2;
			
			Vector2 diff = new Vector2(diffX, diffY);
			float radiusCovered = diff.magnitude;
			
			float speedDampenMult = 1.0f;
			speedDampenMult = radiusCovered / (Screen.height * 0.09f);
			speedDampenMult = Mathf.Clamp(speedDampenMult, 0.0f, 1.0f);
			
			m_mouseSpeedDampen = speedDampenMult;
			
			float maxX = Screen.width / 2;
			float maxY = Screen.height / 2;
			
			float normX = diffX / maxX;
			float normY = diffY / maxY;
			
			normX *= 90.0f;
			normY *= 90.0f;
			
			len = Mathf.Sqrt(normX*normX + normY*normY);
			if(len > 1.0f)
			{
				normX /= len;
				normY /= len;
			}
			
			m_tiltX = normX;
			m_tiltY = normY;
		}
	}
	
	public void SetDefaults()
	{
		m_isTiltAvailable = CheckTiltAvailable();
		
		m_isUsingGyroscope = false;
		m_tiltX = 0.0f;
		m_tiltY = 0.0f;
		m_tiltSensitivity = m_defaultTiltSensitivity;
		
		m_tiltRaw = Matrix4x4.identity;
		m_tiltCalibrated = Matrix4x4.identity;
		m_tiltInverseCalibrated = Matrix4x4.identity;
		m_tiltCorrected = Matrix4x4.identity;
		
	}
	
	public static TouchState CheckTouchState(int id)
	{	

		//Debug.Log("CHECKING TOUCH STATE FOR " + id + "..!!!");
		#if ((UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR)
		// NOTE: iPhone multitouch works differently from Android multitouch. Pressing a finger down becomes touch 0, and pressing a second finger
		// down becomes touch 1 - this is common for both platforms. However, releasing touch 0 has different behaviours: on iOS, the remaining
		// finger still remains touch 1, on Android it becomes touch 0
		
		// NOTE(2): Unity Android bug: On each return to the app (after app loses focus), fingerIds get incremented by 1. Until Unity fixes this,
		// we'll put in a temp hack that takes relative finger ids starting from the lowest one....
		
		//Debug.Log("Checking for id: " + id);
		//string debugString = "Current touch ids: ";
		
		int lowestFingerId = int.MaxValue;
		foreach(Touch touch in Input.touches)
		{
			//debugString += touch.fingerId + ",";
			if(touch.fingerId <= lowestFingerId)
			{
				lowestFingerId = touch.fingerId;
			}
		}
		
		//debugString += " | Num touches = " + Input.touchCount + " | lowestFingerId = " + lowestFingerId + " | ";
		//Debug.Log(debugString);
		
		for(int i=0; i < Input.touches.Length; i++)
		{
			Touch touch = Input.touches[i];
			
			#if UNITY_IPHONE
			int effectiveFingerId = touch.fingerId;
			#elif (UNITY_ANDROID && !UNITY_EDITOR)
			//int effectiveFingerId = touch.fingerId - lowestFingerId;
			int effectiveFingerId = i;
			#endif
			
			// to compensate for another bug where maintaining a touch while losing focus from the app results in touchIds
			// such as [2, 4, ..] on return to the app... 
			//Debug.Log("Touch phase for effective Finger id " + effectiveFingerId + " is: " + touch.phase.ToString());
			if (( touch.phase == UnityEngine.TouchPhase.Began ) && (effectiveFingerId == id))
			{
				m_touchPosition[id].Set(touch.position.x, touch.position.y);
				m_touchID = effectiveFingerId;
				
				//Debug.Log("Returning Touch " + id + " pressed!");
				return TouchState.pressed;
			}
			else if(((touch.phase == UnityEngine.TouchPhase.Moved) || (touch.phase == TouchPhase.Stationary)) && (effectiveFingerId == id))
			{
				m_touchPosition[id].Set(touch.position.x, touch.position.y);
				m_touchID = effectiveFingerId;
				
				//Debug.Log("Returning Touch " + id + " held!");
				return TouchState.held;
			}
			else if((touch.phase == UnityEngine.TouchPhase.Ended) && (effectiveFingerId == id))
			{
				m_touchPosition[id].Set(touch.position.x, touch.position.y);
				m_touchID = effectiveFingerId;
				
				//Debug.Log("Returning Touch " + id + " released!");
				return TouchState.released;
			}
		}
		#else
		if(Input.GetMouseButtonDown(id))
		{
			Vector3 mousePos = Input.mousePosition;
			m_touchPosition[id].Set(mousePos.x, mousePos.y);
			m_touchID = id;
			return TouchState.pressed;
		}
		else if(Input.GetMouseButton(id))
		{
			Vector3 mousePos = Input.mousePosition;
			m_touchPosition[id].Set(mousePos.x, mousePos.y);
			m_touchID = id;
			return TouchState.held;
		}
		else if(Input.GetMouseButtonUp(id))
		{
			Vector3 mousePos = Input.mousePosition;
			m_touchPosition[id].Set(mousePos.x, mousePos.y);
			m_touchID = id;
			return TouchState.released;
		}
		#endif // UNITY_DEVICES
		
		//Debug.Log("Returning Touch " + id + " none.");
		return TouchState.none;
	}
	
	// get position of initial touch. externally - if this is within the pickup radius of the directional control, dc claims this ID until quit.
	public bool GetTapPosition()
	{
		#if UNITY_IPHONE
		/* [PAC]
		if( FGOLControllerMapper.Instance.ControllerConnected )
		{
			for(int i=0; i < m_maxTouches; i++)
			{
				m_touchPosition[i].Set(0,0);
			}
			return FGOLControllerMapper.Instance.GetButton( "BUTTON_A" );
		}
		*/
		#endif
		// MattG: for testing with joystick control with no tilt/touch device.
		// Won't allow you to navigate the UI but should at least allow boost to work.
		if(m_controlMethod == ControlMethod.joystick || m_controlMethod == ControlMethod.mouse || m_controlMethod == ControlMethod.keyboard)
		{
			for(int i=0; i < m_maxTouches; i++)
			{
				m_touchPosition[i].Set(0,0);
			}
			m_touchID = 0;
			#if (TEST_KEYBOARD_IN_EDITOR && UNITY_EDITOR) || (UNITY_STANDALONE && !UNITY_EDITOR)
			return Input.GetButton("Button0");
			#else
			return Input.GetButton("Button0");
			#endif
		}
		
		bool ret = false;
		
		#if UNITY_EDITOR || UNITY_PC
		if(Input.GetMouseButton(0))
		{
			Vector3 mousePos = Input.mousePosition;
			m_touchPosition[0].Set(mousePos.x, mousePos.y);
			m_touchID = 0;
			ret = true;
		}
		#else
		// NOTE(2): Unity Android bug: On each return to the app (after app loses focus), fingerIds get incremented by 1. Until Unity fixes this,
		// we'll put in a temp hack that takes relative finger ids starting from the lowest one....
		int lowestFingerId = int.MaxValue;
		foreach(Touch touch in Input.touches)
		{
			if(touch.fingerId <= lowestFingerId)
			{
				lowestFingerId = touch.fingerId;
			}
		}
		foreach( Touch touch in Input.touches )
		{
			int effectiveFingerId = touch.fingerId - lowestFingerId;
			if ( touch.phase == UnityEngine.TouchPhase.Began )
			{
				m_touchPosition[0].Set(touch.position.x, touch.position.y);
				m_touchID = effectiveFingerId;
				ret = true;
			}
		}
		#endif
		return ret;
	}
	
	public bool GetTouchPositionForID(int id)
	{
		#if UNITY_IPHONE
		/* [PAC]
		if( FGOLControllerMapper.Instance.ControllerConnected )
		{
			for(int i=0; i < m_maxTouches; i++)
			{
				m_touchPosition[i].Set(0,0);
			}
			return FGOLControllerMapper.Instance.GetButton( "BUTTON_A" );
		}
		*/
		#endif
		// MattG: for testing with joystick control with no tilt/touch device.
		// Won't allow you to navigate the UI but should at least allow boost to work.
		if(m_controlMethod == ControlMethod.joystick || m_controlMethod == ControlMethod.mouse || m_controlMethod == ControlMethod.keyboard)
		{
			for(int i=0; i < m_maxTouches; i++)
			{
				m_touchPosition[i].Set(0,0);
			}
			#if (TEST_KEYBOARD_IN_EDITOR && UNITY_EDITOR) || (UNITY_STANDALONE && !UNITY_EDITOR)
			return Input.GetButton("Button0");
			#else
			return Input.GetButton( "Button0" );
			#endif
		}
		
		bool ret = false;
		
		// NOTE(2): Unity Android bug: On each return to the app (after app loses focus), fingerIds get incremented by 1. Until Unity fixes this,
		// we'll put in a temp hack that takes relative finger ids starting from the lowest one....
		int lowestFingerId = int.MaxValue;
		foreach(Touch touch in Input.touches)
		{
			if(touch.fingerId <= lowestFingerId)
			{
				lowestFingerId = touch.fingerId;
			}
		}
		
		foreach( Touch touch in Input.touches )
		{
			int effectiveFingerId = touch.fingerId - lowestFingerId;
			if 	((effectiveFingerId == id) && 
			     ((touch.phase == UnityEngine.TouchPhase.Began) || (touch.phase == UnityEngine.TouchPhase.Moved) || (touch.phase == UnityEngine.TouchPhase.Stationary))
			     )
			{
				m_touchPosition[id].Set(touch.position.x, touch.position.y);
				ret = true;
			}
		}
		
		return ret;
		
	}
	
	private void UpdateRawTilt()
	{
		// get raw acceleration values from sensor
		float ax = Input.acceleration.x;
		float ay = Input.acceleration.y;
		float az = Input.acceleration.z;
		
		// swap x and y now that we're on unity 4 
		float temp = ax;
		ax = ay;
		ay = -temp;
		
		
		// filter the input values
		float filter = m_tiltFilter;
		Util.FilterValue(ref ax, ref m_lastAccX, filter);
		Util.FilterValue(ref ay, ref m_lastAccY, filter);
		Util.FilterValue(ref az, ref m_lastAccZ, filter);
		
		// normalize
		float len = Mathf.Sqrt(ax*ax + ay*ay + az*az);
		if(len > 0.0f)
		{
			ax /= len;
			ay /= len;
			az /= len;
		}
		else
		{
			// we'll get here if no tilt device available.  Still may as well continue with default values to represent device lying flat.
			ax = 0.0f;
			ay = 0.0f;
			az = -1.0f;
		}
		
		// set this flag if the device is being held at too steep of an angle to safely calibrate.
		m_isDodgyCalibratePosition = (Mathf.Abs(az) < m_dodgyCalibratePosition) || az>0.0f;
		
		// Extract a rot matrix from the sensor values.
		// Currently we only have 3 values, these give us only the Y components of the 3 axes.
		float m01 = ay;
		float m11 = -az;
		float m21 = -ax;
		
		// Generate the other axes by using cross products.
		
		// default z components
		float m02 = 0.0f;
		float m12 = 0.0f;
		float m22 = 1.0f;
		
		// x = normalized (y cross z)
		float m00 = m11*m22 - m12*m21;
		float m10 = m21*m02 - m22*m01;
		float m20 = m01*m12 - m02*m11;
		len = Mathf.Sqrt(m00*m00 + m10*m10 + m20*m20);
		// no zero check necessary?
		m00 /= len;
		m10 /= len;
		m20 /= len;
		
		// fixed z = (x cross y)
		m02 = m10*m21 - m11*m20;
		m12 = m20*m01 - m21*m00;
		m22 = m00*m11 - m01*m10;
		
		// copy final values into the matrix.
		// looks like our mXY conventions are back to front, so flip them.
		m_tiltRaw.m00 = m00;
		m_tiltRaw.m10 = m01;
		m_tiltRaw.m20 = m02;
		m_tiltRaw.m01 = m10;
		m_tiltRaw.m11 = m11;
		m_tiltRaw.m21 = m12;
		m_tiltRaw.m02 = m20;
		m_tiltRaw.m12 = m21;
		m_tiltRaw.m22 = m22;
		
	}
	
	// if we get all zeroes back from Input.acceleration, means we're testing on PC with no tilt device available, or
	// maybe means we're actually running on a device without tilt.
	public bool CheckTiltAvailable()
	{
		#if !UNITY_EDITOR
		return true;
		#else
		return ((Input.acceleration.x != 0.0f) || (Input.acceleration.y != 0.0f) || (Input.acceleration.z != 0.0f));
		#endif
	}
	
	public void CalibrateTilt()
	{
		// Get the current tilt reading and make this the centred/calibrated position.
		UpdateRawTilt();
		
		// If we tried to calibrate in a dodgy position, we'll just have to leave it untouched.  It's up to the caller
		// to check the dodgyCalibratePosition flag and handle that situation in its own way.
		if(!m_isDodgyCalibratePosition)
		{
			m_tiltCalibrated = m_tiltRaw;
			m_tiltInverseCalibrated = m_tiltCalibrated.inverse;
			m_tiltCorrected = Matrix4x4.identity;
		}
	}
	
	private void UpdateDeviceOrientation()
	{
		// Check if orientation changed
		if(((Screen.orientation == ScreenOrientation.Landscape) && m_screenFlipped) ||
		   (Screen.orientation == ScreenOrientation.LandscapeRight) && !m_screenFlipped)
		{
			m_screenFlipped = !m_screenFlipped;
			print("SCREEN FLIPPED: "+m_screenFlipped);
		}
		
		// Check if orientation needs to be locked.  We disable auto rotate during gameplay.
		bool shouldLock = false; // [PAC] (App.Instance != null) && !App.paused;	// TODO: fix this if App/King stuff changes
		if(shouldLock != m_orientationLocked)
		{
			m_orientationLocked = shouldLock;
			if(shouldLock)
			{
				print("LOCKING ORIENTATION");
				Screen.autorotateToPortrait = false;
				Screen.autorotateToPortraitUpsideDown = false;
				Screen.autorotateToLandscapeLeft = false;
				Screen.autorotateToLandscapeRight = false;
			}
			else
			{
				print("UNLOCKING ORIENTATION");
				Screen.autorotateToLandscapeLeft = true;
				Screen.autorotateToLandscapeRight = true;
			}
		}		
	}
	
}
