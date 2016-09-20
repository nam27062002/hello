using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultsScreenController : MonoBehaviour {

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Text m_timeLabel = null;
	[SerializeField] private NumberTextAnimator m_scoreAnimator = null;
	[SerializeField] private NumberTextAnimator m_coinsAnimator = null;
	[SerializeField] private NumberTextAnimator m_bonusCoinsAnimator = null;

	[Separator]
	[SerializeField] private Localizer m_highScoreLabel = null;
	[SerializeField] private GameObject m_newHighScoreDeco = null;

	[Separator]
	[SerializeField] private Slider m_levelBar = null;
	[SerializeField] private Localizer m_levelText = null;
	[SerializeField] private Localizer m_dragonNameText = null;

	[Separator]
	[SerializeField] private ResultsScreenCarousel m_carousel = null;

	// Internal
	private int m_levelAnimCount = 0;
	private Tween m_xpBarTween = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	void Awake() {
		// Check required fields
		Debug.Assert(m_timeLabel != null, "Required field not initialized!");
		Debug.Assert(m_scoreAnimator != null, "Required field not initialized!");
		Debug.Assert(m_coinsAnimator != null, "Required field not initialized!");
		Debug.Assert(m_bonusCoinsAnimator != null, "Required field not initialized!");

		Debug.Assert(m_highScoreLabel != null, "Required field not initialized!");
		Debug.Assert(m_newHighScoreDeco != null, "Required field not initialized!");

		Debug.Assert(m_levelBar != null, "Required field not initialized!");
		Debug.Assert(m_levelText != null, "Required field not initialized!");
		Debug.Assert(m_dragonNameText != null, "Required field not initialized!");
		Debug.Assert(m_carousel != null, "Required field not initialized!");
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// If we got a tween running, kill it immediately
		if(m_xpBarTween != null) {
			m_xpBarTween.Kill(false);
			m_xpBarTween = null;
		}
	}

	/// <summary>
	/// Initislization.
	/// </summary>
	public void Initialize() {
		// set values from score manager
		// Launch number animators
		int survivalBonus = RewardManager.instance.CalculateSurvivalBonus();

		m_scoreAnimator.SetValue(0, (int)RewardManager.score);
		m_coinsAnimator.SetValue(0, (int)(RewardManager.coins + survivalBonus));
		m_bonusCoinsAnimator.SetValue(0, RewardManager.instance.CalculateSurvivalBonus()); //TODO: get bouns coins from Reward Manager

		m_highScoreLabel.Localize(m_highScoreLabel.tid, StringUtils.FormatNumber(UsersManager.currentUser.highScore));

		m_newHighScoreDeco.SetActive(RewardManager.isHighScore);

		// Set time - format to MM:SS
		GameSceneController game = InstanceManager.GetSceneController<GameSceneController>();
		m_timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);

		// Set dragon name and xp
		// Get updated dragon's data from the dragon manager
		DragonData data = DragonManager.currentDragon;
		m_dragonNameText.Localize(data.def.Get("tidName"));

		// Bar value - animate!
		// [AOC] As usual, animating the XP bar is not obvious (dragon may have leveled up several times during a single game)
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = 1;
		m_levelBar.value = RewardManager.dragonInitialLevelProgress;
		m_levelAnimCount = RewardManager.dragonInitialLevel;
		m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
		LaunchXPBarAnim();

		// Hide carousel
		m_carousel.gameObject.SetActive(false);
	}

	/// <summary>
	/// Launches the XP anim.
	/// </summary>
	private void LaunchXPBarAnim() {
		// Aux vars
		DragonData data = DragonManager.currentDragon;
		bool isTargetLevel = (m_levelAnimCount == DragonManager.currentDragon.progression.level);
		float targetDelta = isTargetLevel ? data.progression.progressCurrentLevel : 1f;	// Full bar if not target level

		// Create animation
		m_xpBarTween = DOTween.To(
			// Getter function
			() => { 
				return m_levelBar.value; 
			}, 

			// Setter function
			(_newValue) => {
				m_levelBar.value = _newValue;
			},

			// Value and speed
			targetDelta, 0.75f
		)

		// Other setup parameters
		.SetSpeedBased(true)
		.SetEase(Ease.InOutCubic)

		// What to do once the anim has finished?
		.OnComplete(
			() => {
				// Was it the target level? We're done!
				if(isTargetLevel) {
					// Now we can start the carousel!
					m_carousel.gameObject.SetActive(true);
					m_carousel.StartCarousel();
					return;
				}

				// Not the target level, increase level counter and restart animation!
				m_levelAnimCount++;

				// Set text and animate
				m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
				m_levelText.transform.DOScale(1.5f, 0.15f).SetLoops(2, LoopType.Yoyo);

				// Put bar to the start
				m_levelBar.value = 0f;

				// Lose tween reference (will be self-destroyed immediately) and create a new one
				m_xpBarTween = null;
				LaunchXPBarAnim();
			}
		);
	}



	/// <summary>
	/// Try to go back to the menu. If a popup is pending (chest reward, egg reward), it will be displayed instead.
	/// </summary>
	/// <returns>Whether we're going back to the menu (<c>true</c>) or we've been interrupted by some pending popup (<c>false</c>).</returns>
	private bool TryGoToMenu() {
		// If we found a chest, open the chest reward popup
		if(ChestManager.selectedChest != null && ChestManager.selectedChest.collected) {
			PopupManager.OpenPopupAsync(PopupChestReward.PATH);
		}

		// Did we found an egg during the game?
		else if(EggManager.collectibleEgg != null && EggManager.collectibleEgg.collected) {
			PopupManager.OpenPopupAsync(PopupEggReward.PATH);	// Yes! Show popup
		}

		// Nothing else to show, go back to the menu!
		else {
			// Update global stats
			UsersManager.currentUser.gamesPlayed = UsersManager.currentUser.gamesPlayed + 1;

			// Apply rewards to user profile
			RewardManager.ApplyRewardsToProfile();

			// Process Missions: give rewards and generate new missions replacing those completed
			MissionManager.ProcessMissions();

			// Clear collectibles
			ChestManager.ClearSelectedChest();
			EggManager.ClearCollectibleEgg();

			// Save persistence
			PersistenceManager.Save(true);

			// Go back to main menu
			FlowManager.GoToMenu();

			return true;
		}

		return false;
	}

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that has been closed</param>
	public void OnPopupClosed(PopupController _popup) {
		// Was it the chest reward or the egg reward popups?
		if(_popup.GetComponent<PopupChestReward>() != null
		|| _popup.GetComponent<PopupEggReward>() != null) {
			// Go back to menu
			TryGoToMenu();
		}
	}

	/// <summary>
	/// Go back to the main menu, finalizing all the required stuff in the game scene.
	/// </summary>
	public void OnGoToMenu() {
		// Use internal method
		TryGoToMenu();
	}
}
