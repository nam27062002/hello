using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;
using UnityEngine.Apple.ReplayKit;

public class HDTongueDetector : MonoBehaviour 
{
	public FireBreathDynamic fireRushPrefab;
	public float m_effectScale = 1.0f;
	private bool shapeEnabled = false;
	private bool fireEnabled = false;
	private Dictionary<string, float> currentBlendShapes;

	private bool m_isRecording = false;
	private bool m_recordAvailable = false;
	private string m_buttonString = "Start Record!";

	// Use this for initialization
	void Start () 
	{
		UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
		UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
		UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
	}
		
	void recordButton()
	{
		if (m_isRecording) {
			ReplayKit.StopRecording ();
			m_buttonString = "Start Record";
			m_isRecording = false;
			m_recordAvailable = true;
		} else {
			ReplayKit.StartRecording ();
			m_buttonString = "Stop Record";
			m_isRecording = true;
		}

		Debug.Log (">>>>>>>>>>Record button");
	}

	void previewButton()
	{
		if (m_recordAvailable) {
			ReplayKit.Preview ();
		}
		Debug.Log (">>>>>>>>>>Preview button");
	}

	void FaceAdded (ARFaceAnchor anchorData)
	{
		shapeEnabled = true;
		currentBlendShapes = anchorData.blendShapes;
		fireRushPrefab.EnableFlame (false, false);
	}

	void FaceUpdated (ARFaceAnchor anchorData)
	{
		currentBlendShapes = anchorData.blendShapes;
	}

	void FaceRemoved (ARFaceAnchor anchorData)
	{
		shapeEnabled = false;
		fireRushPrefab.EnableFlame (false, false);
	}


	void OnGUI()
	{
#if !UNITY_EDITOR		
		if (!ReplayKit.APIAvailable) return;
#endif

		GUILayout.Space (50.0f);
		if (GUILayout.Button (m_buttonString, GUILayout.Width(250.0f), GUILayout.Height(125.0f))) {
			recordButton ();
		}
		if (m_recordAvailable) {
			if (GUILayout.Button ("Preview", GUILayout.Width(250.0f), GUILayout.Height(125.0f))) {
				previewButton ();
			}
		}
	}



	// Update is called once per frame
	void Update () {
		bool enableTongue = false;

		if (shapeEnabled) 
		{
			if (currentBlendShapes.ContainsKey (ARBlendShapeLocation.TongueOut)) 
			{
				enableTongue = (currentBlendShapes [ARBlendShapeLocation.TongueOut] > 0.5f);

			}

		}
		if (enableTongue != fireEnabled) {
			fireEnabled = enableTongue;
			fireRushPrefab.EnableFlame (fireEnabled);
			fireRushPrefab.setEffectScale (m_effectScale, m_effectScale);
		}
	}
}
