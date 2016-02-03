using UnityEngine;
using System.Collections;

public class DragonGrab : MonoBehaviour 
{

	GrabBehaviour m_grabbed;
	Transform m_parentTransform;
	float m_grabbingTimer;

	Transform m_claws = null;

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
			if (Physics.Linecast(transform.position, transform.position + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))) {

				flyHeight =  transform.position.y - ground.point.y;
			}

			if ( m_grabbingTimer > 2 || flyHeight > 100 )	// or is x high
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
}
