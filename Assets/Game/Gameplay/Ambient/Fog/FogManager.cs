using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FogManager : UbiBCN.SingletonMonoBehaviour<FogManager>
{
	public Texture m_fogCurveTexture;
	// private FogNode[] m_fogNodes;
	private List<FogNode> m_usedFogNodes = new List<FogNode>();
	private Rect m_getRect = new Rect();

	public enum FogMode
	{
		FogNodes,
		FogArea
	};
	public FogMode m_fogMode = FogMode.FogNodes;

	[System.Serializable]
	public class FogResult
	{
		public Color m_fogColor;
		public float m_fogStart;
		public float m_fogEnd;
		public float m_fogRamp;
		public void Reset()
		{
			m_fogColor = Color.clear;
			m_fogStart = 0;
			m_fogEnd = 0;
			m_fogRamp = 0;
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

	QuadTree<FogNode> m_fogNodes;
	FogNode[] m_fogNodesArray;

	private bool m_useQuadtree = false;


	// For Area Mode
	public FogResult m_defaultAreaFog;
	List<FogArea> m_fogAreaList = new List<FogArea>();
	float m_start;
	float m_end;
	Color m_color;
	float m_rampY;
	bool m_firstTime = true;


	FogResult m_tempResult = new FogResult();

	void Awake()
	{
		Shader.SetGlobalTexture("_FogTexture", m_fogCurveTexture);
	}

	IEnumerator Start()
	{
		if ( Application.isPlaying )
		{
			while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
			{
				yield return null;
			}
		}

		// Find all ambient nodes
		RefillQuadtree();
		m_ready = true;
	}

	void Update()
	{
		switch( m_fogMode )
		{
			case FogMode.FogNodes:
			{
				if ( !Application.isPlaying )	
				{
					RefillQuadtree();
					Shader.SetGlobalTexture("_FogTexture", m_fogCurveTexture);
					m_ready = true;	
				}
			}break;
			case FogMode.FogArea:
			{
				if ( Application.isPlaying )	
				{
					if ( m_fogAreaList.Count > 0 )
					{
						FogArea selectedFogArea = m_fogAreaList.Last<FogArea>();
						m_tempResult.m_fogStart = selectedFogArea.m_fogStart;
						m_tempResult.m_fogEnd = selectedFogArea.m_fogEnd;
						m_tempResult.m_fogColor = selectedFogArea.m_fogColor;
						m_tempResult.m_fogRamp = selectedFogArea.m_fogRamp;
					}
					else
					{
						m_tempResult.m_fogStart = m_defaultAreaFog.m_fogStart;
						m_tempResult.m_fogEnd = m_defaultAreaFog.m_fogEnd;
						m_tempResult.m_fogColor = m_defaultAreaFog.m_fogColor;
						m_tempResult.m_fogRamp = m_defaultAreaFog.m_fogRamp;
					}

					if ( m_firstTime )
					{
						m_firstTime = false;
						m_start = m_tempResult.m_fogStart;
						m_end = m_tempResult.m_fogEnd;
						m_color = m_tempResult.m_fogColor;
						m_rampY = m_tempResult.m_fogRamp;
					}
					else
					{
						float delta = Time.deltaTime;
						m_start = Mathf.Lerp( m_start, m_tempResult.m_fogStart, delta);
						m_end = Mathf.Lerp( m_end, m_tempResult.m_fogEnd, delta);
						m_color = Color.Lerp( m_color, m_tempResult.m_fogColor, delta);
						m_rampY = Mathf.Lerp( m_rampY, m_tempResult.m_fogRamp, delta);
					}
					Shader.SetGlobalFloat("_FogStart", m_start);
					Shader.SetGlobalFloat("_FogEnd", m_end);
					Shader.SetGlobalColor("_FogColor", m_color);
					Shader.SetGlobalFloat("_FogRampY", m_rampY);
				}
			}break;
		}


	}

	void RefillQuadtree()
	{
		Rect bounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelData data = LevelManager.currentLevelData;
		if(data != null) {
			bounds = data.bounds;
		}
		m_fogNodesArray = FindObjectsOfType(typeof(FogNode)) as FogNode[];
		m_fogNodes = new QuadTree<FogNode>(bounds.x, bounds.y, bounds.width, bounds.height);
		for( int i = 0; i<m_fogNodesArray.Length; i++ )
			m_fogNodes.Insert( m_fogNodesArray[i] );
	}

	public bool IsReady()
	{
		return m_ready;
	}

	public void GetFog( Vector3 position, ref FogResult result )
	{
		// FogResult result = new FogResult();
		result.Reset();
		// Search closest fog nodes
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			m_resultNodes[i].Reset();
		}

		m_usedFogNodes.Clear();

		Vector2 pos = (Vector2)position;
		FogNode[] inRangeNodes;
		if ( m_useQuadtree )
		{
			int sizeMultiplier = 1;
			float distance;
			int validNodes = 0;
			do
			{
				validNodes = 0;
				distance = 100 * sizeMultiplier;
				m_getRect.size = Vector2.one * distance;
				m_getRect.center = pos;
				inRangeNodes = m_fogNodes.GetItemsInRange( m_getRect );
				// remove nodes not in distance
				for( int i = 0; i<inRangeNodes.Length; i++ )
				{
					if ( ((Vector2)inRangeNodes[i].transform.position - pos).sqrMagnitude > (distance * distance) )
					{
						inRangeNodes[i] = null;
					}
					else
					{
						validNodes++;
					}
				}
				sizeMultiplier++;
			}while( validNodes < NODES_TO_TAKE_INTO_ACCOUNT );
		}
		else
		{
			inRangeNodes = m_fogNodesArray;
		}
		for( int i = 0; i<inRangeNodes.Length; i++ )
		{
			if ( inRangeNodes[i] == null )
				continue;

			Vector2 nodePos = (Vector2)inRangeNodes[i].transform.position;
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
				m_resultNodes[selectedIndex].m_node = inRangeNodes[i];
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
				m_usedFogNodes.Add(m_resultNodes[i].m_node);
			}
		}

			// Inverse Values
		float totalWeight = 0;
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				float w = Mathf.Pow(totalDistance - m_resultNodes[i].m_distance, NODES_TO_TAKE_INTO_ACCOUNT);
				m_resultNodes[i].m_weight = w;
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
				result.m_fogRamp += m_resultNodes[i].m_node.m_fogRamp * w;
			}
		}
	}

	void OnDrawGizmos()
	{
#if UNITY_EDITOR
		// Check if fog node selected
		if ( m_fogNodesArray != null )
		for( int i = 0; i<m_fogNodesArray.Length; i++ )
		{
			if (UnityEditor.Selection.Contains( m_fogNodesArray[i].gameObject ) )
			{
				DrawGizmos();
				return;
			}
		}

		if ( UnityEditor.Selection.activeGameObject != null && UnityEditor.Selection.activeGameObject.GetComponent<FogSetter>() != null )
			DrawGizmos();
#endif			
	}

	void OnDrawGizmosSelected()
	{
		DrawGizmos();
	}

	void DrawGizmos()
	{
		for( int i = 0; i<m_fogNodesArray.Length; i++ )
			m_fogNodesArray[i].CustomGuizmoDraw( m_usedFogNodes.Contains( m_fogNodesArray[i] ) );
	}


	public void RegisterFog( FogArea _area )
	{
		m_fogAreaList.Add( _area );
	}


	public void UnregisterFog( FogArea _area )
	{
		m_fogAreaList.Remove( _area );
	}

}
