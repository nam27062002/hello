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
	// AmbientNodeResults[] m_resultLightDir = new AmbientNodeResults[NODES_TO_TAKE_INTO_ACCOUNT];

	RainController m_rainController;

	// CurrentValues
		// Ambient
	Color m_ambientColor = Color.white;
	float m_ambientIntensity = 0;
		// Skybox
	float m_sunSize;
	Color m_skyColor = Color.white;
	Color m_horizonColor = Color.white;
	float m_horizonHeigth;
	Color m_groundColor = Color.white;
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
	Color m_targetSkyColor = Color.white;
	Color m_targetHorizonColor = Color.white;
	float m_targetHorizonHeight;
	Color m_targetGroundColor = Color.white;
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
			while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
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
				GameObject go = Instantiate( Resources.Load("Particles/Master/PF_RainParticle") ) as GameObject;
				// go.transform.parent = InstanceManager.player.transform;
				go.transform.parent = transform;
				go.transform.localPosition = Vector3.up * 8 + Vector3.forward * 22;
				m_rainController = go.GetComponent<RainController>();

				// Create Ember particle over player
				go = Instantiate( Resources.Load("Particles/Master/PF_FlyingEmber") ) as GameObject;
				go.transform.parent = transform;
				go.transform.localPosition = Vector3.forward * 50;


				RenderSettings.fogMode = FogMode.Linear;


				// Get Closer and start from there
				RefreshCloserNodes();
				RefreshTargetValues();

				// CurrentValues
				m_ambientColor = m_targetAmbientColor;
				m_ambientIntensity = m_targetAmbientIntensity;
				m_sunSize = m_targetSunSize;
				m_skyColor = m_targetSkyColor;
				m_groundColor = m_targetGroundColor;
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
			float lightDirLerp = 0.9f * Time.deltaTime;
			if (!Application.isPlaying)
			{
				lerpValue = 1;
				lightDirLerp = 1;
			}
			// Lerp current values
				// Ambient
			m_ambientColor =  Color.Lerp( m_ambientColor, m_targetAmbientColor, lerpValue);
			m_ambientIntensity = Mathf.Lerp( m_ambientIntensity , m_targetAmbientIntensity, lerpValue);
				// Skybox
			m_sunSize = Mathf.Lerp( m_sunSize, m_targetSunSize, lightDirLerp);
			m_skyColor = Color.Lerp( m_skyColor, m_targetSkyColor, lerpValue);
			m_horizonColor = Color.Lerp( m_horizonColor, m_targetHorizonColor, lerpValue);
			m_horizonHeigth = Mathf.Lerp( m_horizonHeigth, m_targetHorizonHeight, lerpValue );
			m_groundColor = Color.Lerp( m_groundColor, m_targetGroundColor, lerpValue);
				// Light
			m_lightAngles = Vector3.Lerp( m_lightAngles, m_targetLightAngles, lightDirLerp );
			m_flaresIntensity = Mathf.Lerp( m_flaresIntensity, m_targetFlaresIntensity, lightDirLerp);
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
		m_skyTint = Color.Lerp( m_skyTint, m_targetSkyTint, lerpValue);
		m_ground = Color.Lerp( m_ground, m_targetGround, lerpValue);
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
			// m_resultLightDir[i].Reset();
		}

		for( int i = 0; i<m_ambientNodes.Length; i++ )
		{
			float magnitude = (m_followTransform.position - m_ambientNodes[i].transform.position).sqrMagnitude;
			m_ambientNodes[i].SetIsUsed(false);
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
				m_resultNodes[selectedIndex].m_node = m_ambientNodes[i];
			}

			// Light Dir Values
			/*
			if ( m_ambientNodes[i].m_useLightDir )
			{
				int selectedLightIndex = -1;
				farthestValue = 0;
				for( int j = 0; j<NODES_TO_TAKE_INTO_ACCOUNT; j++ )
				{
					if ( m_resultLightDir[j].m_node == null)
					{
						selectedLightIndex = j;
						break;
					}
					else if ( m_resultLightDir[j].m_distance > farthestValue)
					{
						farthestValue = m_resultLightDir[j].m_distance;
						selectedLightIndex = j;
					}
				}
				if ( selectedLightIndex != -1 && magnitude < m_resultLightDir[selectedLightIndex].m_distance)
				{
					m_resultLightDir[selectedLightIndex].m_distance = magnitude;
					m_resultLightDir[selectedLightIndex].m_node = m_ambientNodes[i];
				}
			}
			*/
		}

		// Now set the weigth
			// Total Distance
		float totalDistance = 0;
		float totalLightDistance = 0;
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				m_resultNodes[i].m_node.SetIsUsed(true);
				totalDistance += m_resultNodes[i].m_distance;
			}
			/*
			if( m_resultLightDir[i].m_node != null )
			{
				m_resultLightDir[i].m_node.SetIsUsed( true );
				totalLightDistance += m_resultLightDir[i].m_distance;
			}
			*/
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
			/*
			if ( m_resultLightDir[i].m_node != null )
			{
				m_resultLightDir[i].m_weight = totalLightDistance - m_resultNodes[i].m_distance;
				totalLightWeight += m_resultLightDir[i].m_weight;
			}
			*/
		}

			// Normalize values
		for( int i = 0; i<NODES_TO_TAKE_INTO_ACCOUNT; i++ )
		{
			if ( m_resultNodes[i].m_node != null )
			{
				m_resultNodes[i].m_weight = m_resultNodes[i].m_weight / totalWeight;
			}
			/*
			if ( m_resultLightDir[i].m_node != null )
			{
				m_resultLightDir[i].m_weight = m_resultLightDir[i].m_weight / totalLightWeight;
			}
			*/
		}
	}

	void RefreshTargetValues()
	{
		// Ambient
		m_targetAmbientColor = emptyColor;
		m_targetAmbientIntensity = 0;
		// Skybox
		m_targetSunSize = 0;
		m_targetSkyColor = emptyColor;
		m_targetHorizonColor = emptyColor;
		m_targetHorizonHeight = 0;
		m_targetGroundColor = emptyColor;
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
				// m_targetSunSize += node.m_sunSize * nodeResult.m_weight;
				m_targetSkyColor += node.m_skyColor * nodeResult.m_weight;
				m_targetHorizonColor += node.m_horizonColor * nodeResult.m_weight;
				m_targetHorizonHeight += node.m_horizonHeight * nodeResult.m_weight;
				m_targetGroundColor += node.m_groundColor * nodeResult.m_weight;
					
					// Rain
				m_targetRainIntensity += node.m_rainIntensity * nodeResult.m_weight;

					// Sun size
				m_targetSunSize += node.m_sunSize * nodeResult.m_weight;
			}
			/*
			if ( m_resultLightDir[i].m_node != null )
			{
				AmbientNodeResults nodeResult = m_resultLightDir[i];
				AmbientNode node = nodeResult.m_node;
				m_targetSunSize += node.m_sunSize * nodeResult.m_weight;

				// Light
				m_targetLightAngles += node.transform.rotation.eulerAngles * nodeResult.m_weight;
				m_targetFlaresIntensity += node.m_flaresIntensity * nodeResult.m_weight;
			}
			*/
		}
	}

	void ApplyCurrentValues()
	{
		RenderSettings.ambientLight = m_ambientColor;
		RenderSettings.ambientIntensity = m_ambientIntensity;

		Material m = RenderSettings.skybox;
		m.SetFloat("_SunSize", m_sunSize);
		m.SetColor("_SkyColor", m_skyColor);
		m.SetColor("_HorizonColor", m_horizonColor);
		m.SetFloat("_HorizonHeight", m_horizonHeigth);
		m.SetColor("_GroundColor", m_groundColor);

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
		m_targetSkyColor = node.m_skyColor;
		m_targetHorizonColor = node.m_horizonColor;
		m_targetHorizonHeight = node.m_horizonHeight;
		m_targetGroundColor = node.m_groundColor;
			// Light
		m_targetLightAngles = node.transform.rotation.eulerAngles;
		m_targetFlaresIntensity = node.m_flaresIntensity;
			// Rain
		m_targetRainIntensity = node.m_rainIntensity;
	}

}
