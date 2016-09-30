// ShowHideAnimator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to show/hide the target game object using either an animator,
/// a predefined set of tweens, or custom tweens.
/// TODO!!
/// 	- Support for non-symmetric animations
/// </summary>
[RequireComponent(typeof(RectTransform))]	// Only for UI objects for now!
public class ShowHideAnimator : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum TweenType {
		NONE,

		IDLE,	// Special one used to delay the instant show/hide of the object (for example when waiting for other animations to finish)

		FADE,

		UP,
		DOWN,
		LEFT,
		RIGHT,

		SCALE,

		CUSTOM,

		ANIMATOR
	}

	protected enum State {
		INIT,
		VISIBLE,
		HIDDEN
	}

	[System.Serializable]
	public class ShowHideAnimatorEvent : UnityEvent<ShowHideAnimator> { }

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Animation type
	// For tweens, defines the "show" direction; "hide" will be the reversed tween. 
	// For CUSTOM, add as many DOTweenAnimation components as desired to the target object and put them in the show() and hide() arrays.
	// For animators, just link the animator to be used. Must have a "show" and "hide" triggers.
	[SerializeField] protected TweenType m_tweenType = TweenType.NONE;
	public TweenType tweenType { get { return m_tweenType; }}

	// Config
	// Tween params
	// All tween-related parameters will be ignored if an animator is defined.
	// Feel free to add new tween types or extra parameters
	[SerializeField] protected float m_tweenDuration = 0.25f;
	public float tweenDuration { get { return m_tweenDuration; }}

	[SerializeField] protected float m_tweenValue = 1f;					// Use it to tune the animation (e.g. offset for move tweens, scale factor for the scale tweens, initial alpha for fade tweens).
	public float tweenValue { get { return m_tweenValue; }}

	[SerializeField] protected Ease m_tweenEase = Ease.OutBack;
	public Ease tweenEase { get { return m_tweenEase; }}

	[SerializeField] protected float m_tweenDelay = 0f;
	public float tweenDelay { get { return m_tweenDelay; }}

	// Custom tweens
	[SerializeField] protected DOTweenAnimation[] m_showTweens = new DOTweenAnimation[0];
	[SerializeField] protected DOTweenAnimation[] m_hideTweens = new DOTweenAnimation[0];

	// Animator param
	[Comment("Must have triggers \"show\" and \"hide\"")]
	[SerializeField] protected Animator m_animator = null;

	// Events
	public ShowHideAnimatorEvent OnShowPreAnimation = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnShowPostAnimation = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnHidePreAnimation = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnHidePostAnimation = new ShowHideAnimatorEvent();

	// Internal references
	protected CanvasGroup m_canvasGroup = null;	// Not required, if the object has no animator nor a canvas group, it will be automatically added
	protected RectTransform m_rectTransform = null;

	// Internal
	protected Sequence m_sequence = null;	// We will reuse the same tween and play it forward/backwards accordingly
	protected bool m_isDirty = true;
	protected bool m_disableAfterHide = true;

	// Since visibility is linked to object's being active, we cannot trust in initializing it properly on the Awake call (since Awake is not called for disabled objects)
	// Forced to do this workaround
	protected State m_state = State.INIT;
	public bool visible {
		get { 
			if(m_state == State.INIT) {
				if(gameObject.activeSelf) {
					m_state = State.VISIBLE;
				} else {
					m_state = State.HIDDEN;
				}
			}
			return m_state == State.VISIBLE; 
		}
	}

	// Public properties
	// Sequence delta, only for tween animations
	public float delta {
		get {
			if(m_sequence == null) {
				return 0f;
			} else {
				return m_sequence.ElapsedPercentage(false); 
			}
		}
		set { 
			if(m_sequence == null) {
				return;
			} else {
				m_sequence.Goto(Mathf.Clamp01(value), m_sequence.IsPlaying()); 
			}
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Get external references
		m_rectTransform = GetComponent<RectTransform>();
	}

	/// <summary>
	/// First update.
	/// </summary>
	protected void Start() {
		
	}

	/// <summary>
	/// A change has been made on the inspector.
	/// http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnValidate.html
	/// </summary>
	protected void OnValidate() {
		// Mark the object as dirty so that the next time an animation is required the sequence is re-created
		m_isDirty = true;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Just in case
		if(m_sequence != null) {
			m_sequence.Kill();
			m_sequence = null;
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show the object. Will be ignored if object is already visible/showing.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	public virtual void Show(bool _animate = true) {
		// If we're already in the target state, skip (unless dirty, in which case we want to place the animation sequence at the right place)
		if(visible && !m_isDirty) return;

		// Update state
		m_state = State.VISIBLE;

		// In any case, make sure the object is active
		gameObject.SetActive(true);

		// If dirty, re-create the tween (will be destroyed if not needed)
		if(m_isDirty) RecreateTween();

		// Broadcast pre-animation event
		OnShowPreAnimation.Invoke(this);

		// If not using animations, put sequence to the end point and return
		if(!_animate) {
			if(m_sequence != null) m_sequence.Goto(1f);

			// Immediately broadcast post-animation event
			OnShowPostAnimation.Invoke(this);
			return;
		}

		// Perform different actions depending on the selected animation type
		switch(m_tweenType) {
			case TweenType.ANIMATOR: {
				// If using an animator, let it do all the work
				if(m_animator != null) {
					// Just activate the right trigger
					m_animator.SetTrigger("show");
				}
			} break;

			case TweenType.NONE: {
				// Nothing else to do, Immediately broadcast post-animation event
				OnShowPostAnimation.Invoke(this);
			} break;

			case TweenType.CUSTOM: {
				// Stop all running hide animators
				for(int i = 0; i < m_hideTweens.Length; i++) {
					m_hideTweens[i].DOPause();
				}

				// If using custom tween animators, restart all the animators in the show array
				for(int i = 0; i < m_showTweens.Length; i++) {
					m_showTweens[i].DORestart();
				}
			} break;

			default: {
				// Using tweens, play the sequence in the proper direction
				if(m_sequence != null) {
					m_sequence.PlayForward();		// The cool thing is that if the hide animation is interrupted, the show animation will start from the interruption point
				}
			} break;
		}
	}

	/// <summary>
	/// Same as show but overriding current state.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	public void ForceShow(bool _animate = true) {
		// Force state to make sure Show() call is not skipped
		m_state = State.HIDDEN;
		Show(_animate);
	}

	/// <summary>
	/// Hide the object. Will be ignored if object is already hidden/hiding.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public virtual void Hide(bool _animate = true, bool _disableAfterAnimation = true) {
		// If we're already in the target state, skip (unless dirty, in which case we want to place the animation sequence at the right place)
		if(!visible && !m_isDirty) return;

		// Update state
		m_state = State.HIDDEN;
		m_disableAfterHide = _disableAfterAnimation;

		// If dirty, re-create the tween (will be destroyed if not needed)
		if(m_isDirty) {
			RecreateTween();

			// Since we're going backwards, initialize the new sequence at the end
			if(m_sequence != null) m_sequence.Goto(1f);
		}

		// Broadcast pre-animation event
		OnHidePreAnimation.Invoke(this);

		// If not using animations, put sequence to the start point and instantly disable the object
		if(!_animate) {
			if(m_sequence != null) {
				m_sequence.Goto(0f);
				if(_disableAfterAnimation) gameObject.SetActive(false);
			} else {
				gameObject.SetActive(false);
			}

			// Immediately broadcast post-animation event
			OnHidePostAnimation.Invoke(this);
			return;
		}

		// Perform different actions depending on the selected animation type
		switch(m_tweenType) {
			case TweenType.ANIMATOR: {
				// If using an animator, let it do all the work
				if(m_animator != null) {
					// Just activate the right trigger
					m_animator.SetTrigger("hide");
				}
			} break;

			case TweenType.NONE: {
				// If tween type is set to NONE, just disable the object
				gameObject.SetActive(false);

				// Immediately broadcast post-animation event
				OnHidePostAnimation.Invoke(this);
			} break;

			case TweenType.CUSTOM: {
				// Stop all running hide animators
				for(int i = 0; i < m_showTweens.Length; i++) {
					m_showTweens[i].DOPause();
				}

				// If using custom tween animators, restart all the animators in the hide event
				for(int i = 0; i < m_hideTweens.Length; i++) {
					m_hideTweens[i].DORestart();
				}
			} break;

			default: {
				// Using tweens, play the sequence in the proper direction
				if(m_sequence != null) {
					m_sequence.PlayForward();		// The cool thing is that if the hide animation is interrupted, the show animation will start from the interruption point
				}
			} break;
		}

		// If using an animator, let it do all the work
		if(m_animator != null) {
			// Just activate the right trigger
			m_animator.SetTrigger("hide");
			return;
		}

		// If tween type is set to NONE, just disable the object
		if(m_tweenType == TweenType.NONE) {
			gameObject.SetActive(false);
			return;
		}

		// If using custom tween animators, just let the tween engine manage it
		if(m_tweenType == TweenType.CUSTOM) {
			DOTween.Restart(gameObject, "hide");
			return;
		}

		// Using tweens, play the sequence in the proper direction
		if(m_sequence != null) {
			m_sequence.PlayBackwards();	// The cool thing is that if the show animation is interrupted, the hide animation will start from the interruption point
		}
	}

	/// <summary>
	/// Same as hide but overriding current state.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public void ForceHide(bool _animate = true, bool _disableAfterAnimation = true) {
		// Force state to make sure Hide() call is not skipped
		m_state = State.VISIBLE;
		Hide(_animate, _disableAfterAnimation);
	}

	/// <summary>
	/// Toggle visibility state.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	public void Toggle(bool _animate = true) {
		// Easy
		if(visible) {
			Hide(_animate);
		} else {
			Show(_animate);
		}
	}

	/// <summary>
	/// Set the specified visibility state.
	/// Will be ignored if the object is already at the target state.
	/// </summary>
	/// <param name="_visible">Whether to show or hide the object.</param>
	/// <param name="_animate">Whether to use animations or not.</param>
	public void Set(bool _visible, bool _animate = true) {
		// Easy
		if(_visible) {
			Show(_animate);
		} else {
			Hide(_animate);
		}
	}

	/// <summary>
	/// Same as Set() but overriding current state.
	/// </summary>
	/// <param name="_visible">Whether to show or hide the object.</param>
	/// <param name="_animate">Whether to use animations or not.</param>
	public void ForceSet(bool _visible, bool _animate = true) {
		// Easy
		if(_visible) {
			ForceShow(_animate);
		} else {
			ForceHide(_animate);
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create the tween sequence according to current setup.
	/// If a sequence already exists, it will be killed.
	/// </summary>
	protected void RecreateTween() {
		// If the sequence is already created, kill it
		if(m_sequence != null) {
			m_sequence.Complete();	// Make sure sequence is at its end-state to restore object's default values so the new sequence can take them
			m_sequence.Kill();
			m_sequence = null;
		}

		// Make sure we have required components
		if(m_rectTransform == null) {
			m_rectTransform = GetComponent<RectTransform>();
		}

		// Clear dirty flag
		m_isDirty = false;

		// If animator is defined, we won't use tweens
		if(m_animator != null) return;
		
		// If using no animation at all, we're done
		if(m_tweenType == TweenType.NONE) return;

		// If using a custom set of tweens, add listeners to them and return
		if(m_tweenType == TweenType.CUSTOM) {
			// Register both show and hide callbacks to the OnComplete event
			for(int i = 0; i < m_showTweens.Length; i++) {
				// Remove+Add to make sure it's added only once
				m_showTweens[i].onComplete.RemoveListener(OnShowTweenCompleted);
				m_showTweens[i].onComplete.AddListener(OnShowTweenCompleted);

				// Some extra setup
				m_showTweens[i].autoKill = false;
				m_showTweens[i].autoPlay = false;
			}

			for(int i = 0; i < m_hideTweens.Length; i++) {
				// Remove+Add to make sure it's added only once
				m_hideTweens[i].onComplete.RemoveListener(OnHideTweenCompleted);
				m_hideTweens[i].onComplete.AddListener(OnHideTweenCompleted);

				// Some extra setup
				m_hideTweens[i].autoKill = false;
				m_hideTweens[i].autoPlay = false;
			}
		}

		// If the object doesn't have a canvas group, add it to be able to fade it in/out - all tween types will use it
		if(m_canvasGroup == null) {
			// Try to fetch an existing canvas, create a new one if not found
			m_canvasGroup = this.gameObject.ForceGetComponent<CanvasGroup>();
		}

		// Create new sequence
		m_sequence = DOTween.Sequence()
			.SetAutoKill(false)
			.OnStepComplete(() => { OnSequenceCompleted(); });

		// Shared parameters
		TweenParams sharedParams = new TweenParams()
			.SetEase(m_tweenEase);

		// Initialize based on current parameters
		switch(m_tweenType) {
			case TweenType.IDLE: {
				m_sequence.PrependInterval(m_tweenDuration);	// Do nothing else than adding an idle interval
			} break;

			case TweenType.FADE: {
				m_tweenValue = Mathf.Clamp01(m_tweenValue);
				m_sequence.Join(m_canvasGroup.DOFade(m_tweenValue, m_tweenDuration).SetAs(sharedParams).From());
			} break;

			case TweenType.UP: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, m_tweenDuration).SetAs(sharedParams).From());
				m_sequence.Join(m_rectTransform.DOBlendableLocalMoveBy(Vector3.down * m_tweenValue, m_tweenDuration).SetAs(sharedParams).From());
			} break;

			case TweenType.DOWN: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, m_tweenDuration).SetAs(sharedParams).From());
				m_sequence.Join(m_rectTransform.DOBlendableLocalMoveBy(Vector3.up * m_tweenValue, m_tweenDuration).SetAs(sharedParams).From());
			} break;

			case TweenType.LEFT: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, m_tweenDuration).SetAs(sharedParams).From());
				m_sequence.Join(m_rectTransform.DOBlendableLocalMoveBy(Vector3.right * m_tweenValue, m_tweenDuration).SetAs(sharedParams).From());
			} break;

			case TweenType.RIGHT: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, m_tweenDuration).SetAs(sharedParams).From());
				m_sequence.Join(m_rectTransform.DOBlendableLocalMoveBy(Vector3.left * m_tweenValue, m_tweenDuration).SetAs(sharedParams).From());
			} break;

			case TweenType.SCALE: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, m_tweenDuration).SetAs(sharedParams).From());
				m_sequence.Join(m_rectTransform.DOScale(m_tweenValue, m_tweenDuration).SetAs(sharedParams).From());
			} break;
		}

		// Insert delay at the beginning of the sequence
		m_sequence.PrependInterval(m_tweenDelay);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Either a show or a hide animation has finished.
	/// Won't be called when animation is interrupted.
	/// </summary>
	protected virtual void OnSequenceCompleted() {
		// Which animation has finished?
		if(visible) {
			// Show animation, broadcast event
			OnShowPostAnimation.Invoke(this);
		} else {
			// Hide animation, broadcast event
			OnHidePostAnimation.Invoke(this);

			// Optionally disable object after the hide animation has finished
			if(m_disableAfterHide) {
				gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// A tween from the CUSTOM tweens array has finished.
	/// </summary>
	private void OnShowTweenCompleted() {
		// Check all hide tweens, if all of them are completed, process end of animation
		for(int i = 0; i < m_showTweens.Length; i++) {
			// If it's active, no need to keep checking
			if(m_showTweens[i].isActive) {
				return;
			}
		}

		// No more active tweens!
		// Broadcast event
		OnShowPostAnimation.Invoke(this);
	}

	/// <summary>
	/// A tween from the CUSTOM tweens array has finished.
	/// </summary>
	private void OnHideTweenCompleted() {
		// Check all hide tweens, if all of them are completed, process end of animation
		for(int i = 0; i < m_hideTweens.Length; i++) {
			// If it's active, no need to keep checking
			if(m_hideTweens[i].isActive) {
				return;
			}
		}

		// No more active tweens!
		// Broadcast event
		OnHidePostAnimation.Invoke(this);

		// Optionally disable object after the hide animation has finished
		if(m_disableAfterHide) {
			gameObject.SetActive(false);
		}
	}
}

