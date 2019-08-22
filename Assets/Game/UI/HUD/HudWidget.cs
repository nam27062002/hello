using UnityEngine;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to store common stuff among different hud widgets showing a piece of data.
/// </summary>
public abstract class HudWidget : MonoBehaviour
{
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	protected const float MINIMUM_ANIM_INTERVAL = 0f;	// Seconds, minimum time without updating before triggering the animation again

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
    protected virtual void Awake()
    {
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

    protected virtual void Update()
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
