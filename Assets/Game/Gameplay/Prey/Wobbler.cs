using UnityEngine;

public class Wobbler : MonoBehaviour {

	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	private float m_wobbleDuration = 0.5f;
	[SerializeField]
	private Vector2 m_wobbleOffset = new Vector2(0.3f, 0.5f);
	[SerializeField]
	private Vector2 m_wobbleSpeed = new Vector2(20f, 30f);
	[SerializeField]
	private float m_nextWobbleTimeout = 1f;
	[SerializeField]
	private bool m_disableWhenDone = true;

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private Transform m_target;
	private Vector3 m_initialPosition;

	private bool m_isWobbling;
	private float m_wobbleStartTime;
	private Vector3 m_wobblePosition;
	private Vector2 m_timeFactor;
	private bool m_invertXWobble;
	private bool m_invertYWobble;
	private Vector2 m_initialMaxWobble;
	private Vector2 m_initialMinWobble;
	private Vector2 m_maxWobble;
	private Vector2 m_minWobble;
	private Vector2 m_timeFactorForDampen;
	private float m_deltaTime;
	private float m_timeoutStartTime;
	private Vector2 m_wobbleAcceleratedSpeed;
	
	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Update()
	{
		if(m_isWobbling)
		{
			m_deltaTime = Time.deltaTime;
			if(Time.time - m_wobbleStartTime > m_wobbleDuration)
			{
				m_isWobbling = false;
				// cache the time for the next wobbling timeout.
				m_timeoutStartTime = Time.time;
				// reset position.
				m_target.localPosition = m_initialPosition;
				// disable if requested.
				if(m_disableWhenDone)
				{
					enabled = false;
				}
				return;
			}
			// wobble the thing.
			m_wobblePosition = m_target.localPosition;
			if(m_wobbleOffset.x != 0f)
			{
				if(m_invertXWobble)
				{
					m_wobblePosition.x = Mathf.Lerp(m_minWobble.x, m_maxWobble.x, m_timeFactor.x);
				}
				else
				{
					m_wobblePosition.x = Mathf.Lerp(m_maxWobble.x, m_minWobble.x, m_timeFactor.x);
				}
			}
			if(m_wobbleOffset.y != 0f)
			{
				if(m_invertYWobble)
				{
					m_wobblePosition.y = Mathf.Lerp(m_minWobble.y, m_maxWobble.y, m_timeFactor.y);
				}
				else
				{
					m_wobblePosition.y = Mathf.Lerp(m_maxWobble.y, m_minWobble.y, m_timeFactor.y);
				}
			}
			m_target.localPosition = m_wobblePosition;
			// calculate the time factor for the next frame.
			m_timeFactor += m_deltaTime * m_wobbleAcceleratedSpeed;
			// check if a wobble cycle is completed.
			if(m_timeFactor.x > 1f)
			{
				m_timeFactor.x = 0f;
				m_invertXWobble = !m_invertXWobble;
				// dampen the wobble.
				m_minWobble.x = Mathf.Lerp(m_initialMinWobble.x, m_initialPosition.x, m_timeFactorForDampen.x);
				m_maxWobble.x = Mathf.Lerp(m_initialMaxWobble.x, m_initialPosition.x, m_timeFactorForDampen.x);
				// calculate the time factor for the next dampen.
				m_timeFactorForDampen.x = (Time.time - m_wobbleStartTime) / m_wobbleDuration;
				// accelerate the speed proportionally to the dampen factor. at the end will be twice faster.
				m_wobbleAcceleratedSpeed.x += (1f + m_timeFactorForDampen.x);
				// if there is nothing else to dampen, the wobble is done. maybe is redundant but better to clamp.
				if(m_timeFactorForDampen.x > 1f)
				{
					m_minWobble.x = m_maxWobble.x = m_initialPosition.x;
				}
			}
			// same fo the Y axis.
			if(m_timeFactor.y > 1f)
			{
				m_timeFactor.y = 0f;
				m_invertYWobble = !m_invertYWobble;
				m_minWobble.y = Mathf.Lerp(m_initialMinWobble.y, m_initialPosition.y, m_timeFactorForDampen.y);
				m_maxWobble.y = Mathf.Lerp(m_initialMaxWobble.y, m_initialPosition.y, m_timeFactorForDampen.y);
				//Debug.LogWarning("minWobble = (" + m_minWobble.x + ", " + m_minWobble.y + ")");
				//Debug.LogWarning("maxWobble = (" + m_maxWobble.x + ", " + m_maxWobble.y + ")");
				m_timeFactorForDampen.y = (Time.time - m_wobbleStartTime) / m_wobbleDuration;
				m_wobbleAcceleratedSpeed.y = m_wobbleSpeed.y * (1f + m_timeFactorForDampen.y);
				//Debug.LogWarning("timeFactorDamper = (" + m_timeFactorForDampen.x + ", " + m_timeFactorForDampen.y + ")");
				if(m_timeFactorForDampen.y > 1f)
				{
					m_minWobble.y = m_maxWobble.y = m_initialPosition.y;
				}
			}
		}
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void StartWobbling(Transform target, Vector3 initialPosition)
	{
		m_target = target;
		m_initialPosition = initialPosition;
		if(!m_isWobbling && Time.time - m_timeoutStartTime > m_nextWobbleTimeout)
		{
			m_wobbleStartTime = Time.time;
			m_timeFactor = m_timeFactorForDampen = Vector2.zero;
			m_initialMaxWobble = new Vector2(m_initialPosition.x + m_wobbleOffset.x, m_initialPosition.y + m_wobbleOffset.y);
			m_initialMinWobble = new Vector2(m_initialPosition.x - m_wobbleOffset.x, m_initialPosition.y - m_wobbleOffset.y);
			m_maxWobble = new Vector2(m_initialMaxWobble.x, m_initialMaxWobble.y);
			m_minWobble = new Vector2(m_initialMinWobble.x, m_initialMinWobble.y);
			m_wobbleAcceleratedSpeed = new Vector2(m_wobbleSpeed.x, m_wobbleSpeed.y);
			m_isWobbling = true;
		}
	}
}