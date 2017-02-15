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
		public Texture2D texture{ 
			get { return m_texture; } 
			set { m_texture = value; } 
		}

		public void CreateTexture()
		{
			m_texture = new Texture2D(TEXTURE_SIZE,1, TextureFormat.RGBA32, false);
			m_texture.filterMode = FilterMode.Point;
			m_texture.wrapMode = TextureWrapMode.Clamp;
		}

		public void DestroyTexture( bool inmediate = false )
		{
			if (m_texture != null )
			{
				if ( inmediate )
					DestroyImmediate( m_texture );
				else
					Destroy( m_texture);	
				m_texture = null;
			}
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
	List<FogAttributes> m_generatedAttributes = new List<FogAttributes>();
	List<FogArea> m_activeFogAreaList = new List<FogArea>();
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
			if ( m_activeFogAreaList.Count > 0 )
			{
				FogArea selectedFogArea = m_activeFogAreaList.Last<FogArea>();
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


	public void ActivateArea( FogArea _area )
	{
		if ( _area.m_attributes.texture == null )
		{
			for( int i = 0; i<m_generatedAttributes.Count; i++ )
			{
				if ( Equals(m_generatedAttributes[i].m_fogGradient, _area.m_attributes.m_fogGradient) )
				{
					_area.m_attributes.m_fogGradient = m_generatedAttributes[i].m_fogGradient;
					_area.m_attributes.texture = m_generatedAttributes[i].texture;
					break;
				}

			}

			if ( _area.m_attributes.texture == null )
			{
				_area.m_attributes.CreateTexture();
				_area.m_attributes.RefreshTexture();

				FogAttributes newAttributes = new FogAttributes();
				newAttributes.m_fogGradient = _area.m_attributes.m_fogGradient;
				newAttributes.texture = _area.m_attributes.texture;
				m_generatedAttributes.Add( newAttributes );
			}
		}

		m_activeFogAreaList.Add( _area );
	}

	public bool Equals( Gradient a, Gradient b)
	{
		
		if ( a.mode == b.mode && a.alphaKeys.Length == b.alphaKeys.Length && a.colorKeys.Length == b.colorKeys.Length )
		{
			for( int i = 0; i<a.alphaKeys.Length; i++ )
			{
				if( !a.alphaKeys[i].Equals(b.alphaKeys[i]))
				{
					return false;
				}
			}

			for( int i = 0; i<a.colorKeys.Length; i++ )
			{
				if( !a.colorKeys[i].Equals(b.colorKeys[i]))
				{
					return false;
				}
			}
		}
		else
		{
			return false;
		}
		return true;


	}

	public void DeactivateArea( FogArea _area )
	{
		m_activeFogAreaList.Remove( _area );
	}

}
