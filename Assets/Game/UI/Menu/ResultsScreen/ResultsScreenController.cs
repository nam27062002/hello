using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ResultsScreenController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		INIT,
		INTRO,
		RESULTS,
		MISSIONS,
		PROGRESSION_1,
		PROGRESSION_2,
		FINISHED
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private TextMeshProUGUI m_timeLabel = null;
	[SerializeField] private NumberTextAnimator m_scoreAnimator = null;
	[SerializeField] private NumberTextAnimator m_coinsAnimator = null;
	[SerializeField] private NumberTextAnimator m_bonusCoinsAnimator = null;

	[Separator]
	[SerializeField] private Localizer m_highScoreLabel = null;
	[SerializeField] private GameObject m_newHighScoreDeco = null;

	[Separator]
	[SerializeField] private ResultsScreenUnlockBar m_unlockBar = null;
	[SerializeField] private ResultsScreenCarousel m_carousel = null;

	// References
	private ResultsSceneSetup m_scene = null;

	// Internal
	private State m_state = State.INIT;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Make use of properties to easily add test code without getting it all dirty
	private long score {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.scoreValue;
			} else {
				return RewardManager.score;
			}
		}
	}

	private long coins {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.coinsValue;
			} else {
				return RewardManager.coins;
			}
		}
	}

	private int survivalBonus {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return (int)CPResultsScreenTest.survivalBonus;
			} else {
				return RewardManager.instance.CalculateSurvivalBonus();
			}
		}
	}

	private long highScore {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.highScoreValue;
			} else {
				return UsersManager.currentUser.highScore;
			}
		}
	}

	private bool isHighScore {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.newHighScore;
			} else {
				return RewardManager.isHighScore;
			}
		}
	}

	private float time {
		get {
			if(CPResultsScreenTest.testEnabled) {
				return CPResultsScreenTest.timeValue;
			} else {
				return InstanceManager.GetSceneController<GameSceneController>().elapsedSeconds;
			}
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Check required fields
		Debug.Assert(m_timeLabel != null, "Required field not initialized!");
		Debug.Assert(m_scoreAnimator != null, "Required field not initialized!");
		Debug.Assert(m_coinsAnimator != null, "Required field not initialized!");
		Debug.Assert(m_bonusCoinsAnimator != null, "Required field not initialized!");

		Debug.Assert(m_highScoreLabel != null, "Required field not initialized!");
		Debug.Assert(m_newHighScoreDeco != null, "Required field not initialized!");

		Debug.Assert(m_carousel != null, "Required field not initialized!");
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	/// <summary>
	/// Manual initialization.
	/// </summary>
	public void LaunchAnim() {
		// set values from score manager
		// Launch number animators
		int coinsBonus = survivalBonus;

		// Number animations
		m_scoreAnimator.SetValue(0, score);
		m_coinsAnimator.SetValue(0, coins + coinsBonus);
		m_bonusCoinsAnimator.SetValue(0, coinsBonus);

		// High Score Label
		m_highScoreLabel.Localize(m_highScoreLabel.tid, StringUtils.FormatNumber(highScore));

		// New High Score animation
		if(isHighScore) {
			m_newHighScoreDeco.SetActive(true);
			m_newHighScoreDeco.transform.localScale = Vector3.zero;
			m_newHighScoreDeco.transform.DOScale(1f, 0.25f)
				.SetDelay(m_scoreAnimator.duration)	// Sync with score number animation
				.SetEase(Ease.OutBack)
				.SetAutoKill(true)
				.OnComplete(() => {
					// TODO!! Play some SFX
				});
		} else {
			m_newHighScoreDeco.SetActive(false);
		}

		// Set time - format to MM:SS
		m_timeLabel.text = TimeUtils.FormatTime(time, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);

		// Initialize unlock bar and start animating!
		m_unlockBar.Initialize(m_carousel.progressionPill);

		// Start carousel as well!
		m_carousel.gameObject.SetActive(true);
		m_carousel.StartCarousel();
	}

	/// <summary>
	/// Try to go back to the menu. If a popup is pending (chest reward, egg reward), it will be displayed instead.
	/// </summary>
	/// <returns>Whether we're going back to the menu (<c>true</c>) or we've been interrupted by some pending popup (<c>false</c>).</returns>
	private bool TryGoToMenu() {
		// Check for any impediment to go to the menu (i.e. pending popups)
		if(false) {
			// Nothing for now
		}

		// Nothing else to show, go back to the menu!
		else {
			// Update global stats
			UsersManager.currentUser.gamesPlayed = UsersManager.currentUser.gamesPlayed + 1;

			// Apply rewards to user profile
			RewardManager.ApplyRewardsToProfile();

			// Process Missions: give rewards and generate new missions replacing those completed
			MissionManager.ProcessMissions();

			// Process collectible chests: give rewards and update collected chests count
			ChestManager.ProcessChests();

			// Process collectible egg
			EggManager.ProcessCollectibleEgg();

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
	/// Go back to the main menu, finalizing all the required stuff in the game scene.
	/// </summary>
	public void OnGoToMenu() {
		// Use internal method
		TryGoToMenu();
	}
}
