// AdvancedButton.cs
// 
// Created by Alger Ortín Castellví on 25/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System;
using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension of the default Unity UI Button with extra functionality.
/// - Recursive tint on all children (when using Color Tint as transition method)
/// </summary>
[AddComponentMenu("UI/Button Extended", 30)]
public class ButtonExtended : Button {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Multitouch avoidance
    [SerializeField]
    [Tooltip("Disables the posibility of several buttons can be pushed at same time.")]
    public bool m_MultiTouchDisable = true;

    private ButtonClickedEvent m_eventBackup = new ButtonClickedEvent();
    private static bool m_buttonMultitouchProtector = false;

    public static bool checkMultitouchAvailability()
    {
        Debug.Log(">>>>> checkMultitouchAvailability()");
        if (m_buttonMultitouchProtector) return false;
        Debug.Log(">>>>> enter");
        m_buttonMultitouchProtector = true;
        CoroutineManager.Instance.StartCoroutine(WaitAMoment(0.75f));
        return true;
    }

    static IEnumerator WaitAMoment(float time)
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(time);
        m_buttonMultitouchProtector = false;
        Debug.Log(">>>>> exit");
    }

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// First update call.
    /// </summary>
    private void Start()
    {
        for (int i = 0; i < onClick.GetPersistentEventCount(); i++)
        {
            object ev = onClick.GetPersistentTarget(i);
            m_eventBackup.AddListener(ev as UnityAction);
        }
        onClick.RemoveAllListeners();
        onClick.AddListener(safeOnclick);
    }

    void safeOnclick()
    {
        if (m_MultiTouchDisable && !checkMultitouchAvailability()) return;
        m_eventBackup.Invoke();
    }

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
		Graphic[] graphics = targetGraphic.transform.GetComponentsInChildren<Graphic>();
		for(int i = 0; i < graphics.Length; i++) {
			graphics[i].CrossFadeColor(targetColor, (!_instant) ? this.colors.fadeDuration : 0f, true, true);
		}
	}
}