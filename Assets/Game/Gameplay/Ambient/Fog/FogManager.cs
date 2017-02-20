﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FogManager : MonoBehaviour
{

	private int m_maxGradientTextures = 64;

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
			m_texture.filterMode = FilterMode.Bilinear;
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
			m_texture.Apply(false);
		}
	}


	private bool m_ready = false;

	public enum FogBlendMode
	{
		TEXTURE_APPLY,
		BLIT
	};
	public FogBlendMode m_fogBlendMode;
	private FogBlendMode m_lastBlendMode;

	// For Area Mode
	public FogAttributes m_defaultAreaFog;

	// Runtime variables
	List<FogAttributes> m_generatedAttributes = new List<FogAttributes>();
	List<FogArea> m_activeFogAreaList = new List<FogArea>();

	float m_start;
	float m_end;
	Texture2D m_texture;

	float m_tmpStart;
	float m_tmpEnd;
	Texture2D m_tmpTexture;

	bool m_firstTime = true;

	float m_transitionTimer = 0;
	FogAttributes m_lastSelectedAttributes;
	FogAttributes m_selectedAttributes = null;
	bool m_active = false;
	bool m_updateValues = true;

	// Variables for second version
	private Material m_fogBlendMaterial;
	private RenderTexture m_blitOrigin;
	private RenderTexture m_blitDestination;
	private float m_blitLerpValue = 0;
	bool m_updateBlitOriginTexture = false;

	void Awake()
	{
		InstanceManager.fogManager = this;

		m_texture = new Texture2D( FogAttributes.TEXTURE_SIZE,1, TextureFormat.RGBA32, false);
		m_texture.filterMode = FilterMode.Bilinear;
		m_texture.wrapMode = TextureWrapMode.Clamp;

		m_tmpTexture = new Texture2D( FogAttributes.TEXTURE_SIZE,1, TextureFormat.RGBA32, false);
		m_tmpTexture.filterMode = FilterMode.Bilinear;
		m_tmpTexture.wrapMode = TextureWrapMode.Clamp;

		m_blitDestination = new RenderTexture( FogAttributes.TEXTURE_SIZE, 1, 0);
		m_blitDestination.filterMode = FilterMode.Bilinear;
		m_blitDestination.wrapMode = TextureWrapMode.Clamp;

		m_blitOrigin = new RenderTexture( FogAttributes.TEXTURE_SIZE, 1, 0);
		m_blitOrigin.filterMode = FilterMode.Bilinear;
		m_blitOrigin.wrapMode = TextureWrapMode.Clamp;

		Shader s = Shader.Find("Hidden/FogBlend");
		m_fogBlendMaterial = new Material(s);
		m_fogBlendMaterial.SetTexture("_OriginalTex", m_blitOrigin);

			// Register default attributes
		CheckTextureAvailability( m_defaultAreaFog );

		m_active = Prefs.GetBoolPlayer(DebugSettings.FOG_MANAGER, true);
		Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);
		Messenger.AddListener<string>(GameEvents.CP_PREF_CHANGED, Debug_OnChangedString);

		m_fogBlendMode = (FogBlendMode) Prefs.GetIntPlayer( DebugSettings.FOG_BLEND_TYPE, 0);
		OnModeChanged();

		if ( !Application.isPlaying )
		{
			RefreshFog();
		}
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

	void OnDestroy()
	{
		InstanceManager.fogManager = null;
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);
		Messenger.RemoveListener<string>(GameEvents.CP_PREF_CHANGED, Debug_OnChangedString);
	}

	void Debug_OnChanged( string _key, bool value)
	{
		if ( _key == DebugSettings.FOG_MANAGER )
			m_active = value;
	}

	void Debug_OnChangedString( string _key )
	{
		if ( _key == DebugSettings.FOG_BLEND_TYPE )
		{
			m_fogBlendMode = (FogBlendMode) Prefs.GetIntPlayer(_key, 0);
		}
	}

	void Update()
	{
		if ( Application.isPlaying && m_active)
		{
			if ( m_fogBlendMode != m_lastBlendMode )
				OnModeChanged();

			if ( m_activeFogAreaList.Count > 0 )
			{
				FogArea selectedFogArea = m_activeFogAreaList.Last<FogArea>();
				m_selectedAttributes = selectedFogArea.m_attributes;
			}
			else
			{
				m_selectedAttributes = m_defaultAreaFog;
			}

			if (m_lastSelectedAttributes != m_selectedAttributes)
			{
				m_lastSelectedAttributes = m_selectedAttributes;
				m_transitionTimer = 1.0f;

				// Copy destination render texture to original texture
				m_updateBlitOriginTexture = true;
			}

			switch( m_fogBlendMode )
			{
				case FogBlendMode.TEXTURE_APPLY:
				{
					UpdateApplyMode();
				}break;
				case FogBlendMode.BLIT:
				{
					UpdateBlitMode();
				}break;
			}
		}
	}

	void UpdateApplyMode()
	{
		m_updateValues = false;
		if ( m_firstTime )
		{
			m_firstTime = false;
			m_updateValues = true;
			SetAsSelectedAttributes();
		}
		else
		{
			if ( m_transitionTimer > 0 )
			{
				m_updateValues = true;
				m_transitionTimer -= Time.deltaTime;
				if ( m_transitionTimer <= 0 )
				{
					SetAsSelectedAttributes();
				}
				else
				{
					float delta = 1.0f - m_transitionTimer;
					m_start = Mathf.Lerp( m_tmpStart, m_selectedAttributes.m_fogStart, delta);
					m_end = Mathf.Lerp( m_tmpEnd, m_selectedAttributes.m_fogEnd, delta);
					for( int i = 0; i<FogAttributes.TEXTURE_SIZE; i++ )
					{
						Color c = Color.Lerp( m_tmpTexture.GetPixel(i,0), m_selectedAttributes.texture.GetPixel(i,0), delta);
						m_texture.SetPixel( i, 0, c);
					}
				}
			}
		}

		if ( m_updateValues )
		{
			m_texture.Apply(false);
			Shader.SetGlobalFloat("_FogStart", m_start);
			Shader.SetGlobalFloat("_FogEnd", m_end);
			Shader.SetGlobalTexture("_FogTexture", m_texture);
		}
	}

	private void SetAsSelectedAttributes()
	{
		m_start = m_tmpStart = m_selectedAttributes.m_fogStart;
		m_end = m_tmpEnd = m_selectedAttributes.m_fogEnd;
		for( int i = 0; i<FogAttributes.TEXTURE_SIZE; i++ )
			m_texture.SetPixel(i, 0, m_selectedAttributes.texture.GetPixel(i,0));
		for( int i = 0; i<FogAttributes.TEXTURE_SIZE; i++ )
			m_tmpTexture.SetPixel(i, 0, m_selectedAttributes.texture.GetPixel(i,0));
	}

	void UpdateBlitMode()
	{
		if ( Application.isPlaying && m_active && m_fogBlendMode == FogBlendMode.BLIT )
		{
			m_updateValues = false;
			if ( m_firstTime )
			{
				m_firstTime = false;
				m_updateValues = true;
				EndBlitBlending();
			}
			else
			{
				if ( m_transitionTimer > 0 )
				{
					m_updateValues = true;
					m_transitionTimer -= Time.deltaTime;
					if ( m_transitionTimer <= 0 )
					{
						EndBlitBlending();
					}
					else
					{
						float delta = 1.0f - m_transitionTimer;
						m_start = Mathf.Lerp( m_tmpStart, m_selectedAttributes.m_fogStart, delta);
						m_end = Mathf.Lerp( m_tmpEnd, m_selectedAttributes.m_fogEnd, delta);
						m_blitLerpValue = delta;
					}
				}
			}
		}
	}

	void OnPreRender()
	{
		if ( Application.isPlaying && m_active && m_fogBlendMode == FogBlendMode.BLIT )
		{
			if ( m_updateValues )
			{
				Shader.SetGlobalFloat("_FogStart", m_start);
				Shader.SetGlobalFloat("_FogEnd", m_end);
				m_fogBlendMaterial.SetFloat("_LerpValue" , m_blitLerpValue);
				if (m_updateBlitOriginTexture)
				{
					Graphics.Blit(m_blitDestination, m_blitOrigin);
				}
				Graphics.Blit( m_selectedAttributes.texture, m_blitDestination, m_fogBlendMaterial);

			}
		}
	}

	private void EndBlitBlending()
	{
		m_start = m_tmpStart = m_selectedAttributes.m_fogStart;
		m_end = m_tmpEnd = m_selectedAttributes.m_fogEnd;
		m_blitLerpValue = 1;
	}

	public bool IsReady()
	{
		return m_ready;
	}


	public void ActivateArea( FogArea _area )
	{
		CheckTextureAvailability( _area.m_attributes );
		m_activeFogAreaList.Add( _area );
	}

	public void CheckTextureAvailability( FogManager.FogAttributes _attributes)
	{
		if ( _attributes.texture == null )
		{
			for( int i = 0; i<m_generatedAttributes.Count; i++ )
			{
				if ( Equals(m_generatedAttributes[i].m_fogGradient, _attributes.m_fogGradient) )
				{
					_attributes.m_fogGradient = m_generatedAttributes[i].m_fogGradient;
					_attributes.texture = m_generatedAttributes[i].texture;
					break;
				}

			}

			if ( _attributes.texture == null )
			{
				_attributes.CreateTexture();
				_attributes.RefreshTexture();

				FogAttributes newAttributes = new FogAttributes();
				newAttributes.m_fogGradient = _attributes.m_fogGradient;
				newAttributes.texture = _attributes.texture;
				m_generatedAttributes.Add( newAttributes );

				if ( UnityEngine.Debug.isDebugBuild && m_generatedAttributes.Count >= m_maxGradientTextures)
				{
					Debug.TaggedLogWarning("Fog Manager", "To many gradient textures");
				}
			}
		}
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


	void OnModeChanged()
	{
		switch( m_fogBlendMode )
		{
			case FogBlendMode.TEXTURE_APPLY:
			{
			}break;
			case FogBlendMode.BLIT:
			{
				Shader.SetGlobalTexture("_FogTexture", m_blitDestination);
			}break;
		}
		m_lastBlendMode = m_fogBlendMode;
	}

	void OnDrawGizmosSelected()
	{
		
			RefreshFog();
	}

	void RefreshFog()
	{
		if ( m_defaultAreaFog.texture == null )
			m_defaultAreaFog.CreateTexture();
		m_defaultAreaFog.RefreshTexture();

		if (!Application.isPlaying )
		{
			Shader.SetGlobalFloat("_FogStart", m_defaultAreaFog.m_fogStart);
			Shader.SetGlobalFloat("_FogEnd", m_defaultAreaFog.m_fogEnd);
			Shader.SetGlobalTexture("_FogTexture", m_defaultAreaFog.texture);
		}
	}
}
