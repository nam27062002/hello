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
	public bool m_applyToChildren = false;

	protected class PSDataRegistry
	{
		public float m_gravityModifierMultiplier;
		public float m_minGravityModifier;
		// public float m_startSpeedMultiplier;
		public ParticleSystem m_psystem;
	}
	protected List<PSDataRegistry> m_originalData = new List<PSDataRegistry>();

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
		m_originalData.Clear();
		SaveOriginalData();
	}

	void SaveOriginalData()
	{
		if(m_applyToChildren) {
			ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
			foreach(ParticleSystem p in childs)
				SaveParticleData(p);
		} else {
			ParticleSystem particle = GetComponent<ParticleSystem>();
			if(particle) {
				SaveParticleData(particle);
			}
		}
	}

	void SaveParticleData( ParticleSystem ps )
	{
		PSDataRegistry data = new PSDataRegistry();
		ParticleSystem.MainModule mainModule = ps.main;
		data.m_gravityModifierMultiplier = mainModule.gravityModifierMultiplier;
		switch( mainModule.gravityModifier.mode )
		{
			case ParticleSystemCurveMode.TwoConstants:
			{
				data.m_minGravityModifier = mainModule.gravityModifier.constantMin;
			}break;
		}
		// data.m_startSpeedMultiplier = mainModule.startSpeedMultiplier;
		data.m_psystem = ps;

		m_originalData.Add(data);
	}

	void ResetOriginalData()
	{
		if(m_applyToChildren) {
			foreach(PSDataRegistry p in m_originalData)
				ResetParticleData(p);
		} else {
			if(m_originalData.Count > 0) ResetParticleData(m_originalData[0]);
		}
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
		// mainModule.startSpeedMultiplier = data.m_startSpeedMultiplier;       
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
		if(m_applyToChildren) {
			foreach(PSDataRegistry pdata in m_originalData)
				ScaleGravity(pdata, scale);
		} else {
			if(m_originalData.Count > 0) ScaleGravity(m_originalData[0], scale);
		}
	}
	
	void ScaleGravity(PSDataRegistry data, float scale)
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
