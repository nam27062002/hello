// SelectableButton.cs
// 
// Created by Alger Ortín Castellví on 25/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Add an extra state to the Button component (selected), mostly used for tab-like behaviour.
/// Will override the "Disabled" state (aka a button cannot be selected AND disabled at the same time).
/// Simple implementation, won't work properly if misused.
/// </summary>
[RequireComponent(typeof(Button))]
public class SelectableButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	// Support the 3 transition types, store original value
	[SerializeField] private Color m_transitionColor = Colors.green;
	private Color m_transitionColorDisabled = Colors.gray;

	[SerializeField] private string m_transitionAnimationTrigger = "Selected";
	private string m_transitionAnimationTriggerDisabled = "Disabled";

	[SerializeField] private Sprite m_transitionSprite = null;
	private Sprite m_transitionSpriteDisabled = null;

	// Internal
	private bool m_selected = false;
	public bool selected {
		get { return m_selected; }
	}

	private Button m_button = null;
	public Button button {
		get { 
			if(m_button == null) {
				// Store button ref
				m_button = GetComponent<Button>();

				// Init backup values
				if(m_button != null) {
					m_transitionColorDisabled = m_button.colors.disabledColor;
					m_transitionAnimationTriggerDisabled = m_button.animationTriggers.disabledTrigger;
					m_transitionSpriteDisabled = m_button.spriteState.disabledSprite;
				}
			}
			return m_button;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Sets the selected state.
	/// </summary>
	/// <param name="_selected">Whether to select or unselect the button.</param>
	/// <param name="_stayDisabled">If leaving selection state, whether to go to normal state or keep the button disabled. Doesn't apply if <paramref name="_selected"/> is <c>true</c></param>
	public void SetSelected(bool _selected, bool _stayDisabled = false) {
		// Change button's transition
		// Color
		ColorBlock newColors = button.colors;
		newColors.disabledColor = _selected ? m_transitionColor : m_transitionColorDisabled;
		button.colors = newColors;

		// Trigger
		AnimationTriggers newTriggers = button.animationTriggers;
		newTriggers.disabledTrigger = _selected ? m_transitionAnimationTrigger : m_transitionAnimationTriggerDisabled;
		button.animationTriggers = newTriggers;

		// Sprite
		SpriteState newSprites = button.spriteState;
		newSprites.disabledSprite = _selected ? m_transitionSprite : m_transitionSpriteDisabled;
		button.spriteState = newSprites;

		// Set button's disabled state
		// Delay it until the end of frame, otherwise the transition setup wont be updated in time
		// Make it interactable by default, will be re-applied on the coroutine
		bool willBeInteractable = _selected ? false : !_stayDisabled;	// Disabled if selected or forced by flag
		button.interactable = willBeInteractable;

		// Reset selection state (so the proper transition is triggered)
		if(m_selected != _selected) {
			if(_selected) {
				button.OnSelect(new BaseEventData(EventSystem.current));
			} else {
				button.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}

		// Consume trigger to reset animation state
		if(button.transition == Selectable.Transition.Animation) {
			// button.animator.ResetTrigger(m_transitionAnimationTrigger);
			if(_selected) {
				button.animator.SetTrigger("Selected");
			} else {
				button.animator.SetTrigger("Normal");
				button.animator.ResetTrigger("Highlighted");
				button.animator.ResetTrigger("Selected");
			}
		}

		m_selected = _selected;
	}
}