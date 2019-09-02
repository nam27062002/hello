using UnityEngine;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to store common stuff among different hud widgets showing a piece of data.
/// </summary>
public abstract class IHUDCounter : IHUDWidget
{
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	protected const float MINIMUM_ANIM_INTERVAL = 0f;   // Seconds, minimum time without updating before triggering the animation again

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField]protected TextMeshProUGUI m_valueTxt;
    protected Animator m_anim;
	private float m_lastAnimTimestamp = 0f;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//	
    /// <summary>
    /// Initialization.
    /// </summary>
    protected override void Awake()
    {
		// Call parent
		base.Awake();

        if (m_valueTxt == null)
        {
            // Get external references
            m_valueTxt = GetComponentInChildren<TextMeshProUGUI>();
        }

        m_valueTxt.text = "0";

        m_anim = GetComponent<Animator>();
    }

    /// <summary>
	/// First update call.
	/// </summary>
    protected virtual void Start()
    {
        Value = 0;
        NeedsToPlayAnim = false;
        PrintValue();
    }

	/// <summary>
	/// How often the widget should be updated for the given graphic quality level.
	/// </summary>
	/// <param name="_qualityLevel">Graphics quality level to be considered. A value in [0, MAX_PROFILE_LEVEL] if the user has ever chosen a profile, otherwise <c>-1</c>.</param>
	/// <returns>Seconds, how often the widget should be refreshed.</returns>
	public override float GetUpdateIntervalByQualityLevel(int _qualityLevel) {
		if(_qualityLevel < 1) { // Very Low
			return 0.35f;
		} else if(_qualityLevel < 4) {
			return 0.25f;
		} else {                // Very High
			return 0.1f;
		}
	}

	public override void PeriodicUpdate()
    {
        if (NeedsToPrintValue)
        {
            PrintValue();
        }

        if (NeedsToPlayAnim)
        {
            PlayAnim();            
        }
    }

    protected void UpdateValue(long value, bool playAnim, bool immediate=false)
    {
        if (Value != value)
        {
            Value = value;
            NeedsToPrintValue = true;            

            if (immediate)
            {
                PrintValue();
            }
        }

        NeedsToPlayAnim = playAnim;

        if (NeedsToPlayAnim && immediate)
        {
            PlayAnim();
        }
    }

    protected bool NeedsToPlayAnim { get; set; }
    private bool NeedsToPrintValue { get; set; }
    protected long Value { get; set; }
    
    protected abstract string GetValueAsString();

    protected void PrintValue()
    {
        if (m_valueTxt != null)
            PrintValueExtended();        

        NeedsToPrintValue = false;
    }

    protected virtual void PrintValueExtended()
    {
        m_valueTxt.text = GetValueAsString();
    }

    private void PlayAnim()
    {
		NeedsToPlayAnim = false;

		if(m_anim != null) {
			// Has enough time passed?
			if(Time.realtimeSinceStartup - m_lastAnimTimestamp >= GetMinimumAnimInterval()) {
				PlayAnimExtended();
			}
		}

		// Reset timer
		m_lastAnimTimestamp = Time.realtimeSinceStartup;
	}

    protected virtual void PlayAnimExtended()
    {
		m_anim.SetTrigger( GameConstants.Animator.START );
    }

	protected virtual float GetMinimumAnimInterval() {
		return MINIMUM_ANIM_INTERVAL;
	}
}
