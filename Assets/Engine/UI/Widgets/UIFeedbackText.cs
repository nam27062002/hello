// UIFeedbackText.cs
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom UI game object with a text and a simple in-out animation.
/// Extends from Unity's text component for more flexibility.
/// TODO!! Pool
/// </summary>
[RequireComponent(typeof(Text))]
public class UIFeedbackText : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static string DEFAULT_PREFAB_PATH = "UI/Common/Generic/PF_UIFeedbackText";	// Just for comfort, change it if path changes
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed Setup
	[SerializeField] private float m_duration = 2f;
	public float duration {
		get { return m_duration; }
	}

	// Internal
	private Sequence m_sequence = null;
	public Sequence sequence {
		get { return m_sequence; }
	}

	private Text m_text = null;
	public Text text {
		get { return m_text; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get Text reference
		m_text = GetComponent<Text>();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Just in case
		ClearSequence();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHOS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Creates the sequence anew.
	/// </summary>
	public void GenerateSequence() {
		// Clear current sequence
		ClearSequence();

		// Aux vars
		CanvasScaler parentCanvas = GetComponentInParent<CanvasScaler>();

		// Compute different steps duration
		float[] durations = new float[] {
			m_duration * 0.1f,	// In
			m_duration * 0.2f,	// Idle
			m_duration * 0.7f	// Out
		};

		// Create sequence, pause it
		m_sequence = DOTween.Sequence()
			// In
			.Append(m_text.rectTransform.DOScale(0f, durations[0]).From().SetEase(Ease.OutBack))
			.Join(m_text.DOFade(0f, durations[0] * 0.75f).From())

			// Idle
			.AppendInterval(durations[1])

			// Out
			.Append(m_text.rectTransform.DOBlendableLocalMoveBy(Vector3.up * parentCanvas.referenceResolution.y * 0.25f, durations[2]).SetEase(Ease.InCubic))
			.Join(m_text.DOFade(0f, durations[2]).SetEase(Ease.InCubic))

			// Sequence setup and pause
			.OnComplete(() => { GameObject.Destroy(this.gameObject); })	// Self-destroy on end
			.Pause();
	}

	/// <summary>
	/// Kill and destroy the current sequence, if any.
	/// </summary>
	private void ClearSequence() {
		// Just in case
		if(m_sequence != null) {
			m_sequence.Kill();
			m_sequence = null;
		}
	}

	//------------------------------------------------------------------------//
	// FACTORY METHOS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create a new UI text feedback using an object as position reference and 
	/// attach it to the given RectTransform.
	/// </summary>
	/// <param name="_text">The text to be displayed.</param>
	/// <param name="_anchor">The transform used as a reference to define the spawn position of the feedback.</param>
	/// <param name="_offset">The position offset relative to the <paramref name="_anchor"/>'s position.</param>
	/// <param name="_parent">The transform to attach the new text feedback to.</param>
	/// <param name="_name">The name to be given to the new text feedback. If another UIFeedbackText with the same name already exists on the target rect transform, it will be instantly killed.</param>
	/// <param name="_prefabPath">The prefab to be used for this notification. If not defined, the default one will be used.</param>
	public static UIFeedbackText CreateAndLaunch(string _text, RectTransform _anchor, Vector2 _offset, RectTransform _parent, string _name = "UIFeedbackText", string _prefabPath = "") {
		// Don't do anything if parent is not valid
		if(_parent == null) return null;

		// Convert anchor's transform coords to parent's transform coords
		// 1) Target position in anchor's local coordinates
		Vector3 pos = new Vector3(
			_offset.x,
			_offset.y,
			0f
		);

		// 2) Position in world coords
		pos = _anchor.TransformPoint(pos);

		// 3) Position in parent's local coords
		pos = _parent.InverseTransformPoint(pos);

		// Use the other factory method
		return CreateAndLaunchLocal(_text, pos, _parent, _name, _prefabPath);
	}

	/// <summary>
	/// Create a new UI text feedback using a global viewport position and attach 
	/// it to the given RectTransform.
	/// </summary>
	/// <param name="_text">The text to be displayed.</param>
	/// <param name="_viewportPos">The initial position of the text, relative parent canvas coords [(0,0)..(1,1)].</param>
	/// <param name="_parent">The transform to attach the new text feedback to.</param>
	/// <param name="_name">The name to be given to the new text feedback. If another UIFeedbackText with the same name already exists on the target rect transform, it will be instantly killed.</param>
	/// <param name="_prefabPath">The prefab to be used for this notification. If not defined, the default one will be used.</param>
	public static UIFeedbackText CreateAndLaunch(string _text, Vector2 _viewportPos, RectTransform _parent, string _name = "UIFeedbackText", string _prefabPath = "") {
		// Don't do anything if parent is not valid
		if(_parent == null) return null;

		// Convert viewport coords to parent's transform coords
		// Find parent canvas to perform the viewport-world transformation
		Canvas canvas = _parent.GetComponentInParent<Canvas>();
		RectTransform rt = canvas.transform as RectTransform;

		// 1) Position in canvas root local coordinates
		Vector3 pos = new Vector3(
			rt.rect.x + rt.rect.width * _viewportPos.x,
			rt.rect.y + rt.rect.height * _viewportPos.y,
			0f
		);

		// 2) Position in world coords
		pos = rt.TransformPoint(pos);

		// 3) Position in parent's local coords
		pos = _parent.InverseTransformPoint(pos);

		// Use the other factory method
		return CreateAndLaunchLocal(_text, pos, _parent, _name, _prefabPath);
	}

	/// <summary>
	/// Create a new UI text feedback using and attach it to the given RectTransform.
	/// </summary>
	/// <param name="_parent">The transform to attach the new text feedback to.</param>
	/// <param name="_pos">The initial position of the text, local coords relative to parent transform.</param>
	/// <param name="_name">The name to be given to the new text feedback. If another UIFeedbackText with the same name already exists on the target rect transform, it will be instantly killed.</param>
	/// <param name="_prefabPath">The prefab to be used for this notification. If not defined, the default one will be used.</param>
	public static UIFeedbackText CreateAndLaunchLocal(string _text, Vector3 _pos, RectTransform _parent, string _name = "UIFeedbackText", string _prefabPath = "") {
		// Don't do anything if parent is not valid
		if(_parent == null) return null;

		// Load prefab
		if(string.IsNullOrEmpty(_prefabPath)) _prefabPath = DEFAULT_PREFAB_PATH;
		GameObject prefab = Resources.Load<GameObject>(_prefabPath);
		Debug.Assert(prefab != null, "Prefab " + _prefabPath + " for UIFeedbackText not found!");

		// If another UITextFeedback with the same name and parent already exists, destroy it
		GameObject oldObj = _parent.FindObjectRecursive(_name);
		if(oldObj != null) {
			GameObject.Destroy(oldObj);
		}

		// Create a new instance
		GameObject newObj = GameObject.Instantiate<GameObject>(prefab);
		newObj.name = _name;

		// Initialize the UIFeedbackText component
		UIFeedbackText feedbackText = newObj.GetComponent<UIFeedbackText>();
		Debug.Assert(feedbackText != null,  "Prefab " + _prefabPath + " doesn't have a UIFeedbackText component!");
		feedbackText.text.text = _text;

		// Attach to parent
		RectTransform notificationRt = feedbackText.transform as RectTransform;
		notificationRt.SetParent(_parent, false);

		// Set initial position
		notificationRt.localPosition = _pos;

		// Generate and start animation
		feedbackText.GenerateSequence();
		feedbackText.sequence.Play();

		return feedbackText;
	}
}