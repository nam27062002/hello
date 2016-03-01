// MultiOptionButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// UI widget in the form of a foldable sub-menu appering from a button.
/// Implements the IDeselectHandler to detect clicks outside the button.
/// http://docs.unity3d.com/ScriptReference/EventSystems.IDeselectHandler.html
/// TODO!!:
/// - Drag interaction?
/// </summary>
[RequireComponent(typeof(Button))]
public class MultiOptionButton : MonoBehaviour, IDeselectHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Button[] m_subButtons;

	// Other refs
	private Button m_rootButton;

	// Internal logic
	private bool m_closed = true;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_rootButton = GetComponent<Button>();
		Debug.Assert(m_rootButton != null, "Required component!");
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Start closed
		m_closed = true;

		// Link main button to Open method
		m_rootButton.onClick.AddListener(Toggle);

		// Initialize sub buttons
		for(int i = 0; i < m_subButtons.Length; i++) {
			// Start with all the buttons disabled
			m_subButtons[i].gameObject.SetActive(false);

			// Add a callback to disable buttons again after close animation has finished
			DOTweenAnimation[] anims = m_subButtons[i].gameObject.GetComponents<DOTweenAnimation>();
			for(int j = 0; j < anims.Length; j++) {
				if(anims[j].id == "close") {
					anims[j].tween.OnComplete(() => { OnCloseAnimCompleted(m_subButtons[i]); });	// [AOC] Lambda expressions (puke) (see http://dotween.demigiant.com/documentation.php "OnComplete")
					break;
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launch the unfold animation.
	/// Will interrupt any active animation.
	/// </summary>
	public void Open() {
		// Only if closed
		if(!m_closed) return;
		m_closed = false;

		// Activate all buttons and launch their open animation
		for(int i = 0; i < m_subButtons.Length; i++) {
			m_subButtons[i].gameObject.SetActive(true);
			DOTween.Pause(m_subButtons[i].gameObject);
			DOTween.Restart(m_subButtons[i].gameObject, "open");
		}
	}

	/// <summary>
	/// Launch the fold animation.
	/// Will interrupt any active animation.
	/// </summary>
	public void Close() {
		// Only if not already closed
		if(m_closed) return;
		m_closed = true;

		// Activate all buttons and launch their open animation
		for(int i = 0; i < m_subButtons.Length; i++) {
			DOTween.Pause(m_subButtons[i].gameObject);
			DOTween.Restart(m_subButtons[i].gameObject, "close");
		}
	}

	/// <summary>
	/// Toggles between opened and closed states.
	/// </summary>
	public void Toggle() {
		if(m_closed) {
			Open();
		} else {
			Close();
		}
	}

	//------------------------------------------------------------------//
	// IDeselectHandler IMPLEMENTATION									//
	//------------------------------------------------------------------//
	/// <summary>
	/// A click has occurred outside this object after it has been clicked.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public void OnDeselect(BaseEventData _eventData) {
		Close();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The close animation on a button has finished.
	/// </summary>
	/// <param name="_targetBtn">The button whose animation has finished.</param>
	public void OnCloseAnimCompleted(Button _targetBtn) {
		// Just disable button
		_targetBtn.gameObject.SetActive(false);
	}
}