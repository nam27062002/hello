using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityARFaceAnchorManager : MonoBehaviour {

	[SerializeField]
	private GameObject anchorPrefab;

	private UnityARSessionNativeInterface m_session;

	// Use this for initialization
	void Start () {
        Debug.Log( "UnityARFaceAnchorManager Start" );
		m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

		Application.targetFrameRate = 60;
		ARKitFaceTrackingConfiguration config = new ARKitFaceTrackingConfiguration();
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.enableLightEstimation = true;

		if (config.IsSupported ) {
			m_session.RunWithConfigAndOptions (config, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors);
            Debug.Log( "UnityARFaceAnchorManager Register" );
			UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
			UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
			UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
		}

	}

	void FaceAdded (ARFaceAnchor anchorData)
	{
        Debug.Log( "UnityARFaceAnchorManager FaceAdded" );
		anchorPrefab.transform.position = UnityARMatrixOps.GetPosition (anchorData.transform);
		anchorPrefab.transform.rotation = UnityARMatrixOps.GetRotation (anchorData.transform);
		anchorPrefab.SetActive (true);
	}

	void FaceUpdated (ARFaceAnchor anchorData)
	{
		anchorPrefab.transform.position = UnityARMatrixOps.GetPosition (anchorData.transform);
		anchorPrefab.transform.rotation = UnityARMatrixOps.GetRotation (anchorData.transform);
        Debug.Log( "UnityARFaceAnchorManager FaceUpdated ");
        anchorPrefab.SetActive (true);
	}

	void FaceRemoved (ARFaceAnchor anchorData)
	{
        Debug.Log( "UnityARFaceAnchorManager FaceRemoved" );
		anchorPrefab.SetActive (false);
	}


	void OnDestroy()
	{
		Debug.Log (">>>>>>>>>>>>>>> UnityARFaceAnchorManager.OnDestroy()");
        UnityARSessionNativeInterface.ARFaceAnchorAddedEvent -= FaceAdded;
        UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent -= FaceUpdated;
        UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent -= FaceRemoved;
	}
}
