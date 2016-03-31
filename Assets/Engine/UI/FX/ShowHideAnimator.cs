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

		CUSTOM
	}

	protected enum State {
		INIT,
		VISIBLE,
		HIDDEN
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Config
	// References
	[Comment("Optional, must have triggers \"show\" and \"hide\"")]
	[Separator("Animator", 20f)]
	[SerializeField] protected Animator m_animator = null;

	// Tween params
	// All tween-related parameters will be ignored if an animator is defined.
	// Feel free to add new tween types or extra parameters
	[Separator("Tween Setup", 20f)]
	[SerializeField] protected TweenType m_tweenType = TweenType.NONE;	// Define the "show" direction. "hide" will be the reversed tween. To use CUSTOM, add as many DOTweenAnimation components as desired to the target object with the id's "show" and "hide".
	[SerializeField] protected float m_tweenDuration = 0.25f;
	[SerializeField] protected float m_tweenValue = 1f;					// Use it to tune the animation (e.g. offset for move tweens, scale factor for the scale tweens, initial alpha for fade tweens).
	[SerializeField] protected Ease m_tweenEase = Ease.OutBack;
	[SerializeField] protected float m_tweenDelay = 0f;

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
		m_canvasGroup = GetComponent<CanvasGroup>();
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

		// If not using animations, put sequence to the end point and return
		if(!_animate) {
			if(m_sequence != null) m_sequence.Goto(1f);
			return;
		}

		// If using an animator, let it do all the work
		if(m_animator != null) {
			// Just activate the right trigger
			m_animator.SetTrigger("show");
			return;
		}

		// If tween type is set to NONE, nothing else to do
		if(m_tweenType == TweenType.NONE) return;

		// If using custom tween animators, just let the tween engine manage it
		if(m_tweenType == TweenType.CUSTOM) {
			DOTween.Restart(gameObject, "show");
			return;
		}

		// Using tweens, play the sequence in the proper direction
		if(m_sequence != null) {
			m_sequence.PlayForward();		// The cool thing is that if the hide animation is interrupted, the show animation will start from the interruption point
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

		// If not using animations, put sequence to the start point and instantly disable the object
		if(!_animate) {
			if(m_sequence != null) {
				m_sequence.Goto(0f);
				if(_disableAfterAnimation) gameObject.SetActive(false);
			} else {
				gameObject.SetActive(false);
			}
			return;
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

		// If using custom or none types, we won't use the sequence
		if(m_tweenType == TweenType.NONE || m_tweenType == TweenType.CUSTOM) return;

		// If the object doesn't have a canvas group, add it to be able to fade it in/out - all tween types will use it
		if(m_canvasGroup == null) m_canvasGroup = gameObject.AddComponent<CanvasGroup>();

		// Create new sequence
		m_sequence = DOTween.Sequence()
			.SetAutoKill(false)
			.OnStepComplete(() => { OnTweenCompleted(); });

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
	protected virtual void OnTweenCompleted() {
		// Optionally disable object after the hide animation has finished
		if(!visible) {
			if(m_disableAfterHide) {
				gameObject.SetActive(false);
			}
		}
	}
}

