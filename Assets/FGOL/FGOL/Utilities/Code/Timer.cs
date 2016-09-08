public struct Timer
{
	float m_current;
	float m_duration;
	System.Action m_cbFn;
	
	public Timer(float duration = -1.0f, System.Action cbFn = null)
	{
		m_current = 0.0f;
		m_duration = -1.0f;
		m_cbFn = cbFn;
	}
	
	public void Start(float duration, System.Action cbFn = null)
	{
		m_current = 0.0f;
		m_duration = duration;
		m_cbFn = cbFn;
	}
	
	public bool Update(float deltaTime)
	{
		if(m_duration > 0.0f)
		{
			m_current += deltaTime;
			if(m_current >= m_duration)
			{
				if(m_cbFn != null)
					m_cbFn();
				m_cbFn = null;
				m_duration = -1.0f;
				return true;
			}
		}
		return false;
	}
}