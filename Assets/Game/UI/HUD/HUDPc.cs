// HUDPc.cs
// Hungry Dragon
// 

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to update a textfield with the current amount of PC of the player.
/// </summary>
public class HUDPc : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Text m_text = null;
	[SerializeField] private Image m_icon = null;
	[SerializeField] private Animator m_anim = null;

	private float m_timer;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_text != null, "Required component!");
		m_text.text = "";
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateText();
	}
	
	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);

		m_text.gameObject.SetActive(false);
		m_icon.gameObject.SetActive(false);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}

	private void Update() {
		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_text.gameObject.SetActive(false);
				m_icon.gameObject.SetActive(false);
				m_timer = 0;
			}
		}
	}
	
	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed coins.
	/// </summary>
	private void UpdateText() {
		// Do it!
		m_text.text = StringUtils.FormatNumber(RewardManager.pc);
	}
	
	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied, show feedback for it.
	/// </summary>
	/// <param name="_reward">The reward that has been applied.</param>
	/// <param name="_entity">The entity that triggered the reward. Can be null.</param>
	private void OnRewardApplied(Reward _reward, Transform _entity) {
		// We only care about coin rewards
		if(_reward.pc > 0) {
			UpdateText();
			m_text.gameObject.SetActive(true);
			m_icon.gameObject.SetActive(true);
			m_timer = 3f;
			if(m_anim != null) m_anim.SetTrigger("start");
		}
	}
}

