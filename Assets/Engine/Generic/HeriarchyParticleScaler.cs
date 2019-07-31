using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeriarchyParticleScaler : MonoBehaviour 
{

	public Transform m_transform;

	[Space]
	public bool m_resetFirst = false;

	public enum WhenScale
	{
		START,
		ENABLE,
		AFTER_ENABLE,
		MANUALLY,
		ALWAYS	// Use carefully!!
	}
	[Space]
	public WhenScale m_whenScale;

	// Internal
	static ParticleSystem.Particle[] m_particlesBuffer;	// [AOC] Prevent constant memory allocation by having a buffer to perform particle by particle operations. 
                                                        // [MALH] And if we share it between all particleScalers we avoid multiple news and memory trashing :p


	protected class PSDataRegistry
	{
		public float m_gravityModifierMultiplier;
		public float m_minGravityModifier;
		public float m_startSpeedMultiplier;
		public ParticleSystem m_psystem;
	}
	protected PSDataRegistry m_originalData = new PSDataRegistry();


    void Awake()
	{
		// Save original data
		SaveOriginalData();
	}


	// Use this for initialization
	void Start () 
	{
		if ( m_whenScale == WhenScale.START )
			DoScale();
	}

	private void Update() {
		if(m_whenScale == WhenScale.ALWAYS) {
			DoScale();
		}else{
			enabled = false;
		}
	}

	public void ReloadOriginalData() {
		SaveOriginalData();
	}

	void SaveOriginalData()
	{
		ParticleSystem particle =  GetComponent<ParticleSystem>();
		if (particle)
		{
			SaveParticleData( particle );
		}	
	}

	void SaveParticleData( ParticleSystem ps )
	{
		ParticleSystem.MainModule mainModule = ps.main;
		m_originalData.m_gravityModifierMultiplier = mainModule.gravityModifierMultiplier;
		switch( mainModule.gravityModifier.mode )
		{
			case ParticleSystemCurveMode.TwoConstants:
			{
				m_originalData.m_minGravityModifier = mainModule.gravityModifier.constantMin;
			}break;
		}
		m_originalData.m_startSpeedMultiplier = mainModule.startSpeedMultiplier;
		m_originalData.m_psystem = ps;
	}


    void ResetOriginalData()
	{
		ResetParticleData(m_originalData);
	}


	void ResetParticleData(PSDataRegistry data)
	{
		ParticleSystem ps = data.m_psystem;
		ParticleSystem.MainModule mainModule = ps.main;
		ParticleSystem.MinMaxCurve curve = mainModule.gravityModifier;
		mainModule.gravityModifierMultiplier = data.m_gravityModifierMultiplier;
		switch( curve.mode )
		{
			case ParticleSystemCurveMode.TwoConstants:
			{
				curve.constantMin = data.m_minGravityModifier;
				mainModule.gravityModifier = curve;
			}break;
			case ParticleSystemCurveMode.Curve:
			{
				Debug.LogError("Not Supported");
			}break;
			case ParticleSystemCurveMode.TwoCurves:
			{
				Debug.LogError("Not Supported");
			}break;
		}
		mainModule.startSpeedMultiplier = data.m_startSpeedMultiplier;       
    }

    void OnEnable()
	{
		if ( m_whenScale == WhenScale.ENABLE )
			DoScale();
		else if (m_whenScale == WhenScale.AFTER_ENABLE) 
		{
			StartCoroutine( AfterEnable() );
		}
	}

	IEnumerator AfterEnable()
	{
		yield return null;
		DoScale();
	}

	public void DoScale()
	{
		if ( m_resetFirst )
		{
			ResetOriginalData();
		}

		if (m_transform != null)
		{
			Scale(m_transform.lossyScale.x);
		}
	}

	void Scale( float scale )
	{	
		ScalePS( m_originalData, scale );
	}
	
	void ScalePS(PSDataRegistry data, float scale)
	{
        if (data.m_psystem != null)
        {
			ParticleSystem ps = data.m_psystem;
			ParticleSystem.MainModule mainModule = ps.main;
			mainModule.gravityModifierMultiplier *= scale;

			ParticleSystem.MinMaxCurve curve = mainModule.gravityModifier;
			switch( curve.mode )
			{
				case ParticleSystemCurveMode.TwoConstants:
				{
					curve.constantMin *= scale;
					mainModule.gravityModifier = curve;
				}break;
				case ParticleSystemCurveMode.TwoCurves:
				{
					Debug.LogError("Not Supported");
				}break;
				case ParticleSystemCurveMode.Curve:
				{
					Debug.LogError("Not Supported");
				}break;
			}
            mainModule.startSpeedMultiplier *= scale;
        }
    }

}
