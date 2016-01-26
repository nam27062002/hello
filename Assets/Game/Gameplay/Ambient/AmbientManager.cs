using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AmbientManager : MonoBehaviour 
{
	const int NODES_TO_TAKE_INTO_ACCOUNT = 2;

	public Transform m_followTransform;
	public float m_updateDistance;
	private Vector3 m_lastFollowPosition;
	public Light m_sunLight;
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

	RainController m_rainController;

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
		// Light
	Vector3 m_lightAngles;
	float m_flaresIntensity;
		// Rain
	float m_rainIntensity;

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
		// Light
	Vector3 m_targetLightAngles;
	float m_targetFlaresIntensity;
		// Rain
	float m_targetRainIntensity;

	void Awake()
	{
		RenderSettings.skybox = new Material( Resources.Load("Game/Materials/Skybox") as Material);
	}

	IEnumerator Start()
	{
		if ( Application.isPlaying )
		{
			while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
			{
				yield return null;
			}

			m_followTransform = InstanceManager.player.transform;

			// Find all ambient nodes
			m_ambientNodes = FindObjectsOfType(typeof(AmbientNode)) as AmbientNode[];
			if (m_ambientNodes.Length < 2)
			{
				enabled = false;
				// return;
			}
			else
			{
				// Version 2 - WIP
				/*
				for( int i = 0; i<m_ambientNodes.Length; i++ )
					m_ambientNodes[i].m_onEnter += SetTarget;
				*/

				GameObject sun = GameObject.Find("SunFlare");
				if (sun != null)
				{
					m_sunLight = sun.GetComponent<Light>();
				}

				// Create Rain particle over player
				GameObject go = Instantiate( Resources.Load("Particles/PF_RainParticle") ) as GameObject;
				// go.transform.parent = InstanceManager.player.transform;
				go.transform.parent = transform;
				go.transform.localPosition = Vector3.up * 8 + Vector3.forward * 22;
				m_rainController = go.GetComponent<RainController>();

				RenderSettings.fogMode = FogMode.Linear;


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
				m_lightAngles = m_targetLightAngles;
				m_flaresIntensity = m_targetFlaresIntensity;
				m_rainIntensity = m_targetRainIntensity;
				ApplyCurrentValues();
			}
		}
		m_lastFollowPosition = Vector3.one * float.MaxValue;
	}

	void Update()
	{
		// Version 1
		if (m_followTransform != null)
		{
			if ( (m_followTransform.position - m_lastFollowPosition).magnitude >= m_updateDistance || (Application.isEditor && !Application.isPlaying))
			{
				if ( Application.isEditor && !Application.isPlaying)
				{
					m_ambientNodes = FindObjectsOfType(typeof(AmbientNode)) as AmbientNode[];	
					if ( m_ambientNodes.Length == 0 )
						return;
				}

				RefreshCloserNodes();
				RefreshTargetValues();
				m_lastFollowPosition = m_followTransform.position;
			}

			float lerpValue = 0.9f * Time.deltaTime;
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
				// Light
			m_lightAngles = Vector3.Lerp( m_lightAngles, m_targetLightAngles, lerpValue );
			m_flaresIntensity = Mathf.Lerp( m_flaresIntensity, m_targetFlaresIntensity, lerpValue);
				// Rain
			m_rainIntensity = Mathf.Lerp( m_rainIntensity, m_targetRainIntensity, lerpValue);
			// Apply current Values
			ApplyCurrentValues();
		}


		// Version 2 - with box triggers WIP
		/*
		float lerpValue = 0.9f * Time.deltaTime;
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
			// Light
		m_lightAngles = Vector3.Lerp( m_lightAngles, m_targetLightAngles, lerpValue );
		m_flaresIntensity = Mathf.Lerp( m_flaresIntensity, m_targetFlaresIntensity, lerpValue);
			// Rain
		m_rainIntensity = Mathf.Lerp( m_rainIntensity, m_targetRainIntensity, lerpValue);
		// Apply current Values
		ApplyCurrentValues();
		*/

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
			m_ambientNodes[i].SetIsUsed(false);
			// find empty or farthest
			int selectedIndex = -1;
			float farthestValue = 0;
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
				m_resultNodes[selectedIndex].m_node = m_ambientNodes[i];
			}
		}

		// Now set the weigth
			// Total Distance
		float totalDistance = 0;
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				m_resultNodes[i].m_node.SetIsUsed(true);
				totalDistance += m_resultNodes[i].m_distance;
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
				m_resultNodes[i].m_weight = m_resultNodes[i].m_weight / totalWeight;
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
		// Light
		m_targetLightAngles = Vector3.zero;
		m_targetFlaresIntensity = 0;
		// Rain
		m_targetRainIntensity = 0;

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
					// Light
				m_targetLightAngles += node.transform.rotation.eulerAngles * nodeResult.m_weight;
				m_targetFlaresIntensity += node.m_flaresIntensity * nodeResult.m_weight;
					// Rain
				m_targetRainIntensity += node.m_rainIntensity * nodeResult.m_weight;
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

		if ( m_sunLight != null)
		{
			Quaternion rot = m_sunLight.transform.rotation;
			rot.eulerAngles = m_lightAngles;
			m_sunLight.transform.rotation = rot;

			m.SetVector("_SunPos", -m_sunLight.transform.forward);
		}

		RenderSettings.flareStrength = m_flaresIntensity;
		if (m_rainController != null)
		{
			m_rainController.SetIntensity( m_rainIntensity );
		}
	}

	public void SetTarget( AmbientNode node )
	{
			// Ambient
		m_targetAmbientColor = node.m_ambientColor;
		m_targetAmbientIntensity = node.m_ambientIntensity;
			// Skybox
		m_targetSunSize = node.m_sunSize;
		m_targetAtmosphereThickness = node.m_atmosphereThickness;
		m_targetSkyTint = node.m_skyTint;
		m_targetGround = node.m_ground;
		m_targetExposure = node.m_exposure;
			// Fog
		m_targetFogColor = node.m_fogColor;
		m_targetFogStart = node.m_fogStart;
		m_targetFogEnd = node.m_fogEnd;
			// Light
		m_targetLightAngles = node.transform.rotation.eulerAngles;
		m_targetFlaresIntensity = node.m_flaresIntensity;
			// Rain
		m_targetRainIntensity = node.m_rainIntensity;
	}

}
