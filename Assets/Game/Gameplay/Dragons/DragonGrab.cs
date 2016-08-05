using UnityEngine;
using System.Collections;

public class DragonGrab : MonoBehaviour 
{

	GrabBehaviour m_grabbed;
	Transform m_parentTransform;
	float m_grabbingTimer;

	Transform m_claws = null;
	DragonBreathBehaviour m_breath;

	// Drop if hit, or too high, or to much time, or in water
	// Once dropped whould we use phisics? does it has to explode always?

	void Awake()
	{
		m_claws = transform.FindTransformRecursive("Dragon_RLeg_4");
	}

	// Use this for initialization
	void Start () 
	{
		m_grabbed = null;
		m_breath = GetComponent<DragonBreathBehaviour>();
	}

	void OnEnable()
	{
		Messenger.AddListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	void OnDisable()
	{
		Messenger.RemoveListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	// Update is called once per frame
	void Update () 
	{
		if ( m_grabbed != null )
		{
			float lerp = Time.deltaTime * 5;
			m_grabbed.transform.position = Vector3.Lerp( m_grabbed.transform.position, m_claws.position, lerp );
			m_grabbed.transform.rotation = Quaternion.Lerp (m_grabbed.transform.rotation,m_claws.rotation*Quaternion.AngleAxis(180,Vector3.right),lerp);

			m_grabbingTimer += Time.deltaTime;

			RaycastHit ground;
			float flyHeight = 0;
			if (Physics.Linecast(transform.position, transform.position + Vector3.down * 10000f, out ground, LayerMask.GetMask("Ground", "GroundVisible"))) {

				flyHeight =  transform.position.y - ground.point.y;
			}

			if ( m_grabbingTimer > 2 || flyHeight > 100 || m_breath.IsFuryOn()  )	// or is x high
			{
				Drop();
			}
		}
	}

	public Transform GetClaws()
	{
		return m_claws;
	}

	public bool Grab( GrabBehaviour element )
	{
		if ( m_grabbed == null )
		{
			m_grabbed = element;
			m_grabbingTimer = 0;

			m_parentTransform = element.transform.parent;
			element.transform.parent = m_claws;
			// Animate claws

			return true;	
		}
		return false;
	}

	void OnDamageReceived( float damage, DamageType _type, Transform origin )
	{
		if ( IsGrabbing() )
			Drop();
	}

	public void Drop()
	{
		m_grabbed.transform.parent = m_parentTransform;
		m_grabbed.OnDrop();

		// release claws

		m_grabbed = null;
		m_parentTransform = null;
	}

	public bool IsGrabbing()
	{
		return m_grabbed != null;
	}

	public bool CanGrab()
	{
		return !IsGrabbing() && !m_breath.IsFuryOn();
	}
}
