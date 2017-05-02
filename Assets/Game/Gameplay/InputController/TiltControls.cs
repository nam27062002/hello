﻿using UnityEngine;
using System.Collections;


public class TiltControls : MonoBehaviour
{
	// default settings
	private float       m_defaultTiltSensitivity = 0.5f;
	private float       m_tiltSensitivityScaleMin = 8.0f;
    private float       m_tiltSensitivityScaleMax = 16.0f;
	private float       m_dodgyCalibratePosition = 0.475f;
	private float       m_tiltFilter = 0.1f;

	// vars
	private bool		m_isDodgyCalibratePosition;	// set if device is tilted to a position where it is illegal to calibrate
	private float		m_tiltSensitivity;			// 0 = min, 1.0 = max


	private Matrix4x4	m_tiltRaw;					// the tilt orientation as read from the device
	private Matrix4x4	m_tiltCalibrated;			// the position that we calibrated at
	private Matrix4x4	m_tiltInverseCalibrated;	// the inverse of the calibrated position, use to correct the raw position
	private Matrix4x4	m_tiltCorrected;			// this is the raw orientation relative to the calibrated one, this is usable

	private float		m_lastAccX = 0.0f;			// sensor readings from previous update, use for filtering
	private float		m_lastAccY = 0.0f;
	private float		m_lastAccZ = -1.0f;

	public bool m_touchAction = false;
	protected Vector2 m_diffVecNorm = Vector3.zero;
	protected Vector2 m_sharkDesiredVel = Vector2.zero;
	public Vector2 SharkDesiredVel { get { return m_sharkDesiredVel; } }

	virtual public void Start () {
		m_tiltSensitivity 		= m_defaultTiltSensitivity;

		m_tiltRaw 				= Matrix4x4.identity;
		m_tiltCalibrated 		= Matrix4x4.identity;
		m_tiltInverseCalibrated = Matrix4x4.identity;
		m_tiltCorrected 		= Matrix4x4.identity;

		Calibrate();
	}

	void OnEnable() {
		Calibrate();
	}

	public void UpdateTiltControls()
	{
		OnUpdate();
	}

	public void CalcSharkDesiredVelocity(float speed)
	{
		m_sharkDesiredVel.x = m_diffVecNorm.x * speed;
		m_sharkDesiredVel.y = m_diffVecNorm.y * speed;
	}

	public bool getAction()
	{
		return m_touchAction;
	}


	public void OnUpdate()
	{
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

		//Get the stick inputs
		m_diffVecNorm.x = tx;
		m_diffVecNorm.y = ty;

        /*
		if (stick.sqrMagnitude > 1.0f)
		{
			stick = stick.normalized;
		}
        */

		//Now get the touch for boost
		bool fire = false;
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
            //int effectiveFingerId = touch.fingerId - lowestFingerId;
			//if ( touch.phase == UnityEngine.TouchPhase.Began )
            if ((touch.phase == UnityEngine.TouchPhase.Began) ||
                (touch.phase == UnityEngine.TouchPhase.Moved) ||
                (touch.phase == UnityEngine.TouchPhase.Stationary))
			{
				fire = true;
			}
		}

		m_touchAction = fire;

	}

	public bool IsMoving()
	{
		return Mathf.Abs( m_diffVecNorm.x) > float.Epsilon || Mathf.Abs( m_diffVecNorm.y ) > float.Epsilon;
	}

	public bool Calibrate()
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

        SetSensitivity(m_defaultTiltSensitivity);

        return m_isDodgyCalibratePosition;
	}

    public void SetSensitivity(float value)
    {
        if ((value >= 0.0f) && (value <= 1.0f))
        {
            m_tiltSensitivity = value * 4;
        }
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
}
