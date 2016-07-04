using UnityEngine;
using System.Collections;

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
			m_fogColor = Color.black;
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

	void Awake()
	{
		/*
		RenderSettings.fog = true;
		RenderSettings.fogColor = m_fogColor;
		RenderSettings.fogStartDistance = m_fogStart;
		RenderSettings.fogEndDistance = m_fogEnd;
		*/
		RenderSettings.fogMode = FogMode.Linear;

		// Search all Fog Nodes

	}

	void Start()
	{
		// Find all ambient nodes
		m_fogNodes = FindObjectsOfType(typeof(FogNode)) as FogNode[];
		m_ready = true;
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
			float magnitude = (position - m_fogNodes[i].transform.position).sqrMagnitude;
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
		float totalLightDistance = 0;
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				totalDistance += m_resultNodes[i].m_distance;
			}
		}

			// Inverse Values
		float totalWeight = 0;
		float totalLightWeight = 0;
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





}
