using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TouchControlsDPad : TouchControls {
	
	// INSPECTOR VARIABLES
	public bool m_isFixed = false;
	public GameObject m_dpadObj;
	public GameObject m_dpadDotObj;

	
	// PRIVATE VARIABLES - DPAD SPECIFIC
	private float m_radiusToCheck = 40.0f;
	private float m_boostRadiusToCheck = 50.0f;// another 10 pixels 
	
	private const float m_decelerationTimeLimit = 0.25f; // 0.5 seconds to come to a halt
	private float m_decelerationTimer = 0.0f;
	
	// DPAD Rendering
	private bool m_isInitialTouchPosSet = false;
		
	private Vector3 m_dpadPos = Vector3.zero;
	private Vector3 m_dpadDotPos = Vector3.zero;

	// [AOC] Quick'n'dirty fix!
	private RectTransform m_dPadRectTransform = null;
	private RectTransform m_dPadDotRectTransform = null;

	
	// Use this for initialization
	override public void Start () 
	{
		// [AOC] Quick'n'dirty fix!
		m_dPadRectTransform = m_dpadObj.transform as RectTransform;
		m_dPadDotRectTransform = m_dpadDotObj.transform as RectTransform;

		base.Start();
		
		m_type = TouchControlsType.dpad;
				

		RectTransform rt = m_dpadObj.GetComponent<RectTransform>();

		if (rt != null) {
			//m_radiusToCheck = rt.sizeDelta.x * rt.lossyScale.x * 0.45f;
			//m_boostRadiusToCheck = rt.sizeDelta.x * rt.lossyScale.x * 1.65f;

			// [AOC] Quick'n'dirty fix!
			m_radiusToCheck = m_dPadRectTransform.rect.width * 0.45f;
			m_boostRadiusToCheck = m_dPadRectTransform.rect.width * 1.65f;
		} else {
			m_radiusToCheck = Screen.height * 0.09f;
			m_boostRadiusToCheck = Screen.height * 0.15f;
		}

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
		
		//if(King._Instance != null)	// don't want to render anything in WIP levels
		{	
			if(on || m_isFixed)
			{
				// sync the circle and dot position to touch positions in screen space
				// NOTE: the positions are scaled because the camera quadrants are each in the range (+/- 960, +/-640)
				// while the screen quadrants (extents) are screen-dependent... 1280 x 960 or 1024 x 768.., etc.,
				m_dpadPos.x = m_initialTouchPos.x;
				m_dpadPos.y = m_initialTouchPos.y;
				m_dpadPos.z = 0;
				//m_dpadObj.transform.position = m_dpadPos;

				// project current touch pos on circle
				Vector3 diffUnit = (m_currentTouchPos - m_initialTouchPos);
				if(diffUnit.magnitude > m_radiusToCheck)
				{
					diffUnit.Normalize();
					diffUnit *= m_radiusToCheck;
					diffUnit += m_initialTouchPos;
					
					m_dpadDotPos.x = diffUnit.x;
					m_dpadDotPos.y = diffUnit.y;
					m_dpadDotPos.z = 0;
					//m_dpadDotObj.transform.position = m_dpadDotPos;
				}
				else
				{
					m_dpadDotPos.x = m_currentTouchPos.x;
					m_dpadDotPos.y = m_currentTouchPos.y;
					m_dpadDotPos.z = 0;
					//m_dpadDotObj.transform.position = m_dpadDotPos;
				}

				// [AOC] Quick'n'dirty fix!
				Vector2 correctedDPadPos = new Vector2(
					(m_dpadPos.x / Screen.width),
					(m_dpadPos.y / Screen.height)
				);
				m_dPadRectTransform.anchorMin = correctedDPadPos;
				m_dPadRectTransform.anchorMax = correctedDPadPos;

				Vector2 correctedDPadDotPos = new Vector2(
					(m_dpadDotPos.x / Screen.width),
					(m_dpadDotPos.y / Screen.height)
				);
				m_dPadDotRectTransform.anchorMin = correctedDPadDotPos;
				m_dPadDotRectTransform.anchorMax = correctedDPadDotPos;
			}
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
		if(!m_isFixed)
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
		else
		{
			// click has to be in the right area
 			float minX = m_initialTouchPos.x - m_radiusToCheck;
			float minY = m_initialTouchPos.y + m_radiusToCheck;
			float width = 2 * m_radiusToCheck;
				
			Vector2 mousePos = GameInput.touchPosition[0];
			if(((mousePos.x > minX) && (mousePos.x < (minX + width))) &&
				((mousePos.y < minY) && (mousePos.y > (minY - width))))
			{
				RefreshCurrentTouchPos();
				RefreshDiffVec();
				
				return true;
			}
			return false;
		}
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
			else
				touchAction = false;
		}

		return true;
	}
	
	override public bool OnTouchRelease()
	{
		RefreshDiffVec();
		return true;
	}
	
	override public void UpdateTouchControls() 
	{	
		// for fixed dpad, set initial position
		if(GameInput.m_controlMethod == ControlMethod.touch)
		{
			if(m_isFixed && !m_isInitialTouchPosSet)
			{
				m_isInitialTouchPosSet = true;
				SetInitialTouchPos();
			}
		}
		
		base.UpdateTouchControls();
	}
	
	override public void CalcSharkDesiredVelocity(float speed, bool disableDecceleration = false)
	{
		float decelerationMult = 1.0f;
		float radiusCovered = m_diffVec.magnitude;

		if( !m_decelerate )
		{
			decelerationMult = 1.0f;
		}
		else
		{
			// need to get to touch position somehow... slow down to it
			m_decelerationTimer += Time.deltaTime;
			if( (m_decelerationTimer >= m_decelerationTimeLimit) || disableDecceleration )
			{
				m_diffVecNorm.x = 0f;
				m_diffVecNorm.y = 0f;
				m_decelerate = false;
				
				m_decelerationTimer = 0.0f;
			}
			else
			{
				decelerationMult = (m_decelerationTimeLimit - m_decelerationTimer) / m_decelerationTimeLimit;
				decelerationMult = Mathf.Clamp(decelerationMult, 0.0f, 0.85f);
			}
		}
		
		// normalize the distance of the click in world units away from the shark, by the max click distance
		float speedDampenMult = 1.0f;
		speedDampenMult = radiusCovered / m_radiusToCheck;
		speedDampenMult = Mathf.Clamp(speedDampenMult, 0.0f, 1.0f);
		
		m_sharkDesiredVel.x = m_diffVecNorm.x * speed * speedDampenMult * decelerationMult;
		m_sharkDesiredVel.y = m_diffVecNorm.y * speed * speedDampenMult * decelerationMult;
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
