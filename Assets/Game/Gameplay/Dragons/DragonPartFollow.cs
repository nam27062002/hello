using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPartFollow : MonoBehaviour {

	
	public Transform m_follow;
	public List<Transform> m_parts;

	private class PartInfo
	{
		public Vector3 m_previousFollowPos;
		public Quaternion m_rotation;
		public float m_localDistance = 0;	
	}

	private List<PartInfo> m_partInfos;

	// Use this for initialization
	void Start () 
	{
		m_partInfos = new List<PartInfo>();
		for( int i = 0; i<m_parts.Count; i++ )
		{
			PartInfo newPart = new PartInfo();
			newPart.m_rotation = m_parts[i].rotation;
			newPart.m_localDistance = m_parts[i].localPosition.magnitude;
			m_partInfos.Add( newPart );
		}

	}
	
	// Update is called once per frame
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

			Vector3 diff = partInfo.m_previousFollowPos - follow.position;
			partInfo.m_previousFollowPos = follow.position;
			float lerp = Mathf.Max( diff.magnitude * 2.0f * Mathf.PI, 1.0f);
			partInfo.m_rotation = Quaternion.Lerp( partInfo.m_rotation, follow.rotation, Time.deltaTime * lerp);

			partTransform.rotation = partInfo.m_rotation;
			partTransform.position = follow.position - partTransform.forward * partInfo.m_localDistance;

		}


	}
}
