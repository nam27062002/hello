using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BossCameraAffector))]
public class BossCameraAffectorEditor : Editor 
{
	private BossCameraAffector m_affector;
	private BossCameraAffector.DetectType m_prevType = BossCameraAffector.DetectType.radius;
	private float m_prevRadius;

	// used to prevent unity to do this more than once in the same prefab.
	private static GameObject m_instance = null;

	void OnEnable()
	{
		if(m_instance == null)
		{
			m_affector = target as BossCameraAffector;
			CheckStatus();
		}
	}	

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		// change the things only if the important parameters have been changed.
		if(m_prevType != m_affector.detectType || (m_affector.detectType == BossCameraAffector.DetectType.radius && m_prevRadius != m_affector.radius))
		{
			CheckStatus();
		}
	}

	private void CheckStatus()
	{
		BossCameraAffectorRadius bcar = GetComponentInPrefab<BossCameraAffectorRadius>(m_affector.gameObject);
		// let's do changes only if necessary.
		switch(m_affector.detectType)
		{
			// if the type is radius...
			case BossCameraAffector.DetectType.radius:
				// ... and no BossCameraAffectorRadius are present...
				if(bcar == null)
				{
					// ... add.
					AddBossCameraAffectorRadius();
				}
				else
				{
					// if the configured radius is different...
					SphereCollider sc = bcar.GetComponent<SphereCollider>();
					if(!IsColliderRadiusUpdated(m_affector.radius, sc))
					{
						// ... apply the wanted radius.
						ConfigureBossCameraAffectorRadius();
					}
					m_prevRadius = m_affector.radius;
				}
				break;
			// if the type is onscreen...
			case BossCameraAffector.DetectType.onscreen:
				// ... and a BossCameraAffectorRadius is present...
				if(bcar != null)
				{
					// ... remove it.
					RemoveBossCameraAffectorRadius();
				}
				break;
		}
		m_prevType = m_affector.detectType;
	}

	private void AddBossCameraAffectorRadius()
	{
		GetInstance();
		// create what is needed and configure everything properly.
		GameObject go = new GameObject();
		go.name = "BossCameraAffectorRadius";
		go.layer = LayerMask.NameToLayer("PlayerOnly");
		BossCameraAffectorRadius bcar = go.AddComponent<BossCameraAffectorRadius>();
		SphereCollider sc = go.AddComponent<SphereCollider>();
		sc.isTrigger = true;
		// assign the private serialized fields to the BossCameraAffectorRadius and apply the changes.
		SerializedObject serialized = new SerializedObject(bcar);
		serialized.FindProperty("m_bca").objectReferenceValue = m_affector;
		serialized.FindProperty("m_collider").objectReferenceValue = sc;
		serialized.ApplyModifiedProperties();
		// configure the transform using the instance and not the prefab.
		Transform t = go.transform;
		t.parent = m_instance.transform;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = Vector3.one;
		// now apply the radius defined in the affector.
		ConfigureBossCameraAffectorRadius(sc);
	}

	private void ConfigureBossCameraAffectorRadius(SphereCollider sc = null)
	{
		GetInstance();
		if(sc == null)
		{
			// if the call to this method doesn't come from the creation of the BossCameraAffectorRadius
			// we need to take the SphereCollider from the instance in order to apply the wanted changes correctly.
			sc = m_instance.GetComponentInChildren<BossCameraAffectorRadius>().GetComponent<SphereCollider>();
		}
		m_prevRadius = sc.radius = GetColliderRadius(sc);
		// tell unity that this object is now dirty.
		EditorUtility.SetDirty(m_affector);
		ApplyInstance();
	}

	private void RemoveBossCameraAffectorRadius()
	{
		GetInstance();
		// we need to take the BossCameraAffectorRadius from the instance in order to apply the wanted changes correctly.
		BossCameraAffectorRadius bcar = m_instance.GetComponentInChildren<BossCameraAffectorRadius>();
		DestroyImmediate(bcar.gameObject);
		EditorUtility.SetDirty(m_affector);
		ApplyInstance();
	}

	private void GetInstance()
	{
		if(m_instance == null)
		{
			// if we are checking a prefab, let's make an instance out of it.
			PrefabType pType = PrefabUtility.GetPrefabType(m_affector);
			switch(pType)
			{
				case PrefabType.Prefab:
				case PrefabType.ModelPrefab:
					// if it's a prefab let's instantiate it, change it and apply. lately the instance will be removed.
					m_instance = PrefabUtility.InstantiatePrefab(m_affector.gameObject) as GameObject;
					break;
				default:
					m_instance = m_affector.gameObject;
					break;
			}
		}
	}

	private void ApplyInstance()
	{
		// if this was a prefab, manage it !!
		PrefabType pType = PrefabUtility.GetPrefabType(m_affector);
		switch(pType)
		{
			case PrefabType.Prefab:
			case PrefabType.ModelPrefab:
				PrefabUtility.ReplacePrefab(m_instance, m_affector, ReplacePrefabOptions.ConnectToPrefab);
				// save the assets automatically so that the prefab will be applied on disc.
				EditorApplication.SaveAssets();
				DestroyImmediate(m_instance);
				break;
		}
		m_instance = null;
	}

	private float GetColliderRadius(SphereCollider sc)
	{
		// the lossyscale is because we don't want the value set by the designers to be affected by the scale of the parent transform.
		return m_affector.radius / sc.transform.lossyScale.x;
	}

	private bool IsColliderRadiusUpdated(float bosscameraRadius, SphereCollider sc)
	{
		// the lossyscale is because we don't want the value set by the designers to be affected by the scale of the parent transform.
		return m_affector.radius == sc.radius * sc.transform.lossyScale.x;
	}

	private T GetComponentInPrefab<T>(GameObject pf) where T : Component
	{
		T c = pf.GetComponent<T>();
		if(c != null)
		{
			return c;
		}
		else
		{
			foreach(Transform child in pf.transform)
			{
				c = GetComponentInPrefab<T>(child.gameObject);
				if(c != null)
				{
					return c;
				}
			}
		}
		return default(T);
	}
}