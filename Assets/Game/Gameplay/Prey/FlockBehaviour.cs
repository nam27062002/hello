using UnityEngine;
using System.Collections;

public class FlockBehaviour : MonoBehaviour {



	// --------------------------------------------------------------- //
	private FlockController m_flock; // turn into flock controller
	private PreyMotion m_motion;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {
		m_motion = GetComponent<PreyMotion>();
	}

	void OnDisable() 
	{
		SetFlock(null);
	}

	public void SetFlock( FlockController _flock)
	{
		m_flock = _flock;
	}

	public Vector2 GetFlockTarget() 
	{
		if ( m_flock != null )
		{
			return m_flock.GetTarget();
		}
		else
		{
			if (m_motion.area != null)
				return m_motion.area.RandomInside();
		}
		return transform.position;
	}
}
