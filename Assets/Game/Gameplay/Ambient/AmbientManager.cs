using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AmbientManager : MonoBehaviour 
{
	const int NODES_TO_TAKE_INTO_ACCOUNT = 3;

	public Transform m_followTransform;
	public float m_updateDistance;
	private Vector3 m_lastFollowPosition;

	public AmbientNode[] m_ambientNodes;

	struct AmbientNodeResults
	{
		public AmbientNode m_node;
		public float m_distance;
		public float m_weight;
		public void Reset()
		{
			m_node = null;
			m_distance = float.MaxValue;
			m_weight = 0;
		}
	};
	AmbientNodeResults[] m_resultNodes = new AmbientNodeResults[NODES_TO_TAKE_INTO_ACCOUNT];


	// CurrentValues
		// Ambient
	Color m_ambientColor = Color.white;
	float m_ambientIntensity = 0;
		// Skybox
	float m_sunSize;
	float m_atmosphereThickness;
	Color m_skyTint = Color.white;
	Color m_ground = Color.white;
	float m_exposure;
		// Fog
	Color m_fogColor;
	float m_fogStart;
	float m_fogEnd;

	// Target values
	Color emptyColor = new Color(0,0,0,0);
		// Ambient
	Color m_targetAmbientColor = Color.white;
	float m_targetAmbientIntensity = 0;
		// Skybox
	float m_targetSunSize;
	float m_targetAtmosphereThickness;
	Color m_targetSkyTint = Color.white;
	Color m_targetGround = Color.white;
	float m_targetExposure;
		// Fog
	Color m_targetFogColor;
	float m_targetFogStart;
	float m_targetFogEnd;

	void Start()
	{
		if ( Application.isPlaying )
		{
			m_followTransform = InstanceManager.player.transform;

			// Find all ambient nodes
			m_ambientNodes = FindObjectsOfType(typeof(AmbientNode)) as AmbientNode[];

			// Get Closer and start from there
			RefreshCloserNodes();
			RefreshTargetValues();

			// CurrentValues
			m_ambientColor = m_targetAmbientColor;
			m_ambientIntensity = m_targetAmbientIntensity;
			m_sunSize = m_targetSunSize;
			m_atmosphereThickness = m_targetAtmosphereThickness;
			m_skyTint = m_targetSkyTint;
			m_ground = m_targetGround;
			m_exposure = m_targetExposure;
			m_fogColor = m_targetFogColor;
			m_fogStart = m_targetFogStart;
			m_fogEnd = m_targetFogEnd;
			ApplyCurrentValues();
		}
		m_lastFollowPosition = Vector3.one * float.MaxValue;
	}

	void Update()
	{
		if (m_followTransform != null)
		{
			if ( (m_followTransform.position - m_lastFollowPosition).magnitude >= m_updateDistance )
			{
				if ( Application.isEditor && !Application.isPlaying)
				{
					m_ambientNodes = FindObjectsOfType(typeof(AmbientNode)) as AmbientNode[];	
				}

				RefreshCloserNodes();
				RefreshTargetValues();
				m_lastFollowPosition = m_followTransform.position;
			}

			float lerpValue = 0.9f;
			// Lerp current values
				// Ambient
			m_ambientColor =  Color.Lerp( m_ambientColor, m_targetAmbientColor, lerpValue);
			m_ambientIntensity = Mathf.Lerp( m_ambientIntensity , m_targetAmbientIntensity, lerpValue);
				// Skybox
			m_sunSize = Mathf.Lerp( m_sunSize, m_targetSunSize, lerpValue);
			m_atmosphereThickness =  Mathf.Lerp( m_atmosphereThickness, m_targetAtmosphereThickness, lerpValue);
			m_skyTint = Color.Lerp( m_skyTint, m_targetSkyTint, lerpValue);
			m_ground = Color.Lerp( m_ground, m_targetGround, lerpValue);
			m_exposure = Mathf.Lerp( m_exposure, m_targetExposure, lerpValue);
				// Fog
			m_fogColor = Color.Lerp( m_fogColor, m_targetFogColor, lerpValue);
			m_fogStart = Mathf.Lerp( m_fogStart, m_targetFogStart, lerpValue);
			m_fogEnd = Mathf.Lerp( m_fogEnd, m_targetFogEnd, lerpValue);

			// Apply current Values
			ApplyCurrentValues();


		}
	}


	void RefreshCloserNodes()
	{
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			m_resultNodes[i].Reset();
		}

		for( int i = 0; i<m_ambientNodes.Length; i++ )
		{
			float magnitude = (m_followTransform.position - m_ambientNodes[i].transform.position).sqrMagnitude;
			int selectedIndex = -1;
			float previousDifference = -1;
			// Seach this magnitude is smaller than what we have
			// NOTE: We could use a sorted list
			for( int j = 0; j<NODES_TO_TAKE_INTO_ACCOUNT; j++ )
			{
				if ( m_resultNodes[j].m_node == null)
				{
					selectedIndex = j;
					break;
				}
				else if (magnitude < m_resultNodes[j].m_distance && (previousDifference < 0 || (m_resultNodes[j].m_distance - magnitude) < previousDifference))
				{
					selectedIndex = j;
					previousDifference = m_resultNodes[j].m_distance - magnitude;
				}
			}

			if ( selectedIndex != -1 )
			{
				m_resultNodes[selectedIndex].m_distance = magnitude;
				m_resultNodes[selectedIndex].m_node = m_ambientNodes[i];
			}
		}

		float totalDistance = 0;

		// Now set the weigth
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				totalDistance += m_resultNodes[i].m_distance;
			}
		}

		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				m_resultNodes[i].m_weight = (totalDistance - m_resultNodes[i].m_distance) / totalDistance;
			}
		}
	}

	void RefreshTargetValues()
	{
		// Ambient
		m_targetAmbientColor = emptyColor;
		m_targetAmbientIntensity = 0;
		// Skybox
		m_targetSunSize = 0;
		m_targetAtmosphereThickness = 0;
		m_targetSkyTint = emptyColor;
		m_targetGround = emptyColor;
		m_targetExposure = 0;
		// Fog
		m_targetFogColor = emptyColor;
		m_targetFogStart = 0;
		m_targetFogEnd = 0;


		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				AmbientNodeResults nodeResult = m_resultNodes[i];
				AmbientNode node = nodeResult.m_node;
					// Ambient
				m_targetAmbientColor += node.m_ambientColor * nodeResult.m_weight;
				m_targetAmbientIntensity += node.m_ambientIntensity * nodeResult.m_weight;
					// Skybox
				m_targetSunSize += node.m_sunSize * nodeResult.m_weight;
				m_targetAtmosphereThickness += node.m_atmosphereThickness * nodeResult.m_weight;
				m_targetSkyTint += node.m_skyTint * nodeResult.m_weight;
				m_targetGround += node.m_ground * nodeResult.m_weight;
				m_targetExposure += node.m_exposure * nodeResult.m_weight;
					// Fog
				m_targetFogColor += node.m_fogColor * nodeResult.m_weight;
				m_targetFogStart += node.m_fogStart * nodeResult.m_weight;
				m_targetFogEnd += node.m_fogEnd * nodeResult.m_weight;

			}
		}
	}

	void ApplyCurrentValues()
	{
		RenderSettings.ambientLight = m_ambientColor;
		RenderSettings.ambientIntensity = m_ambientIntensity;

		Material m = RenderSettings.skybox;
		m.SetFloat("_SunSize", m_sunSize);
		m.SetFloat("_AtmosphereThickness", m_atmosphereThickness);
		m.SetColor("_SkyTint", m_skyTint);
		m.SetColor("_GroundColor", m_ground);
		m.SetFloat("_Exposure", m_exposure);

		RenderSettings.fog = true;
		RenderSettings.fogColor = m_fogColor;
		RenderSettings.fogStartDistance = m_fogStart;
		RenderSettings.fogEndDistance = m_fogEnd;
	}

}
