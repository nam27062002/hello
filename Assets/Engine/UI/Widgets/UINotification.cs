// UINotification.cs
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class UINotification : ShowHideAnimator {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly string RESOURCES_PREFAB_PATH = "UI/Common/Generic/PF_UINotification";	// Just for comfort, change it if path changes
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private Sequence m_idleSequence = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected override void Awake() {
		// Call parent
		base.Awake();

		// Create sequence
		m_idleSequence = DOTween.Sequence()
			.SetLoops(-1, LoopType.Restart)
			.AppendInterval(1.5f)
			.Append(m_rectTransform.DOScale(1.25f, 0.15f).SetEase(Ease.InCubic))
			.Append(m_rectTransform.DOScale(1f, 1f).SetEase(Ease.OutBounce))
			.Pause();
	}

	//------------------------------------------------------------------------//
	// FACTORY METHOS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create a new UI notification using the default prefab and optionally attach 
	/// it to the given RectTransform.
	/// </summary>
	/// <param name="_parent">The transform to attach this notification to. Can be <c>null</c>.</param>
	public static UINotification Create(RectTransform _parent) {
		// Load and instantiate prefab
		GameObject prefab = Resources.Load<GameObject>(RESOURCES_PREFAB_PATH);
		GameObject newObj = GameObject.Instantiate<GameObject>(prefab);
		UINotification newNotification = newObj.GetComponent<UINotification>();

		// Attach to someone?
		if(_parent != null) {
			RectTransform notificationRt = newNotification.transform as RectTransform;
			notificationRt.SetParent(_parent, false);
			notificationRt.anchoredPosition = new Vector2(-_parent.rect.width * 0.4f, -_parent.rect.height * 0.4f);	// Assuming anchor is at the middle, this will place it on the top-left corner, a bit overlapped
		}

		return newNotification;
	}

	//------------------------------------------------------------------------//
	// ShowHideAnimator OVERRIDES											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	protected override void OnDestroy() {
		// Destroy sequence
		m_idleSequence.Kill(true);
		m_idleSequence = null;

		// Call parent
		base.OnDestroy();
	}

	/// <summary>
	/// Hide the object. Will be ignored if object is already hidden/hiding.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public virtual void Hide(bool _animate = true, bool _disableAfterAnimation = true) {
		// Pause to avoid conflicting with hide animation
		if(m_tweenType == TweenType.SCALE || m_tweenType == TweenType.CUSTOM) {
			m_idleSequence.Pause();
		}

		// Call parent
		base.Hide(_animate, _disableAfterAnimation);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Either a show or a hide animation has finished.
	/// Won't be called when animation is interrupted.
	/// </summary>
	protected override void OnTweenCompleted() {
		// Either start or stop the idle animation
		if(visible) {
			// Show animation has finished
			m_idleSequence.Restart();
		} else {
			// Hide animation has finished
			m_idleSequence.Pause();
		}

		// Call parent
		base.OnTweenCompleted();
	}
}