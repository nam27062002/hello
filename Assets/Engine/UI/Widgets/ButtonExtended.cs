// AdvancedButton.cs
// 
// Created by Alger Ortín Castellví on 25/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension of the default Unity UI Button with extra functionality
/// </summary>
[AddComponentMenu("UI/Button Extended", 30)]
public class ButtonExtended : Button {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Perform a state transition on the button.
	/// </summary>
	/// <param name="_state">The target state.</param>
	/// <param name="_instant">Whether to animate or not.</param>
	protected override void DoStateTransition(SelectionState _state, bool _instant) {
		// Based on http://answers.unity3d.com/questions/820311/ugui-multi-image-button-transition.html
		// If transition type is different from ColorTint, let parent manage it
		if(this.transition != Selectable.Transition.ColorTint) {
			base.DoStateTransition(_state, _instant);
			return;
		}

		// Do our custom color tint transition!
		// Skip if we don't have a valid target graphic!
		if(this.targetGraphic == null) return;

		// Only if object is active!
		if(!base.gameObject.activeInHierarchy) return;

		// Figure out tint color based on target state
		Color targetColor = this.colors.normalColor;
		switch(_state) {
			case Selectable.SelectionState.Normal:		targetColor = this.colors.normalColor; 		break;
			case Selectable.SelectionState.Highlighted:	targetColor = this.colors.highlightedColor;	break;
			case Selectable.SelectionState.Pressed:		targetColor = this.colors.pressedColor;		break;
			case Selectable.SelectionState.Disabled:	targetColor = this.colors.disabledColor;	break;
			default:									targetColor = Color.black;					break;
		}

		// Apply multiply factor
		targetColor = targetColor * this.colors.colorMultiplier;

		// Iterate all children graphics and apply the same color transition
		Transform root = targetGraphic.transform.parent != null ? targetGraphic.transform.parent : targetGraphic.transform;
		Graphic[] graphics = root.GetComponentsInChildren<Graphic>();
		for(int i = 0; i < graphics.Length; i++) {
			graphics[i].CrossFadeColor(targetColor, (!_instant) ? this.colors.fadeDuration : 0f, true, true);
		}
	}
}