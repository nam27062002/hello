using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPartFollow : MonoBehaviour {

    private struct PartInfo
    {
        public Vector3 m_previousPos;
        public Vector3 m_direction;
        public float m_distance;
    }

    [SerializeField]
    private Transform m_root;

    [SerializeField]
    private List<string> m_partsNames;

    [SerializeField]
    public float springSpeed = 1.0f;

    [SerializeField]
    private Vector3 m_upDir = Vector3.up;

    [SerializeField]
    public bool m_disableOnResults = false;


    [Tooltip("If enabled, the dragon parts movement is not affected by the position of the dragon in the space. Use it for the shop.")]
    [SerializeField]
    private bool m_useLocalTransforms = false;

    [Tooltip("If using local transforms, take this object as parent reference")]
    [SerializeField]
    private Transform m_rootLocalReference;


    private int m_numParts = 0;
	private PartInfo[] m_partInfos;
	private Transform[] m_parts;
	private float m_startScale;


    // Cached values
    private Vector3 m_parentPos;

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

		PartInfo newPart = new PartInfo();
		Vector3 dir = Vector3.zero;

		m_numParts = m_partsNames.Count;
		m_parts = new Transform[m_numParts];
		m_partInfos = new PartInfo[m_numParts];

        // Keep a reference to the parent transform of the dragon
        if (m_useLocalTransforms && m_rootLocalReference != null)
        {
            m_parentPos = m_rootLocalReference.position;
        }


        for ( int i = 0; i<m_numParts; i++ )
		{
			Transform part = m_root.FindTransformRecursive(m_partsNames[i]);
			m_parts[i] = part;
			if (!part)
			{
				Debug.LogError("Cannot find "+ m_partsNames[i] );
			}

			if ( i == 0 )
			{
				dir = (part.position - m_root.position);
			}
			else
			{
				dir = (part.position - m_parts[i-1].position);
			}
			newPart.m_distance = dir.magnitude;
			newPart.m_direction = dir.normalized;

			newPart.m_direction = Quaternion.Inverse( part.rotation ) * newPart.m_direction;
			newPart.m_previousPos = part.position - m_parentPos;

			m_partInfos[ i ] = newPart;
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


        if (m_useLocalTransforms && m_rootLocalReference != null)
        {
            // Update the parent position
            m_parentPos = m_rootLocalReference.position;
        }
        // else: m_parentPos will remain (0,0,0)

        for ( int i = 0; i<m_numParts; i++ )
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

            /*
			fixed (PartInfo* partInfo = &m_partInfos[i])
			{
				Transform partTransform = m_parts[i];
				Vector3 dir = (partInfo->m_previousPos - follow.position).normalized;
				Vector3 wanterDir = follow.TransformDirection( partInfo->m_direction );
				Vector3 finalDir = Vector3.Slerp( dir, wanterDir, Time.deltaTime * springSpeed);
				partTransform.position = follow.position + finalDir * partInfo->m_distance * scale;

				partTransform.LookAt( follow, follow.up );
				partTransform.Rotate( m_upDir, Space.Self );

				partInfo->m_previousPos = partTransform.position;
			}
			*/



            partInfo = m_partInfos[i];
			Transform partTransform = m_parts[i];
			Vector3 dir = (partInfo.m_previousPos + m_parentPos - follow.position).normalized;
			Vector3 wanterDir = follow.TransformDirection( partInfo.m_direction );
			Vector3 finalDir = Vector3.Lerp( dir, wanterDir, delta * springSpeed);
			partTransform.position = follow.position + finalDir * partInfo.m_distance * scale;

			partTransform.LookAt( follow, follow.up );
			partTransform.Rotate( m_upDir, Space.Self );

			partInfo.m_previousPos = partTransform.position - m_parentPos;
			m_partInfos[i] = partInfo;
		}
	}
}
