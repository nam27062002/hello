using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TouchControlsDPad : TouchControls {
	
	// INSPECTOR VARIABLES
	public GameObject m_dpadObj;
	public GameObject m_dpadDotObj;

	
	// PRIVATE VARIABLES - DPAD SPECIFIC
	private float m_radiusToCheck = 40.0f;
	private float m_boostRadiusToCheck = 50.0f;// another 10 pixels 
	
	private const float m_decelerationTimeLimit = 0.25f; // 0.5 seconds to come to a halt
	private float m_decelerationTimer = 0.0f;
	
	// DPAD Rendering
	private bool m_isInitialTouchPosSet = false;
		
	// [AOC] 
	private RectTransform m_dPadContainerRectTransform = null;
	private RectTransform m_dPadRectTransform = null;
	private RectTransform m_dPadDotRectTransform = null;

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

	// Use this for initialization
	override public void Start () 
	{
		// [AOC]
		m_dPadRectTransform = m_dpadObj.transform as RectTransform;
		m_dPadDotRectTransform = m_dpadDotObj.transform as RectTransform;
		m_dPadContainerRectTransform = m_dPadRectTransform.parent as RectTransform;
		m_dPadContainerRectTransform.anchoredPosition = Vector2.zero;	// Make sure it's centered to its anchors, which we will be moving around!

		base.Start();
		
		m_type = TouchControlsType.dpad;

		// [AOC]
		CanvasScaler parentCanvasScaler = m_dPadRectTransform.GetComponentInParent<CanvasScaler>();
		m_radiusToCheck = (m_dPadRectTransform.rect.width * 0.45f) * Screen.width / parentCanvasScaler.referenceResolution.x;	// Half width of the D-Pad applying the ratio between the retina-ref resolution our canvas is using and the actual screen size
		m_boostRadiusToCheck = m_radiusToCheck * 1.2f;
			
		m_dpadObj.SetActive(false);
		m_dpadDotObj.SetActive(false);
	}
	
	override public void SetRender(bool enable)
	{
		m_dpadObj.SetActive(enable);
		m_dpadDotObj.SetActive(enable);
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
			Vector2 correctedDPadPos = new Vector2(
				(m_initialTouchPos.x / Screen.width),
				(m_initialTouchPos.y / Screen.height)
			);
			m_dPadContainerRectTransform.anchorMin = correctedDPadPos;
			m_dPadContainerRectTransform.anchorMax = correctedDPadPos;

			// Move dot a distance within the pad's size in the same orientation as the touch diff vector and proportional to it
			// Using the anchors allows us to directly set relative position [0..1] within the parent
			Vector3 diff = (m_currentTouchPos - m_initialTouchPos);
			Vector3 dir = Vector3.Normalize(diff);
			float delta = Mathf.Clamp01(diff.magnitude/m_radiusToCheck);
			Vector2 correctedDPadDotPos = new Vector2(
				dir.x * delta * 0.5f + 0.5f,	// Scale from [-1..1] to [0..1]
				dir.y * delta * 0.5f + 0.5f		// Scale from [-1..1] to [0..1]
			);
			m_dPadDotRectTransform.anchorMin = correctedDPadDotPos;
			m_dPadDotRectTransform.anchorMax = correctedDPadDotPos;

		}
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
		float radius = 1.25f * m_radiusToCheck;
		Vector2 touchPos = GameInput.touchPosition[0];
		
		//if ( App.inGame )
		{
			// player touched in the border... snap the circle and dot (and touch) to however far it can go...
			if(touchPos.x < radius)
				m_initialTouchPos.x = radius;
			else if(touchPos.x > (Screen.width - radius))
				m_initialTouchPos.x = Screen.width - radius;
			else
				m_initialTouchPos.x = touchPos.x;
			
			// do the same for y
			if(touchPos.y < radius)
				m_initialTouchPos.y = radius;
			else if(touchPos.y > (Screen.height - radius))
				m_initialTouchPos.y = Screen.height - radius;
			else
				m_initialTouchPos.y = touchPos.y;
							

			m_initialTouchPos.z = 0;
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

		float speedDampenMult = 1.0f;
		m_speedDampenMult = radiusCovered / m_radiusToCheck;
		m_speedDampenMult = Mathf.Clamp(speedDampenMult, 0.0f, 1.0f);

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
}
