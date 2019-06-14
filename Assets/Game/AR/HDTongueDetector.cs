// HDTongueDetector.cs
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
#if (UNITY_IOS)
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
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string SCENE_PREFAB_PATH = "AR/Animojis/PF_AnimojiSceneSetup";	

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
	public DragonAnimoji m_dragonAnimojiInstance = null;

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

	void OnDestroy()
	{
		Debug.Log (">>>>>>>>>>>>>>>>>>>>>>>>>> HDTongueDetector.OnDestroy()");
        
        //Subscribe delegates
        UnityARSessionNativeInterface.ARFaceAnchorAddedEvent -= FaceAdded;
        UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent -= FaceUpdated;
        UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent -= FaceRemoved;


//		Destroy(m_dragonAnimojiInstance.gameObject);
	}
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Start () 
	{
		//Subscribe delegates
		UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
		UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
		UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;

		m_audio = GetComponent<AudioSource> ();
	}

	/// <summary>
	/// Load a specific dragon setup.
	/// </summary>
	/// <param name="_dragonPrefab">Dragon prefab to instantiate</param>
	public void InitWithDragon(GameObject _dragonPrefab) {                       		        
		InstantiateDragon(_dragonPrefab); 
    }		    

	private void InstantiateDragon(GameObject prefab) {      
		// Instantiate it and get controller reference
		GameObject dragonInstance = GameObject.Instantiate<GameObject>(prefab, m_dragonAnimojiAnchor, false);
		m_dragonAnimojiInstance = dragonInstance.GetComponentInChildren<DragonAnimoji>();
		Debug.Assert(m_dragonAnimojiInstance != null, "ANIMOJI PREFAB DOESN'T HAVE A DragonAnimoji COMPONENT", this);

		m_dragonAnimojiInstance.gameObject.SetActive(false);        
	}

	/// <summary>
	/// Start recording process.
	/// </summary>
	/// <returns>Did the recording start properly?</returns>
	/// <param name="_enableMicrophone">Record audio as well?</param>
	public bool StartRecording(bool _enableMicrophone) {
#if (UNITY_IOS)
        // Prevent spamming
        if(ReplayKit.isRecording) return false;

		// Do it!
//		ControlPanel.Log(Colors.paleYellow.Tag("DISCARD RECORDING"));
//		ReplayKit.Discard ();

		ControlPanel.Log(Colors.paleYellow.Tag("START RECORDING"));
		bool success = ReplayKit.StartRecording(_enableMicrophone);

		ControlPanel.Log(Colors.paleYellow.Tag("START RECORDING RESULT: " + success));
#endif
        HDTrackingManagerImp.Instance.Notify_AnimojiRecord();
		return false;
	}

	/// <summary>
	/// Stop recording process.
	/// </summary>
	public void StopRecording() {
#if (UNITY_IOS)
		// Prevent spamming
		if(!ReplayKit.isRecording) return;

		// Do it!
		ControlPanel.Log(Colors.paleYellow.Tag("STOP RECORDING"));
		ReplayKit.StopRecording();
#endif
	}

	/// <summary>
	/// Show the native preview dialog.
	/// Will be ignored if a record is not available.
	/// </summary>
	public void ShowPreview() {
#if (UNITY_IOS)
		// Make sure we have something to preview
		if (!ReplayKit.recordingAvailable) {
			Debug.Log ("No recording available!!!");
			return;
		}

		// Do it!
		ControlPanel.Log(Colors.paleYellow.Tag("SHOW PREVIEW"));
		ReplayKit.Preview();
#endif
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
		m_dragonAnimojiInstance.gameObject.SetActive (true);
		m_dragonAnimojiInstance.ToggleFire(false);

		// Notify listeners
		ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI]] FACE DETECTED"));
		onFaceAdded.Invoke();
	}
		
	/// <summary>
	/// A face has been updated.
	/// </summary>
	/// <param name="anchorData">Event data.</param>
	void FaceUpdated (ARFaceAnchor anchorData)
	{
		m_currentBlendShapes = anchorData.blendShapes;

		if(m_faceDetected != anchorData.isTracked) {
			ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI]] FORCING FACE DETECTED/REMOVED"));
			if(anchorData.isTracked) {
				FaceAdded(anchorData);
			} else {
				FaceRemoved(anchorData);
			}			
		}
	}

	/// <summary>
	/// A face has been removed.
	/// </summary>
	/// <param name="anchorData">Event data.</param>
	void FaceRemoved (ARFaceAnchor anchorData)
	{
		m_faceDetected = false;
		m_dragonAnimojiInstance.ToggleFire(false);
		m_dragonAnimojiInstance.gameObject.SetActive (false);

		// Notify listeners
		ControlPanel.Log(Colors.paleYellow.Tag("[ANIMOJI] FACE REMOVED"));
		onFaceRemoved.Invoke();
	}

	private bool m_lastFaceDetected = false;

	// Update is called once per frame
	void Update () {
		bool enableTongue = false;

		if (m_faceDetected != m_lastFaceDetected) {
			m_dragonAnimojiInstance.gameObject.SetActive (m_faceDetected);
			m_lastFaceDetected = m_faceDetected;

			if (m_faceDetected) {
				Debug.Log (">>>>>>>>>FACE DETECTED");
			} else {
				Debug.Log (">>>>>>>>>FACE REMOVED");
			}
		}

		if (m_faceDetected) {
			if (m_currentBlendShapes.ContainsKey (ARBlendShapeLocation.TongueOut)) {
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
}
