using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPartFollowExtended : MonoBehaviour {

	public Transform m_root;
	public Transform m_searchRoot;
	public List<string> m_partsNames;
	public List<string> m_partNamesAsignOrder;
	public float springSpeed = 1.0f;
	private class PartInfo
	{
		public Vector3 m_previousPos;
		public Vector3 m_direction;
		public float m_distance;
		public Transform m_follow;
		public Transform m_part;
		public Quaternion m_rotation;
	}

	private int m_numParts = 0;
	private PartInfo[] m_partInfos;
	private PartInfo[] m_partInfosAssignOrder;
	public Vector3 m_upDir = Vector3.up;
	private float m_startScale;
	// Use this for initialization
	void Start () 
	{
		DragonPartFollowSetup();
	}

	void DragonPartFollowSetup()
	{
		if ( m_root == null )
			m_root = transform;
		m_startScale = m_root.lossyScale.x;
		// Safe
		if ( Mathf.Abs( m_startScale ) <= Mathf.Epsilon )	
			return;

		
		Vector3 dir = Vector3.zero;

		m_numParts = m_partsNames.Count;
		m_partInfos = new PartInfo[m_numParts];
		m_partInfosAssignOrder = new PartInfo[m_numParts];

		for( int i = 0; i<m_numParts; i++ )
		{
			PartInfo newPart = new PartInfo();
			newPart.m_part = m_searchRoot.FindTransformRecursive(m_partsNames[i]);
			if (!newPart.m_part)
			{
				Debug.LogError("Cannot find "+ m_partsNames[i] );
			}

			if ( i == 0 )
			{
				newPart.m_follow = m_root;
			}
			else
			{
				newPart.m_follow = m_partInfos[i-1].m_part;
			}

			dir = newPart.m_part.position - newPart.m_follow.position;

			newPart.m_distance = dir.magnitude;
			newPart.m_direction = dir.normalized;

			newPart.m_direction = Quaternion.Inverse( newPart.m_part.rotation ) * newPart.m_direction;
			newPart.m_previousPos = newPart.m_part.position;

			m_partInfos[ i ] = newPart;

			int assignOrder = m_partNamesAsignOrder.IndexOf( m_partsNames[i]);
			if ( assignOrder >= 0 )
			{
				m_partInfosAssignOrder[ assignOrder ] = newPart;
			}
		}

	}


	void LateUpdate () 
	{
		PartInfo partInfo;
		if ( m_numParts != m_partsNames.Count )
			DragonPartFollowSetup();
		float scale = (m_root.lossyScale.x / m_startScale);

		float delta = Time.deltaTime;
		delta = Mathf.Min( delta, 1/15.0f);

		for( int i = 0; i<m_numParts; i++ )
		{
			partInfo = m_partInfos[i];
			Transform follow = partInfo.m_follow;
			Transform partTransform = partInfo.m_part;
			Vector3 dir = (partInfo.m_previousPos - follow.position).normalized;
			Vector3 wanterDir = follow.TransformDirection( partInfo.m_direction );
			Vector3 finalDir = Vector3.Slerp( dir, wanterDir, delta * springSpeed);
			partInfo.m_previousPos = follow.position + finalDir * partInfo.m_distance * scale;
			partInfo.m_rotation = follow.rotation;
			partInfo.m_rotation = partInfo.m_rotation * Quaternion.Euler(m_upDir);
		}

		for( int i = 0;i<m_numParts; i++ )
		{
			if ( m_partInfosAssignOrder[i] != null )
			{
				PartInfo pInfo = m_partInfosAssignOrder[i];
				Transform partTransform = pInfo.m_part;
				Transform follow = pInfo.m_follow;
				partTransform.position = pInfo.m_previousPos;
				partTransform.rotation = pInfo.m_rotation;

				// pInfo.m_previousPos = partTransform.position;
				// partTransform.LookAt( follow, follow.up );
				// partTransform.Rotate( m_upDir, Space.Self );
				// partTransform.position = pInfo.m_previousPos;
			}
		}
	}
}
