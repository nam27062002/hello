using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FogManager : SingletonMonoBehaviour<FogManager>
{
	private FogNode[] m_fogNodes;

	public struct FogResult
	{
		public Color m_fogColor;
		public float m_fogStart;
		public float m_fogEnd;
		public void Reset()
		{
			m_fogColor = Color.clear;
			m_fogStart = 0;
			m_fogEnd = 0;
		}
	}

	struct FogNodeResult
	{
		public FogNode m_node;
		public float m_distance;
		public float m_weight;
		public void Reset()
		{
			m_node = null;
			m_distance = float.MaxValue;
			m_weight = 0;
		}
	};
	const int NODES_TO_TAKE_INTO_ACCOUNT = 2;
	FogNodeResult[] m_resultNodes = new FogNodeResult[NODES_TO_TAKE_INTO_ACCOUNT];
	private bool m_ready = false;

	void Start()
	{
		// Find all ambient nodes
		m_fogNodes = FindObjectsOfType(typeof(FogNode)) as FogNode[];
		m_ready = true;
	}

	void Update()
	{
		if ( !Application.isPlaying )	
		{
			m_fogNodes = FindObjectsOfType(typeof(FogNode)) as FogNode[];
			m_ready = true;	
		}
	}

	public bool IsReady()
	{
		return m_ready;
	}

	public FogResult GetFog( Vector3 position )
	{
		FogResult result = new FogResult();
		result.Reset();
		// Search closest fog nodes
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			m_resultNodes[i].Reset();
		}

		for( int i = 0; i<m_fogNodes.Length; i++ )
		{
			m_fogNodes[i].used = false;
			Vector2 pos = (Vector2)position;
			Vector2 nodePos = (Vector2)m_fogNodes[i].transform.position;
			// float magnitude = (position - m_fogNodes[i].transform.position).sqrMagnitude;
			float magnitude = (pos - nodePos).sqrMagnitude;
			// find empty or farthest
			int selectedIndex = -1;
			float farthestValue;

			// Ambient Values
			farthestValue = 0;
			for( int j = 0; j<NODES_TO_TAKE_INTO_ACCOUNT; j++ )
			{
				if ( m_resultNodes[j].m_node == null)
				{
					selectedIndex = j;
					break;
				}
				else if ( m_resultNodes[j].m_distance > farthestValue)
				{
					farthestValue = m_resultNodes[j].m_distance;
					selectedIndex = j;
				}
			}

			if ( selectedIndex != -1 && magnitude < m_resultNodes[selectedIndex].m_distance)
			{
				m_resultNodes[selectedIndex].m_distance = magnitude;
				m_resultNodes[selectedIndex].m_node = m_fogNodes[i];
			}
		}

		// Now set the weigth
			// Total Distance
		float totalDistance = 0;
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				totalDistance += m_resultNodes[i].m_distance;
				m_resultNodes[i].m_node.used = true;
			}
		}

			// Inverse Values
		float totalWeight = 0;
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				m_resultNodes[i].m_weight = totalDistance - m_resultNodes[i].m_distance;
				totalWeight += m_resultNodes[i].m_weight;
			}
		}

			// Normalize values
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				float w = m_resultNodes[i].m_weight / totalWeight;
				result.m_fogColor += m_resultNodes[i].m_node.m_fogColor * w;
				result.m_fogStart += m_resultNodes[i].m_node.m_fogStart * w;
				result.m_fogEnd += m_resultNodes[i].m_node.m_fogEnd * w;
			}
		}

		return result;
	}

	void OnDrawGizmos()
	{
#if UNITY_EDITOR
		// Check if fog node selected
		if ( m_fogNodes != null )
		for( int i = 0; i<m_fogNodes.Length; i++ )
		{
			if (UnityEditor.Selection.Contains( m_fogNodes[i].gameObject ) )
			{
				DrawGizmos();
				return;
			}
		}

		if ( UnityEditor.Selection.activeGameObject.GetComponent<FogSetter>() != null )
			DrawGizmos();
#endif			
	}

	void OnDrawGizmosSelected()
	{
		DrawGizmos();
	}

	void DrawGizmos()
	{
		for( int i = 0; i<m_fogNodes.Length; i++ )
			m_fogNodes[i].CustomGuizmoDraw();
	}

}
