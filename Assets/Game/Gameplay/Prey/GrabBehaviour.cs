using UnityEngine;
using System.Collections;

public class GrabBehaviour : MonoBehaviour 
{
	private DragonMotion m_dragon;
	private Transform m_dragonClaws;
	private CircleArea2D m_bounds;
	private DragonGrab m_dragonGrab;
	private Rigidbody m_rigidbody;

	float m_sleepMinTimer;

	enum State
	{
		READY,
		GRABBED,
		FALLING,
		EXPLODING,
		WAITING_TO_RESPAWN
	};
	private State m_state;

	// Use this for initialization
	void Start () 
	{
		m_bounds = GetComponent<CircleArea2D>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_dragonGrab = m_dragon.GetComponent<DragonGrab>();
		m_dragonClaws = m_dragonGrab.GetClaws();
		m_state = State.READY;
		m_rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch( m_state )
		{
			case State.READY:
			{
				if (m_dragonGrab.CanGrab())
				{
					
					float distanceSqr = 100000;
					if (m_bounds != null)
					 	distanceSqr = m_bounds.DistanceSqr( m_dragonClaws.position );
					 else
						distanceSqr = (m_dragonClaws.position - transform.position).sqrMagnitude;

					if (distanceSqr <= 50) 
					{
						bool grabbed = m_dragonGrab.Grab( this );
						if ( grabbed )
						{
							ChangeState( State.GRABBED );
						}
					}
				}
			}break;
			case State.FALLING:
			{
				if (m_rigidbody != null)
				{
					if (m_rigidbody.IsSleeping() && m_sleepMinTimer <= 0 )
						ChangeState(State.READY);
					m_sleepMinTimer -= Time.deltaTime;
				}
			}break;
		}
	}

	private void ChangeState( State newState )
	{
		if ( newState != m_state )
		{
			/*
			switch( m_state )
			{

			}
			*/
			m_state = newState;
			switch( m_state )
			{
				case State.GRABBED:
				{
					if ( m_rigidbody != null )
					{
						m_rigidbody.isKinematic = true;
					}
				}break;
				case State.FALLING:
				{
					// Activate rigidbody and collider /  Activate gravity?
					if ( m_rigidbody != null )
					{
						m_rigidbody.isKinematic = false;
					}
						
					m_sleepMinTimer = 1;
				}break;
			}		
		}
	}

	public void OnDrop()
	{
		ChangeState( State.FALLING );
	}
}
