using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPartFollow : MonoBehaviour {

	
	public Transform m_root;
	public List<Transform> m_parts;
	public float springSpeed = 1.0f;
	private class PartInfo
	{
		public Vector3 m_previousPos;
		public Quaternion m_localRotation;
		public Vector3 m_localPosition;
		public float m_distance = 0;	
	}

	private List<PartInfo> m_partInfos;
	public Vector3 m_upDir = Vector3.up;
	// Use this for initialization
	void Start () 
	{
		if ( m_root == null )
			m_root = transform;
		m_partInfos = new List<PartInfo>();
		for( int i = 0; i<m_parts.Count; i++ )
		{
			PartInfo newPart = new PartInfo();
			newPart.m_localRotation = m_parts[i].localRotation;
			newPart.m_localPosition = m_parts[i].localPosition;
			newPart.m_distance = m_parts[i].localPosition.magnitude;
			newPart.m_previousPos = m_parts[i].position;

			m_partInfos.Add( newPart );
		}

	}
	
	// Update is called once per frame
	// Ver 1
	/*
	void LateUpdate () 
	{
		for( int i = 0; i<m_parts.Count; i++ )
		{
			Transform follow;
			if ( i == 0)
			{
				follow = m_follow;
			}
			else
			{
				follow = m_parts[i-1];
			}

			PartInfo partInfo = m_partInfos[i];
			Transform partTransform = m_parts[i];

			Vector3 dir = partInfo.m_previousPos - follow.position;
			partTransform.position = follow.position + dir.normalized * partInfo.m_distance;
			partTransform.LookAt( follow.transform );

			// Adjust rotation
			partTransform.localRotation = Quaternion.Slerp( partTransform.localRotation, Quaternion.identity, Time.deltaTime * springSpeed);
			partTransform.position = follow.position - partTransform.forward * partInfo.m_distance;
			partTransform.Rotate( partInfo.m_localRotation.eulerAngles, Space.Self );
			partInfo.m_previousPos = partTransform.position;
			

		}
	}
	*/


	void LateUpdate () 
	{
		for( int i = 0; i<m_parts.Count; i++ )
		{
			Transform follow;
			if ( i == 0)
			{
				follow = m_root;
			}
			else
			{
				follow = m_parts[i-1];
			}

			PartInfo partInfo = m_partInfos[i];
			Transform partTransform = m_parts[i];


			Vector3 dir = (partInfo.m_previousPos - follow.position).normalized;
			Vector3 wanterDir = follow.TransformDirection( partInfo.m_localPosition.normalized );
			Vector3 finalDir = Vector3.Slerp( dir, wanterDir, Time.deltaTime * springSpeed);
			partTransform.position = follow.position + finalDir * partInfo.m_distance;

			partTransform.LookAt( follow, follow.up );
			partTransform.Rotate( m_upDir, Space.Self );
			// partTransform.Rotate( -partInfo.m_localRotation.eulerAngles, Space.Self );

			partInfo.m_previousPos = partTransform.position;

			/*
			// Adjust rotation
			partTransform.localRotation = Quaternion.Slerp( partTransform.localRotation, Quaternion.identity, Time.deltaTime * springSpeed);
			partTransform.position = follow.position - partTransform.forward * partInfo.m_distance;
			partTransform.Rotate( partInfo.m_localRotation.eulerAngles, Space.Self );
			partInfo.m_previousPos = partTransform.position;
			*/

		}
	}
}
