// SelfDestroy.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Serialization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Aux class to handle self-destruction.
/// Useful for placeholders.
/// </summary>
public class SelfDestroy : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Mode {
		SECONDS,
		FRAMES
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] protected Mode m_mode = Mode.SECONDS;
	public Mode mode {
		get { return m_mode; }
		set { m_mode = value; }
	}

	[FormerlySerializedAs("m_lifeTime")]
	[SerializeField] protected float m_seconds = 0f;
	public float seconds {
		get { return m_seconds; }
		set { m_seconds = value; m_mode = Mode.SECONDS; }
	}

	[SerializeField] protected int m_frames = 0;
	public int frames {
		get { return m_frames; }
		set { m_frames = value; m_mode = Mode.FRAMES; }
	}

	[Tooltip("Optionally replace with the given prefab before destroying, cloning transform and name properties. Full prefab path from resources.")]
	[FileListAttribute("Resources", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] protected string m_replacementPrefab = "";
	public string replacementPrefab {
		get { return m_replacementPrefab; }
		set { m_replacementPrefab = value; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// If it has to be destroyed immediately (typically because this game object is a placeholder object used in edit mode)
		// then it's done as soon as possible in order to prevent other objects retrieving components from
		// getting components in this game object.
		CheckDestroy();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	protected virtual void Update() {
		// Update conditions (depending on mode)
		switch(m_mode) {
			case Mode.SECONDS: {
				if(m_seconds > 0f) m_seconds -= Time.deltaTime;
			} break;

			case Mode.FRAMES: {
				if(m_frames > 0) --m_frames;
			} break;
		}

		// Check if required destruction conditions are met
		CheckDestroy();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the object needs to be destructed or not, and does it if needed.
	/// </summary>
	protected virtual void CheckDestroy() {
		// Depends on mode
		if(m_mode == Mode.SECONDS && m_seconds <= 0f) {
			DoDestroy();
		} else if(m_mode == Mode.FRAMES && m_frames <= 0) {
			DoDestroy();
		}
	}

	/// <summary>
	/// Do the destruction!
	/// </summary>
	protected virtual void DoDestroy() {
		// Must be replaced by another prefab?
		if(!string.IsNullOrEmpty(m_replacementPrefab)) {
			GameObject prefab = Resources.Load<GameObject>(m_replacementPrefab);
			if(prefab != null) {
				// Instantiate and copy transform and layer
				GameObject instance = Instantiate<GameObject>(prefab, this.transform.parent, false);
				instance.transform.CopyFrom(this.transform);
				instance.layer = this.gameObject.layer;
			}
		}

		// Do the actual destruction
		DestroyObject(this.gameObject);
	}

	//------------------------------------------------------------------------//
	// UTILS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add a SelfDestroy component to a GameObject with parameters.
	/// </summary>
	/// <param name="_obj">Target object.</param>
	/// <param name="_lifeTime">Seconds or frames depending on <paramref name="_mode"/></param>
	/// <param name="_mode">Frames or seconds?</param>
	/// <returns></returns>
	public static SelfDestroy AddToObject(GameObject _obj, float _lifeTime, Mode _mode) {
		// To avoid SelfDestroy destroying himself in the Awake call before configuring the duration, disable parent object :)
		bool wasActive = _obj.activeSelf;
		_obj.SetActive(false);

		// Add the component
		SelfDestroy destructor = _obj.AddComponent<SelfDestroy>();

		// Configure
		switch(_mode) {
			case Mode.SECONDS: {
				destructor.seconds = _lifeTime;
			} break;

			case Mode.FRAMES: {
				destructor.frames = Mathf.RoundToInt(_lifeTime);
			} break;
		}

		// SelfDestroy properly configured. We can now reactivate parent object.
		_obj.SetActive(wasActive);

		// Done!
		return destructor;
	}
}