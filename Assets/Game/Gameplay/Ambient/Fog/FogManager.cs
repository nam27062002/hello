using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FogManager : UbiBCN.SingletonMonoBehaviour<FogManager>
{
	// private FogNode[] m_fogNodes;
	// private List<FogNode> m_usedFogNodes = new List<FogNode>();
	// private Rect m_getRect = new Rect();

	[System.Serializable]
	public class FogAttributes
	{
		public const int TEXTURE_SIZE = 128;

		public Gradient m_fogGradient;
		public float m_fogStart;
		public float m_fogEnd;
		private Texture2D m_texture;
		public Texture2D texture{ get { return m_texture; } }

		public void CreateTexture()
		{
			m_texture = new Texture2D(TEXTURE_SIZE,1, TextureFormat.RGBA32, false);
			m_texture.filterMode = FilterMode.Point;
			m_texture.wrapMode = TextureWrapMode.Clamp;
		}

		public void RefreshTexture()
		{
			for( int i = 0; i<TEXTURE_SIZE; i++ )
			{
				m_texture.SetPixel(i, 0, m_fogGradient.Evaluate( i / (float)TEXTURE_SIZE));
			}
			m_texture.Apply();
		}

	}


	private bool m_ready = false;

	// For Area Mode
	public FogAttributes m_defaultAreaFog;

	// Runtime variables
	List<FogArea> m_fogAreaList = new List<FogArea>();
	float m_start;
	float m_end;
	Texture2D m_texture;
	bool m_firstTime = true;

	float m_tmpStart;
	float m_tmpEnd;
	Texture2D m_tmpTexture;

	void Awake()
	{
		m_texture = new Texture2D( FogAttributes.TEXTURE_SIZE,1, TextureFormat.RGBA32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.wrapMode = TextureWrapMode.Clamp;

		m_defaultAreaFog.CreateTexture();
		m_defaultAreaFog.RefreshTexture();
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
		// RefillQuadtree();
		m_ready = true;
	}

	void Update()
	{
		if ( Application.isPlaying )	
		{
			if ( m_fogAreaList.Count > 0 )
			{
				FogArea selectedFogArea = m_fogAreaList.Last<FogArea>();
				m_tmpStart = selectedFogArea.m_attributes.m_fogStart;
				m_tmpEnd = selectedFogArea.m_attributes.m_fogEnd;
				m_tmpTexture = selectedFogArea.m_attributes.texture;
			}
			else
			{
				m_tmpStart = m_defaultAreaFog.m_fogStart;
				m_tmpEnd = m_defaultAreaFog.m_fogEnd;
				m_tmpTexture = m_defaultAreaFog.texture;
			}

			if ( m_firstTime )
			{
				m_firstTime = false;
				m_start = m_tmpStart;
				m_end = m_tmpEnd;
				for( int i = 0; i<FogAttributes.TEXTURE_SIZE; i++ )
					m_texture.SetPixel(i, 0, m_tmpTexture.GetPixel(i,0));
			}
			else
			{
				float delta = Time.deltaTime;
				m_start = Mathf.Lerp( m_start, m_tmpStart, delta);
				m_end = Mathf.Lerp( m_end, m_tmpEnd, delta);
				for( int i = 0; i<FogAttributes.TEXTURE_SIZE; i++ )
				{
					Color c = Color.Lerp( m_texture.GetPixel(i,0), m_tmpTexture.GetPixel(i,0), delta);
					m_texture.SetPixel( i, 0, c);
				}
			}
			m_texture.Apply();
			Shader.SetGlobalFloat("_FogStart", m_start);
			Shader.SetGlobalFloat("_FogEnd", m_end);
			Shader.SetGlobalTexture("_FogTexture", m_texture);
			// Shader.SetGlobalColor("_FogColor", m_color);
			// Shader.SetGlobalFloat("_FogRampY", m_rampY);
		}
			

	}

	public bool IsReady()
	{
		return m_ready;
	}

	/*
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
	*/

	public void RegisterFog( FogArea _area )
	{
		m_fogAreaList.Add( _area );
	}


	public void UnregisterFog( FogArea _area )
	{
		m_fogAreaList.Remove( _area );
	}

}
