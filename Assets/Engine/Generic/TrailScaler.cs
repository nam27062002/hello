using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrailScaler : MonoBehaviour 
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

	// Internal
	public class TrailDataRegistry {
		// Not much for now
		public TrailRenderer trail;

		public float widthMultiplier;
		public float time;
		public float minVertexDistance;
	}
	protected List<TrailDataRegistry> m_originalTrailData = new List<TrailDataRegistry>();

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
		m_originalTrailData.Clear();
		SaveOriginalData();
	}

	void SaveOriginalData()
	{
		if(m_applyToChildren) {
			TrailRenderer[] trails = gameObject.GetComponentsInChildren<TrailRenderer>(true);
			foreach(TrailRenderer t in trails) {
				SaveTrailData(t);
			}
		} else {
			TrailRenderer trail = GetComponent<TrailRenderer>();
			if(trail != null) {
				SaveTrailData(trail);
			}
		}
	}

	private void SaveTrailData(TrailRenderer _t) {
		// Create new data registry
		TrailDataRegistry data = new TrailDataRegistry();

		// Store reference to the target trail renderer
		data.trail = _t;

		// Store relevant properties
		data.widthMultiplier = _t.widthMultiplier;
		data.time = _t.time;
		data.minVertexDistance = _t.minVertexDistance;

		// Save data!
		m_originalTrailData.Add(data);
	}

	void ResetOriginalData()
	{
		if(m_applyToChildren) {
			foreach(TrailDataRegistry t in m_originalTrailData)
				ResetTrailData(t);
		} else {
			if(m_originalTrailData.Count > 0) ResetTrailData(m_originalTrailData[0]);
		}
	}

	private void ResetTrailData(TrailDataRegistry _data) {
		if(_data.trail == null) return;

		TrailRenderer trail = _data.trail;

		// Restore relevant properties
		trail.widthMultiplier = _data.widthMultiplier;
		trail.time = _data.time;
		trail.minVertexDistance = _data.minVertexDistance;
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
		} else
        {
			Scale(transform.lossyScale.x);
        }
	}

	void Scale( float scale )
	{	
		if(m_applyToChildren) {
			foreach(TrailDataRegistry trailData in m_originalTrailData) {
				ScaleTrail(trailData, scale);
			}
		} else {
			if(m_originalTrailData.Count > 0) ScaleTrail(m_originalTrailData[0], scale);
		}
	}

	private void ScaleTrail(TrailDataRegistry _data, float _scale) {
		if(_data.trail == null) return;

		TrailRenderer trail = _data.trail;

		trail.widthMultiplier *= _scale;
		trail.minVertexDistance *= _scale;
	}
}
