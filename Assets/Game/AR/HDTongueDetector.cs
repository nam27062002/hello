﻿// HDTongueDetector.cs
// Hungry Dragon
// 
// Created by Diego Campos on 20/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;
#if (UNITY_IOS || UNITY_EDITOR_OSX)
using UnityEngine.Apple.ReplayKit;
#endif
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class HDTongueDetector : MonoBehaviour 
{
#if (UNITY_IOS || UNITY_EDITOR_OSX)
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string SCENE_PREFAB_PATH = "AR/Animojis/PF_AnimojiSceneSetup";
	private const string ANIMOJI_PREFABS_DIR = "AR/Animojis/";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Transform m_dragonAnimojiAnchor = null;

	// Internal
	private bool m_faceDetected = false;
	public bool faceDetected {
		get { return m_faceDetected; }
	}

	private bool m_tongueDetected = false;
	public bool tongueDetected {
		get { return m_tongueDetected; }
	}

	private Dictionary<string, float> m_currentBlendShapes;
	private AudioSource m_audio = null;
	private DragonAnimoji m_dragonAnimojiInstance = null;

	// Events
	public UnityEvent onFaceAdded = new UnityEvent();
	public UnityEvent onFaceRemoved = new UnityEvent();
	public UnityEvent onTongueDetected = new UnityEvent();
	public UnityEvent onTongueLost = new UnityEvent();

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether a dragon is supported or not.
	/// </summary>
	/// <returns>Whether the requested dragon supports animojis or not.</returns>
	/// <param name="_dragonSku">Dragon sku.</param>
	public static bool IsDragonSupported(string _dragonSku) {
		// Check content
		DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _dragonSku);
		if(dragonDef == null) return false;

		// Dragon is supported if it has an animoji prefab assigned to it :)
		return dragonDef.Has("animojiPrefab");
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Start () 
	{
		UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
		UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
		UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;

		m_audio = GetComponent<AudioSource> ();
	}

	/// <summary>
	/// Load a specific dragon setup.
	/// </summary>
	/// <param name="_dragonSku">Dragon sku.</param>
	public void InitWithDragon(string _dragonSku) {
		ControlPanel.Log(Colors.paleYellow.Tag("INIT WITH DRAGON " + _dragonSku));

		// Make sure dragon is supported
		Debug.Assert(IsDragonSupported(_dragonSku), "DRAGON " + _dragonSku + " DOESN'T SUPPORT ANIMOJIS!", this);

		// Load dragon head prefab
		DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _dragonSku);
		string prefabPath = dragonDef.GetAsString("animojiPrefab");
		GameObject prefab = Resources.Load<GameObject>(ANIMOJI_PREFABS_DIR + prefabPath);
		Debug.Assert(prefab != null, "COULDN'T LOAD ANIMOJI PREFAB " + ANIMOJI_PREFABS_DIR + prefabPath, this);

		// Instantiate it and get controller reference
		GameObject dragonInstance = GameObject.Instantiate<GameObject>(prefab, m_dragonAnimojiAnchor, false);
		m_dragonAnimojiInstance = dragonInstance.GetComponentInChildren<DragonAnimoji>();
		Debug.Assert(m_dragonAnimojiInstance != null, "ANIMOJI PREFAB " + ANIMOJI_PREFABS_DIR + prefabPath + " DOESN'T HAVE A DragonAnimoji COMPONENT", this);
	}

	/// <summary>
	/// Start recording process.
	/// </summary>
	public void StartRecording() {
		// Prevent spamming
		if(ReplayKit.isRecording) return;

		// Do it!
		ControlPanel.Log(Colors.paleYellow.Tag("START RECORDING"));
		ReplayKit.StartRecording(true);
	}

	/// <summary>
	/// Stop recording process.
	/// </summary>
	public void StopRecording() {
		// Prevent spamming
		if(!ReplayKit.isRecording) return;

		// Do it!
		ControlPanel.Log(Colors.paleYellow.Tag("STOP RECORDING"));
		ReplayKit.StopRecording();
	}

	/// <summary>
	/// Show the native preview dialog.
	/// Will be ignored if a record is not available.
	/// </summary>
	public void ShowPreview() {
		// Make sure we have something to preview
		if(!ReplayKit.recordingAvailable) return;

		// Do it!
		ControlPanel.Log(Colors.paleYellow.Tag("SHOW PREVIEW"));
		ReplayKit.Preview();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A face has been added.
	/// </summary>
	/// <param name="anchorData">Event data.</param>
	void FaceAdded (ARFaceAnchor anchorData)
	{
		m_faceDetected = true;
		m_currentBlendShapes = anchorData.blendShapes;
		m_dragonAnimojiInstance.ToggleFire(false);

		// Notify listeners
		ControlPanel.Log(Colors.paleYellow.Tag("FACE DETECTED"));
		onFaceAdded.Invoke();
	}

	/// <summary>
	/// A face has been updated.
	/// </summary>
	/// <param name="anchorData">Event data.</param>
	void FaceUpdated (ARFaceAnchor anchorData)
	{
		m_currentBlendShapes = anchorData.blendShapes;
	}

	/// <summary>
	/// A face has been removed.
	/// </summary>
	/// <param name="anchorData">Event data.</param>
	void FaceRemoved (ARFaceAnchor anchorData)
	{
		m_faceDetected = false;
		m_dragonAnimojiInstance.ToggleFire(false);

		// Notify listeners
		ControlPanel.Log(Colors.paleYellow.Tag("FACE REMOVED"));
		onFaceRemoved.Invoke();
	}

	// Update is called once per frame
	void Update () {
		bool enableTongue = false;

		if (m_faceDetected) 
		{
			if (m_currentBlendShapes.ContainsKey (ARBlendShapeLocation.TongueOut)) 
			{
				enableTongue = (m_currentBlendShapes [ARBlendShapeLocation.TongueOut] > 0.5f);

			}

		}
		if (enableTongue != m_tongueDetected) {
			m_tongueDetected = enableTongue;
			m_dragonAnimojiInstance.ToggleFire(m_tongueDetected);
			if(m_tongueDetected) {
				m_audio.Play ();

				// Notify listeners
				ControlPanel.Log(Colors.paleYellow.Tag("TONGUE DETECTED"));
				onTongueDetected.Invoke();
			} else {
				m_audio.Stop ();

				// Notify listeners
				ControlPanel.Log(Colors.paleYellow.Tag("TONGUE LOST"));
				onTongueLost.Invoke();
			}
		}
	}
#endif
}
