using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_ANDROID

#if ARCORE_SDK_ENABLED
using GoogleARCore;
#endif

public class ARKitAnchorManager
{
	private List<GameObject> m_kPlaneAnchorMap = null;

#if ARCORE_SDK_ENABLED
	/// <summary>
	/// A list to hold new planes ARCore began tracking in the current frame. This object is used across
	/// the application to avoid per-frame allocations.
	/// </summary>
	private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();

	/// <summary>
	/// A list to hold all planes ARCore is tracking in the current frame. This object is used across
	/// the application to avoid per-frame allocations.
	/// </summary>
	private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();
#endif

	private string m_strSurfaceBasePrefab;

	private string m_strARHitLayer;

	public bool m_bEnabledRendering = true;

	public bool m_bCanUpdate = false;

	public ARKitAnchorManager (string strSurfaceBasePrefab, string strARHitLayer)
	{
		m_strSurfaceBasePrefab = strSurfaceBasePrefab;
		m_strARHitLayer = strARHitLayer;

		m_kPlaneAnchorMap = new List<GameObject> ();
	}



	public void SetCurrentAnchorsVisible (bool bVisible)
	{
		m_bEnabledRendering = bVisible;

		for (int i = 0; i < m_kPlaneAnchorMap.Count; ++i)
		{
			if (m_kPlaneAnchorMap[i] != null) 
			{
				ARCoreTrackedPlaneVisualizer kVisualizer = m_kPlaneAnchorMap [i].GetComponent<ARCoreTrackedPlaneVisualizer> ();
				if (kVisualizer != null)
				{
					kVisualizer.m_bCanRender = bVisible;
				}
			}
		}
	}

	public void Destroy ()
	{
		for (int i = 0; i < m_kPlaneAnchorMap.Count; ++i)
		{
			GameObject.DestroyImmediate (m_kPlaneAnchorMap [i]);
		}

		m_kPlaneAnchorMap.Clear ();
	}

	public List<GameObject> GetCurrentPlaneAnchors()
	{
		return m_kPlaneAnchorMap;
	}

	public void Update ()
	{
#if ARCORE_SDK_ENABLED
		if (!m_bCanUpdate)
			return;

		// Check that motion tracking is tracking.
		if (Session.Status != SessionStatus.Tracking)
		{
			const int lostTrackingSleepTimeout = 15;
			Screen.sleepTimeout = lostTrackingSleepTimeout;
			return;
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
		Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
		for (int i = 0; i < m_NewPlanes.Count; i++)
		{
			// Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
			// the origin with an identity rotation since the mesh for our prefab is updated in Unity World
			// coordinates.

			GameObject kARKitSurfaceBasePrefab = Resources.Load (m_strSurfaceBasePrefab) as GameObject;

			GameObject kARKitSurfaceBase = GameObject.Instantiate (kARKitSurfaceBasePrefab, Vector3.zero, Quaternion.identity);

			CaletyUtils.SetLayer (kARKitSurfaceBase, m_strARHitLayer);
			kARKitSurfaceBase.AddComponent<DontDestroyOnLoad> ();



			ARCoreTrackedPlaneVisualizer kVisualizer = kARKitSurfaceBase.GetComponent<ARCoreTrackedPlaneVisualizer> ();

			if (kVisualizer != null)
			{
				kVisualizer.Initialize(m_NewPlanes[i]);

				kVisualizer.m_bCanRender = m_bEnabledRendering;

				kVisualizer.Update ();
			}

			m_kPlaneAnchorMap.Add (kARKitSurfaceBase);
		}
#endif
	}
}

#elif (UNITY_IOS || UNITY_EDITOR_OSX)

using UnityEngine.XR.iOS;

public class ARKitAnchorManager
{
	private Dictionary<string, ARPlaneAnchorGameObject> m_kPlaneAnchorMap = null;

	private string m_strSurfaceBasePrefab;

	private string m_strARHitLayer;

	public bool m_bEnabledRendering = true;

	public bool m_bCanUpdate = false;

	public ARKitAnchorManager (string strSurfaceBasePrefab, string strARHitLayer)
	{
		m_strSurfaceBasePrefab = strSurfaceBasePrefab;
		m_strARHitLayer = strARHitLayer;

		m_kPlaneAnchorMap = new Dictionary<string,ARPlaneAnchorGameObject> ();

		UnityARSessionNativeInterface.ARAnchorAddedEvent += AddAnchor;
		UnityARSessionNativeInterface.ARAnchorUpdatedEvent += UpdateAnchor;
		UnityARSessionNativeInterface.ARAnchorRemovedEvent += RemoveAnchor;
	}

	public void AddAnchor (ARPlaneAnchor kARPlaneAnchor)
	{
		if (m_kPlaneAnchorMap.ContainsKey (kARPlaneAnchor.identifier))
		{
			RemoveAnchor (kARPlaneAnchor);
		}



		GameObject kARKitSurfaceBase = GameObject.Instantiate (Resources.Load (m_strSurfaceBasePrefab) as GameObject);

		MeshRenderer kRenderer = kARKitSurfaceBase.GetComponentInChildren<MeshRenderer> ();
		kRenderer.enabled = m_bEnabledRendering;

		CaletyUtils.SetLayer (kARKitSurfaceBase, m_strARHitLayer);
		kARKitSurfaceBase.AddComponent<DontDestroyOnLoad> ();

		ARPlaneAnchorGameObject kNewARPlaneAnchor = new ARPlaneAnchorGameObject ();
		kNewARPlaneAnchor.planeAnchor = kARPlaneAnchor;
		kNewARPlaneAnchor.gameObject = kARKitSurfaceBase;

		m_kPlaneAnchorMap.Add (kARPlaneAnchor.identifier, kNewARPlaneAnchor);
	}

	public void RemoveAnchor(ARPlaneAnchor kARPlaneAnchor)
	{
		if (m_kPlaneAnchorMap.ContainsKey (kARPlaneAnchor.identifier))
		{
			ARPlaneAnchorGameObject kStoredARPlaneAnchor = m_kPlaneAnchorMap [kARPlaneAnchor.identifier];
			GameObject.Destroy (kStoredARPlaneAnchor.gameObject);

			m_kPlaneAnchorMap.Remove (kARPlaneAnchor.identifier);
		}
	}

	public void UpdateAnchor(ARPlaneAnchor kARPlaneAnchor)
	{
		if (m_kPlaneAnchorMap.ContainsKey (kARPlaneAnchor.identifier))
		{
			ARPlaneAnchorGameObject kStoredARPlaneAnchor = m_kPlaneAnchorMap [kARPlaneAnchor.identifier];
			UpdatePlaneWithAnchorTransform (kStoredARPlaneAnchor.gameObject, kARPlaneAnchor);
			kStoredARPlaneAnchor.planeAnchor = kARPlaneAnchor;

			m_kPlaneAnchorMap [kARPlaneAnchor.identifier] = kStoredARPlaneAnchor;
		}
	}

	private void UpdatePlaneWithAnchorTransform(GameObject kPlane, ARPlaneAnchor kARPlaneAnchor)
	{
		kPlane.transform.position = UnityARMatrixOps.GetPosition (kARPlaneAnchor.transform);
		kPlane.transform.rotation = UnityARMatrixOps.GetRotation (kARPlaneAnchor.transform);

		MeshFilter mf = kPlane.GetComponentInChildren<MeshFilter> ();

		if (mf != null)
		{
			mf.gameObject.transform.localScale = new Vector3(kARPlaneAnchor.extent.x * 0.1f, kARPlaneAnchor.extent.y * 0.1f, kARPlaneAnchor.extent.z * 0.1f );
			mf.gameObject.transform.localPosition = new Vector3(kARPlaneAnchor.center.x, kARPlaneAnchor.center.y, -kARPlaneAnchor.center.z);
		}
	}

	public void SetCurrentAnchorsVisible (bool bVisible)
	{
		m_bEnabledRendering = bVisible;

		foreach (ARPlaneAnchorGameObject kStoredARPlaneAnchor in GetCurrentPlaneAnchors())
		{
			if (kStoredARPlaneAnchor.gameObject != null) 
			{
				MeshRenderer kRenderer = kStoredARPlaneAnchor.gameObject.GetComponentInChildren<MeshRenderer> ();

				kRenderer.enabled = m_bEnabledRendering;
			}
		}
	}

	public void Destroy()
	{
		UnityARSessionNativeInterface.ARAnchorAddedEvent -= AddAnchor;
		UnityARSessionNativeInterface.ARAnchorUpdatedEvent -= UpdateAnchor;
		UnityARSessionNativeInterface.ARAnchorRemovedEvent -= RemoveAnchor;

		foreach (ARPlaneAnchorGameObject kStoredARPlaneAnchor in GetCurrentPlaneAnchors())
		{
			GameObject.Destroy (kStoredARPlaneAnchor.gameObject);
		}

		m_kPlaneAnchorMap.Clear ();
	}

	public List<ARPlaneAnchorGameObject> GetCurrentPlaneAnchors()
	{
		return m_kPlaneAnchorMap.Values.ToList ();
	}

	public void Update ()
	{
	}
}

#endif
