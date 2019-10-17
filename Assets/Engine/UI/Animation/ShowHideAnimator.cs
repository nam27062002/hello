// ShowHideAnimator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//
#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using System.Diagnostics;
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
/// In order to use a Unity animator, it is strongly recommended to create an Animation 
/// Override Controller on the provided Animation Controller ANC_ShowHideAnimator
/// and override it with custom animations based on the AN_ShowHideAnimator**** animations.
/// TODO!!
/// 	- Support for non-symmetric animations
/// </summary>
public class ShowHideAnimator : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Use to debug specific objects
	public const bool DEBUG_ENABLED = true;
	public const string DEBUG_TARGET = "ClassicDragonInfo";

	public enum TweenType {
		NONE,

		IDLE,	// Special one used to delay the instant show/hide of the object (for example when waiting for other animations to finish)

		FADE,

		UP,
		DOWN,
		LEFT,
		RIGHT,

		SCALE,
		DELTA_SIZE,

		CUSTOM,

		ANIMATOR,

		SCALE_X,
		SCALE_Y
	}

	public enum State {
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
	public TweenType tweenType { 
		get { return m_tweenType; }
		set { m_tweenType = value; m_isDirty = true; }
	}

	// Config
	// Tween params
	// All tween-related parameters will be ignored if an animator is defined.
	// Feel free to add new tween types or extra parameters
	[SerializeField] protected float m_tweenDuration = 0.25f;
	public float tweenDuration { 
		get { return m_tweenDuration; }
		set { m_tweenDuration = value; m_isDirty = true; }
	}

	[SerializeField] protected float m_tweenValue = 1f;					// Use it to tune the animation (e.g. offset for move tweens, scale factor for the scale tweens, initial alpha for fade tweens).
	public float tweenValue {
		get { return m_tweenValue; }
		set { m_tweenValue = value; m_isDirty = true; }
	}

	[SerializeField] protected Ease m_tweenEase = Ease.OutBack;
	public Ease tweenEase { 
		get { return m_tweenEase; }
		set { m_tweenEase = value; m_isDirty = true; }
	}

	[SerializeField] protected float m_tweenDelay = 0f;
	public float tweenDelay { 
		get { return m_tweenDelay; }
		set { m_tweenDelay = value; m_isDirty = true; }
	}

	[SerializeField] protected bool m_ignoreTimeScale = true;			// [AOC] Generally we don't want UI animations to be affected by global timeScale
	public bool ignoreTimeScale { 
		get { return m_ignoreTimeScale; }
		set { m_ignoreTimeScale = value; m_isDirty = true; }
	}

	// Custom tweens
	[SerializeField] protected DOTweenAnimation[] m_showTweens = new DOTweenAnimation[0];
	[SerializeField] protected DOTweenAnimation[] m_hideTweens = new DOTweenAnimation[0];

	// Animator param
	[Comment("Must have triggers \"show\", \"hide\", \"instantShow\" and \"instantHide\" and events \"OnShowAnimationCompleted\" and \"OnHideAnimationCompleted\"")]
	[SerializeField] protected Animator m_animator = null;

	// Other setup
	[Tooltip("Children ShowHideAnimators belonging to active GameObjects will be triggered with this one when showing/hiding")]
	[SerializeField] protected bool m_triggerChildren = true;
	public bool triggerChildren {
		get { return m_triggerChildren; }
		set { m_triggerChildren = value; }
	}

	[SerializeField] protected bool m_canBeTriggeredByParents = true;
	public bool canBeTriggeredByParents {
		get { return m_canBeTriggeredByParents; }
		set { m_canBeTriggeredByParents = value; }
	}

	// Events
	public ShowHideAnimatorEvent OnShowPreAnimation = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnShowPreAnimationAfterDelay = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnShowPostAnimation = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnHidePreAnimation = new ShowHideAnimatorEvent();
	public ShowHideAnimatorEvent OnHidePostAnimation = new ShowHideAnimatorEvent();

	// Code-only events
	public ShowHideAnimatorEvent OnShowCheck = new ShowHideAnimatorEvent();

	// Internal references
	protected CanvasGroup m_canvasGroup = null; // Not required, if the object has no animator nor a canvas group, it will be automatically added
	private ShowHideAnimatorEventsListener m_animatorEventsListener = null;	// Only for ANIMATOR type

	// Internal
	protected Sequence m_sequence = null;	// We will reuse the same tween and play it forward/backwards accordingly
	protected bool m_isDirty = true;
	protected bool m_disableAfterHide = true;
	protected bool m_checkPassed = true;

	// To control delay on Animator-driven animations
	private float m_delayTimer = 0f;
	private bool m_delaying = false;

    // [JOM] Since OnComplete is failing we manually check if the hide animation is finished (ñapa)
    private bool m_isHiding = false;

	// Since visibility is linked to object's being active, we cannot trust in initializing it properly on the Awake call (since Awake is not called for disabled objects)
	// Forced to do this workaround
	protected State m_state = State.INIT;
	public bool visible {
		get { 
			if(m_state == State.INIT) {
				if(gameObject.activeSelf) {
					m_state = State.VISIBLE;
				} else {
					// Use ForceHide to move to the end of the sequence!
					if(Application.isPlaying) {
						ForceHide(false, true);
					} else {
						m_state = State.HIDDEN;
					}

					// If hidden, disable interactability
					if(m_canvasGroup != null) {
						m_canvasGroup.interactable = false;
						m_canvasGroup.blocksRaycasts = false;
					}
				}
			}
			return m_state == State.VISIBLE; 
		}
	}
	public State state { get { return m_state; }}

	// Public properties
	// Sequence delta, only for sequence animations
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
	/// Called every frame.
	/// </summary>
	protected virtual void Update() {
		// Update delay timer!
		if(m_delaying) {
			m_delayTimer -= (m_ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);	// Ignoring time scale?
			if(m_delayTimer <= 0f) {
				// Delay timer should only be active if we're showing with animation
				LaunchShowAnimation(true);
				m_delaying = false;
			}
		}

        // We dont trust onComplete callback, so lets do a manual check
        if (m_isHiding)
        {
            if (delta == 0)
            {
                // The animation is finished
                OnSequenceCompleted();
            }
        }

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

		if(m_animatorEventsListener != null) {
			m_animatorEventsListener.OnShowCompletedEvent.RemoveListener(DoShowPostProcessing);
			m_animatorEventsListener.OnHideCompletedEvent.RemoveListener(DoHidePostProcessing);
			m_animatorEventsListener = null;
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
		// Use internal call with the right parameters
		InternalShow(_animate, true, false);
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
	/// Relaunches the show animation from the start, regardless of current state.
	/// </summary>
	public void RestartShow() {
		// Use internal call with the right parameters
		InternalShow(true, true, true);
	}

	/// <summary>
	/// Hide the object. Will be ignored if object is already hidden/hiding.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public virtual void Hide(bool _animate, bool _disableAfterAnimation = true) {
		// Use internal call with the right parameters
		InternalHide(_animate, _disableAfterAnimation, true, false);
	}

	/// <summary>
	/// Hide the object. Will be ignored if object is already hidden/hiding.
	/// Single param version to be able to connect it via inspector.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	public virtual void Hide(bool _animate = true) {
		Hide(_animate, true);
	}

	/// <summary>
	/// Same as hide but overriding current state.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public void ForceHide(bool _animate, bool _disableAfterAnimation = true) {
		// Force state to make sure Hide() call is not skipped
		m_state = State.VISIBLE;
		Hide(_animate, _disableAfterAnimation);
	}

	/// <summary>
	/// Same as hide but overriding current state.
	/// Single param version to be able to connect it via inspector.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	public void ForceHide(bool _animate = true) {
		ForceHide(_animate, true);
	}

	/// <summary>
	/// Relaunches the hide animation from the start, regardless of current state.
	/// </summary>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public void RestartHide(bool _disableAfterAnimation = true) {
		// Use internal call with the right parameters
		InternalHide(true, _disableAfterAnimation, true, true);
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

	/// <summary>
	/// Relaunches the animation of the target state from the start, regardless of current state.
	/// </summary>
	/// <param name="_visible">Whether to show or hide the object.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	public void RestartSet(bool _visible, bool _disableAfterAnimation = true) {
		if(_visible) {
			RestartShow();
		} else {
			RestartHide(_disableAfterAnimation);
		}
	}

	/// <summary>
	/// Mark check operation as failed.
	/// </summary>
	public void SetCheckFailed() {
		m_checkPassed = false;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create the tween sequence according to current setup.
	/// If a sequence already exists, it will be killed.
	/// </summary>
	public void RecreateTween() {
		// If the sequence is already created, kill it
		if(m_sequence != null) {
			m_sequence.Complete();	// Make sure sequence is at its end-state to restore object's default values so the new sequence can take them
			m_sequence.Kill();
			m_sequence = null;
		}

		// Clear dirty flag
		m_isDirty = false;

		// If using no animation at all, we're done
		if(m_tweenType == TweenType.NONE) return;

		// If using animator, we won't use tweens
		if(m_tweenType == TweenType.ANIMATOR) return;

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

			// If hidden, disable interactability
			if(m_state == State.HIDDEN) {
				m_canvasGroup.interactable = false;
				m_canvasGroup.blocksRaycasts = false;
			}
		}

		// Create new sequence
		m_sequence = DOTween.Sequence()
			.SetAutoKill(false)
			.SetUpdate(UpdateType.Normal, m_ignoreTimeScale)
            .OnStepComplete(() => { OnSequenceCompleted(); });

        // Shared parameters
        TweenParams tweenParams = new TweenParams()
			.SetEase(m_tweenEase);

		// [AOC] Elastic and back easing functions don't work well with a Fade animation, so tweak the fade duration for those easing functions
		float fadeDuration = m_tweenDuration;
		TweenParams fadeParams = new TweenParams()
			.SetEase(m_tweenEase);

		//float fadeDelay = 0f;
		switch(m_tweenEase) {
			case Ease.InElastic:
			case Ease.OutElastic:
			case Ease.InOutElastic:
				fadeDuration = m_tweenDuration * 0.33f;
				fadeParams.SetEase(Ease.Linear);
			break;

			case Ease.InBack:
			case Ease.OutBack:
			case Ease.InOutBack:
				fadeDuration = m_tweenDuration * 0.5f;
				fadeParams.SetEase(Ease.Linear);
			break;
		}

		// Initialize based on current parameters
		switch(m_tweenType) {
			case TweenType.IDLE: {
				m_sequence.PrependInterval(m_tweenDuration);	// Do nothing else than adding an idle interval
			} break;

			case TweenType.FADE: {
				m_tweenValue = Mathf.Clamp01(m_tweenValue);
				m_sequence.Join(m_canvasGroup.DOFade(m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.UP: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOBlendableLocalMoveBy(Vector3.down * m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.DOWN: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOBlendableLocalMoveBy(Vector3.up * m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.LEFT: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOBlendableLocalMoveBy(Vector3.right * m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.RIGHT: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOBlendableLocalMoveBy(Vector3.left * m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.SCALE: {
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOScale(m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.DELTA_SIZE: {
				RectTransform rt = transform as RectTransform;
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(rt.DOSizeDelta(rt.sizeDelta + Vector2.one * m_tweenValue, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.SCALE_X: {
				Vector3 scale = transform.localScale;
				scale.x = m_tweenValue;
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOScale(scale, m_tweenDuration).SetAs(tweenParams).From());
			} break;

			case TweenType.SCALE_Y: {
				Vector3 scale = transform.localScale;
				scale.y = m_tweenValue;
				m_sequence.Join(m_canvasGroup.DOFade(0f, fadeDuration).SetAs(fadeParams).From());
				m_sequence.Join(transform.DOScale(scale, m_tweenDuration).SetAs(tweenParams).From());
			} break;
		}

		m_sequence.PrependCallback(() => {
			// Only when going forward!!
			if(!m_sequence.isBackwards) {
				OnShowPreAnimationAfterDelay.Invoke(this); 
			}
		});

		// Insert delay at the beginning of the sequence
		m_sequence.PrependInterval(m_tweenDelay);
	}

	/// <summary>
	/// Show the object. Will be ignored if object is already visible/showing.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_propagateToChildren">Whether to propagate to children ShowHideAnimators. Setup flags will be checked anyways.</param>
	/// <param name="_restartAnim">Whether the animation should be restarted.</param>
	protected virtual void InternalShow(bool _animate, bool _propagateToChildren, bool _restartAnim) {
		// Debug
		ShowHideAnimator.DebugLog(this, Colors.green.Tag("INTERNAL_SHOW (" + _animate + ", " + _propagateToChildren + ", " + _restartAnim + ")"));

		// First things first: execute any external checks that might interrupt the action
		m_checkPassed = true;
		OnShowCheck.Invoke(this);
		if(!m_checkPassed) return;

		// If restarting the animation, instantly force hide state without animation
		if(_restartAnim) {
			ForceHide(false);
		}

		// If we're already in the target state, skip (unless dirty, in which case we want to place the animation sequence at the right place)
		if(visible && !m_isDirty) {
			ShowHideAnimator.DebugLog(this, Colors.orange.Tag("Early Exit: visible " + visible + ", dirty + " + m_isDirty));
			return;
		}

		// Update state
		m_state = State.VISIBLE;

		// In any case, make sure the object is active
		gameObject.SetActive(true);

		// ...and interactable
		if(m_canvasGroup != null) {
			m_canvasGroup.interactable = true;
			m_canvasGroup.blocksRaycasts = true;
		}

		// If dirty, re-create the tween (will be destroyed if not needed)
		if(m_isDirty) RecreateTween();

		// Broadcast pre-animation event
		OnShowPreAnimation.Invoke(this);

		// Trigger the animation!
		LaunchShowAnimation(_animate);

		// If configured, look for all children containing a ShowHideAnimator component and trigger it!
		if(_propagateToChildren && m_triggerChildren) {
			ShowHideAnimator[] animators = GetComponentsInChildren<ShowHideAnimator>(false);	// Exclude inactive ones - if they're inactive we probably don't want to show them!
			for(int i = 0; i < animators.Length; i++) {
				// Skip ourselves
				if(animators[i] == this) continue;

				// Skip if not allowed
				if(!animators[i].m_canBeTriggeredByParents) continue;

				// Show using internal method directly
				animators[i].InternalShow(_animate, false, _restartAnim);
			}
		}

		// If not animating, do immediate postprocessing
		if(!_animate) {
			DoShowPostProcessing();
		}
	}

	/// <summary>
	/// Hide the object. Will be ignored if object is already hidden/hiding.
	/// </summary>
	/// <param name="_animate">Whether to use animations or not.</param>
	/// <param name="_disableAfterAnimation">Whether to disable the object once the animation has finished or not. Only for non-custom tween animations.</param>
	/// <param name="_propagateToChildren">Whether to propagate to children ShowHideAnimators. Setup flags will be checked anyways.</param>
	/// <param name="_restartAnim">Whether the animation should be restarted.</param>
	protected virtual void InternalHide(bool _animate, bool _disableAfterAnimation, bool _propagateToChildren, bool _restartAnim) {
		// Debug
		ShowHideAnimator.DebugLog(this, Colors.red.Tag("INTERNAL_HIDE (" + _animate + ", " + _disableAfterAnimation + ", " + _propagateToChildren + ", " + _restartAnim + ")"));

		// If restarting the animation, instantly force hide state without animation
		if(_restartAnim) {
			ForceShow(false);
		}

		// If we're already in the target state, skip (unless dirty, in which case we want to place the animation sequence at the right place)
		if(!visible && !m_isDirty) {
			ShowHideAnimator.DebugLog(this, Colors.orange.Tag("Early Exit: visible " + visible + ", dirty + " + m_isDirty));
			return;
		}

		// Update state
		m_state = State.HIDDEN;
		m_disableAfterHide = _disableAfterAnimation;

		// If dirty, re-create the tween (will be destroyed if not needed)
		if(m_isDirty) {
			RecreateTween();

			// Since we're going backwards, initialize the new sequence at the end (in case there is one)
			if(m_sequence != null) m_sequence.Goto(1f);
		}

		// Broadcast pre-animation event
		OnHidePreAnimation.Invoke(this);

		// Trigger the animation!
		LaunchHideAnimation(_animate);

        // We dont trust onComplete event, so lets check manually
        m_isHiding = true;

        // If configured, look for all children containing a ShowHideAnimator component and trigger it!
        if (_propagateToChildren && m_triggerChildren) {
			ShowHideAnimator[] animators = GetComponentsInChildren<ShowHideAnimator>(false);	// Exclude inactive ones - if they're inactive we probably don't want to show them!
			for(int i = 0; i < animators.Length; i++) {
				// Skip ourselves
				if(animators[i] == this) continue;

				// Skip if not allowed
				if(!animators[i].m_canBeTriggeredByParents) continue;

				// Hide but don't disable them, otherwise they won't be shown again when triggering the show animation!
				// Using internal method directly
				animators[i].InternalHide(_animate, false, false, _restartAnim);
			}
		}

		// If not animating, do immediate post-processing
		if(!_animate) {
			DoHidePostProcessing();
		}
	}

	/// <summary>
	/// Launch the show animation. Can be override by heirs.
	/// </summary>
	/// <param name="_animate">Whether to actually launch the animation or just instantly show.</param>
	protected virtual void LaunchShowAnimation(bool _animate) {
		// Perform different actions depending on the selected animation type
		switch(m_tweenType) {
			// Unity animation
			case TweenType.ANIMATOR: {
				// If using an animator, let it do all the work
				// Animate?
				if(_animate) {
					// If we have a delay setup, wait before launching the animation!
					if(tweenDelay > 0f && !m_delaying) {
						m_delayTimer = tweenDelay;
						m_delaying = true;
						SetAnimTrigger("instantHide");	// Reset animation state machine
					} else {
						m_delayTimer = 0f;
						m_delaying = false;
						OnShowPreAnimationAfterDelay.Invoke(this);
						SetAnimTrigger("show");
					}
				} else {
					m_delayTimer = 0f;
					m_delaying = false;
					SetAnimTrigger("instantShow");
				}
			} break;

			// None
			case TweenType.NONE: {
				// Immediately do post-processing, simulating animation ending
				if(_animate) DoShowPostProcessing();
			} break;

			// Custom tweens
			case TweenType.CUSTOM: {
				// Stop all running hide animators
				for(int i = 0; i < m_hideTweens.Length; i++) {
					m_hideTweens[i].DOPause();
				}

				// Restart all the animators in the show array
				for(int i = 0; i < m_showTweens.Length; i++) {
					if(_animate) {
						m_showTweens[i].DORestart();
					} else {
						m_showTweens[i].DOGoto(1f);	// Instantly move to the end of the tween when not animating
					}
				}
			} break;

			// Sequence
			default: {
				// Using tweens, play the sequence in the proper direction
				if(m_sequence != null) {
					// Animate?
					if(_animate) {
						m_sequence.PlayForward();	// The cool thing is that if the hide animation is interrupted, the show animation will start from the interruption point
					} else {
						OnShowPreAnimationAfterDelay.Invoke(this); // Invoke event that would otherwise be ignored
						m_sequence.Goto(1f);		// Instantly move to the end of the sequence
					}
				}
			} break;
		}
	}

	/// <summary>
	/// Launch the hide animation. Can be override by heirs.
	/// </summary>
	/// <param name="_animate">Whether to actually launch the animation or just instantly hide.</param>
	protected virtual void LaunchHideAnimation(bool _animate) {
		// Perform different actions depending on the selected animation type
		switch(m_tweenType) {
			// Unity animation
			case TweenType.ANIMATOR: {
				// If using an animator, let it do all the work
				// No delay on hide animations
				m_delayTimer = 0f;
				m_delaying = false;

				// Animate?
				if(_animate) {
					SetAnimTrigger("hide");
				} else {
					SetAnimTrigger("instantHide");
				}
			} break;

			// None
			case TweenType.NONE: {
				// Immediately do postprocessing
				if(_animate) DoHidePostProcessing();
			} break;

			// Custom tweens
			case TweenType.CUSTOM: {
				// Stop all running hide animators
				for(int i = 0; i < m_showTweens.Length; i++) {
					m_showTweens[i].DOPause();
				}

				// If using custom tween animators, restart all the animators in the hide event
				for(int i = 0; i < m_hideTweens.Length; i++) {
					if(_animate) {
						m_hideTweens[i].DORestart();
					} else {
						m_hideTweens[i].DOGoto(0f);
					}
				}
			} break;

			// Sequence
			default: {
				// Using tweens, play the sequence in the proper direction
				if(m_sequence != null) {
					// Animate?
					if(_animate) {
						if ( delta <= 0 && m_sequence.IsPlaying() && !m_sequence.isBackwards){
							m_sequence.Goto(0);
							DoHidePostProcessing();
						}else{
							m_sequence.PlayBackwards();	// The cool thing is that if the hide animation is interrupted, the show animation will start from the interruption point
						}
					} else {
						m_sequence.Goto(0f);		// Instantly move to the start point
					}
				}
			} break;
		}
	}

	/// <summary>
	/// Perform all the required stuff after the show process (either with or 
	/// without animation) has finished.
	/// Can be override by heirs.
	/// </summary>
	protected virtual void DoShowPostProcessing() {
		// Broadcast event
		OnShowPostAnimation.Invoke(this);
	}

	/// <summary>
	/// Perform all the required stuff after the hide process (either with or 
	/// without animation) has finished.
	/// Can be override by heirs.
	/// </summary>
	protected virtual void DoHidePostProcessing() {
		// Broadcast event
		OnHidePostAnimation.Invoke(this);

		// Debug
		ShowHideAnimator.DebugLog(this, Colors.red.Tag("DISABLE AFTER HIDE: " + m_disableAfterHide));

		// Optionally disable object after the hide animation has finished
		if(m_disableAfterHide) {
			gameObject.SetActive(false);
		}

		// If hidden, object should never be interactable
		if(m_canvasGroup != null) {
			m_canvasGroup.interactable = false;
			m_canvasGroup.blocksRaycasts = false;
		}
	}

	/// <summary>
	/// Only when in ANIMATION mode:
	/// Reset all animator triggers and sets one.
	/// </summary>
	private void SetAnimTrigger(string _trigger) {
		// Animator must be valid!
		if(m_animator == null) return;

		// Make sure we're listening to target animator's events
		if(m_animatorEventsListener == null) {
			m_animatorEventsListener = m_animator.gameObject.ForceGetComponent<ShowHideAnimatorEventsListener>();
			m_animatorEventsListener.OnShowCompletedEvent.AddListener(DoShowPostProcessing);
			m_animatorEventsListener.OnHideCompletedEvent.AddListener(DoHidePostProcessing);
		}

		// Reset all known triggers
		m_animator.ResetTrigger( GameConstants.Animator.SHOW );
		m_animator.ResetTrigger( GameConstants.Animator.HIDE );
		m_animator.ResetTrigger( GameConstants.Animator.INSTANT_SHOW );
		m_animator.ResetTrigger( GameConstants.Animator.INSTANT_HIDE );

		// Set the target trigger
		m_animator.SetTrigger(_trigger);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Either a show or a hide animation has finished.
	/// Won't be called when animation is interrupted.
	/// </summary>
	protected virtual void OnSequenceCompleted() {

        m_isHiding = false;

        // Which animation has finished?
        DebugLog(this, visible ? Color.green.Tag("DO_SHOW_POSTPROCESSING") : Color.red.Tag("DO_HIDE_POSTPROCESSING"));
		if(visible) {
			DoShowPostProcessing();
		} else {
            DoHidePostProcessing();
            
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

		// No more active tweens! Do post processing
		DoShowPostProcessing();
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

		// No more active tweens! Do postprocessing
		DoHidePostProcessing();
	}

    //------------------------------------------------------------------//
    // DEBUG															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Check the debug conditions and log given text if matching.
    /// </summary>
    /// <param name="_target">Target object.</param>
    /// <param name="_text">Text.</param>
    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void DebugLog(Object _target, string _text) {		
		if(_target == null || _target.name != DEBUG_TARGET) return;
		ControlPanel.Log(_text);
	}
}

