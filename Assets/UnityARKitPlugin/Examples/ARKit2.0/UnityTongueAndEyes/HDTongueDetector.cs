using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;

public class HDTongueDetector : MonoBehaviour 
{
	public FireBreathDynamic fireRushPrefab;
	public float m_effectScale = 1.0f;
	bool shapeEnabled = false;
	bool fireEnabled = false;
	Dictionary<string, float> currentBlendShapes;

	// Use this for initialization
	void Start () 
	{
		UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
		UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
		UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;

	}

	void OnGUI()
	{
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
	// Update is called once per frame
	void Update () {
		
	}
}
