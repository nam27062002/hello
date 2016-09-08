public class Tween<T>
{
	T m_start;
	T m_end;
	float m_current = 0.0f;
	float m_duration = 0.0f;
	
	bool m_isTweening = false;

	System.Func<T, T, float, T> m_tweenFunc;

	public Tween(System.Func<T, T, float, T> tweenFunc)
	{
		m_tweenFunc = tweenFunc;
	}

	public void Begin(T s, T e, float duration)
	{
		m_start = s;
		m_end = e;
		m_duration = duration;
		m_current = 0.0f;
		m_isTweening = true;
	}
	
	public bool Update(float ds, out T val)
	{
		val = default(T);
		if(m_isTweening)
		{
			m_current += ds;
			if(m_current >= m_duration)
			{
				m_current = m_duration;
				m_isTweening = false;
			}
			val = m_tweenFunc(m_start, m_end, m_current/m_duration);
		}
		return !m_isTweening;
	}

	public void Reset()
	{
		m_start = default(T);
		m_end = default(T);
		m_duration = 0.0f;
		m_current = 0.0f;
		m_isTweening = false;
	}

	public bool isTweening { get { return m_isTweening; } }
	public T start { get { return m_start; } }
	public T end { get { return m_end; } }
	public float duration { get { return m_duration; } }
}