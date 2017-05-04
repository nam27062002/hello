using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPartFollow : MonoBehaviour {

	
	public Transform m_root;
	public List<string> m_partsNames;
	public List<Transform> m_parts;
	public float springSpeed = 1.0f;
	private class PartInfo
	{
		public Vector3 m_previousPos;
		public Vector3 m_direction;
		public float m_distance = 0;
	}

	private List<PartInfo> m_partInfos;
	public Vector3 m_upDir = Vector3.up;
	private float m_startScale;
	// Use this for initialization
	IEnumerator Start () 
	{
		yield return null;
		if ( m_root == null )
			m_root = transform;

		m_parts = new List<Transform>();
		for (int m = 0; m < m_partsNames.Count; m++) {
			Transform tr = m_root.FindTransformRecursive(m_partsNames[m]);
			if (tr)
			{
				m_parts.Add( tr );
			}
			else
			{
				Debug.LogError("Cannot find "+ m_partsNames[m] );
			}
		}


		m_startScale = m_root.lossyScale.x;
		m_partInfos = new List<PartInfo>();
		for( int i = 0; i<m_parts.Count; i++ )
		{
			PartInfo newPart = new PartInfo();

			if ( i == 0)
			{
				Vector3 dir = (m_parts[i].position - m_root.position);
				newPart.m_distance = dir.magnitude;
				newPart.m_direction = dir.normalized;
			}
			else
			{
				Vector3 dir = (m_parts[i].position - m_parts[i-1].position);
				newPart.m_distance = dir.magnitude;
				newPart.m_direction = dir.normalized;
			}
			newPart.m_direction = Quaternion.Inverse( m_parts[i].rotation ) * newPart.m_direction;
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
		float scale = (m_root.lossyScale.x / m_startScale);
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
			Vector3 wanterDir = follow.TransformDirection( partInfo.m_direction );
			Vector3 finalDir = Vector3.Slerp( dir, wanterDir, Time.deltaTime * springSpeed);
			partTransform.position = follow.position + finalDir * partInfo.m_distance * scale;


			partTransform.LookAt( follow, follow.up );
			partTransform.Rotate( m_upDir, Space.Self );

			partInfo.m_previousPos = partTransform.position;

		}
	}
}
