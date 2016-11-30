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
		WAIT_INTRO,
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

	// Animators
	[Separator]
	[SerializeField] private ShowHideAnimator m_popupAnimator = null;
	[SerializeField] private ShowHideAnimator m_bottomBarAnimator = null;
	[SerializeField] private ShowHideAnimator m_unlockBarAnimator = null;

	// To easily setup animation durations
	[Separator]
	[SerializeField] private float m_introDuration = 1f;

	// References
	private ResultsSceneSetup m_scene = null;

	// Internal
	private State m_state = State.INIT;
	private float m_timer = 0f;

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
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update timer
		if(m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}

		// Do different stuff depending on current state
		switch(m_state) {
			case State.INIT: {
				// Do nothing
			} break;
			case State.WAIT_INTRO: {
				if (m_timer <= 0){
					ChangeState( State.INTRO );
				}
			}break;
			case State.INTRO: {
				// If timer has finished, go to next state!
				if(m_timer <= 0f) {
					ChangeState(State.RESULTS);
				}
			} break;

			case State.RESULTS: {
				// If timer has finished, go to next state!
				if(m_timer <= 0f) {
					ChangeState(State.PROGRESSION_1);
				}
			} break;

			case State.MISSIONS: {
				// Wait for carousel to finish
				if(m_carousel.isIdleOrFinished) {
					ChangeState(State.FINISHED);
				}
			} break;

			case State.PROGRESSION_1: {
				// Wait for carousel to finish
				if(m_carousel.isIdleOrFinished) {
					ChangeState(State.PROGRESSION_2);
				}
			} break;

			case State.PROGRESSION_2: {
				// Noting to do for now, go to missions
				ChangeState(State.MISSIONS);
			} break;

			case State.FINISHED: {
				// Do nothing
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the screen and leave it ready for the LaunchAnim call.
	/// </summary>
	/// <param name="_scene">The 3D scene to be used.</param>
	public void Init(ResultsSceneSetup _scene) {
		// Store the 3D scene
		m_scene = _scene;

		// Go to INIT state
		ChangeState(State.INIT);
	}

	/// <summary>
	/// Manual initialization.
	/// </summary>
	public void LaunchAnim() {
		// Just change state
		ChangeState(State.WAIT_INTRO);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Change the internal logic state.
	/// </summary>
	/// <param name="_newState">Target state.</param>
	private void ChangeState(State _newState) {
		// Different stuff depending on target state
		switch(_newState) {
			case State.INIT: {
				// Tell the 3D scene to reset
				if(m_scene != null) m_scene.Init();

				// Initialize number animators
				m_scoreAnimator.SetValue(0, 0);
				m_coinsAnimator.SetValue(0, 0);
				m_bonusCoinsAnimator.SetValue(0, 0);
				m_newHighScoreDeco.SetActive(false);

				// Initialize High Score Label
				m_highScoreLabel.Localize(m_highScoreLabel.tid, StringUtils.FormatNumber(highScore));

				// Initialize Game time - format to MM:SS
				m_timeLabel.text = TimeUtils.FormatTime(time, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);

				// Hide popup
				m_popupAnimator.ForceHide(false);

				// Initialize unlock bar
				m_unlockBar.Init(m_carousel.progressionPill);
				m_unlockBarAnimator.ForceHide(false);

				// Initialize carousel
				m_carousel.gameObject.SetActive(true);
				m_carousel.Init();
				m_bottomBarAnimator.ForceHide(false);
			} break;
			case State.WAIT_INTRO:
			{
				m_timer = 0.5f;
			}break;
			case State.INTRO: {
				// Launch dragon animation
				m_scene.LaunchDragonAnim();

				// Start timer to next state
				m_timer = m_introDuration;
			} break;

			case State.RESULTS: {
				// Show popup
				m_popupAnimator.Show();

				// Show bottom bar
				m_bottomBarAnimator.Show();

				// Launch number animators
				int coinsBonus = survivalBonus;
				m_scoreAnimator.SetValue(0, score);
				m_coinsAnimator.SetValue(0, coins + coinsBonus);
				m_bonusCoinsAnimator.SetValue(0, coinsBonus);

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
				}

				// Launch 3D rewards animations
				float duration = m_scene.LaunchRewardsAnim();

				// Start timer to next state
				m_timer = duration;
			} break;

			case State.MISSIONS: {
				// Show missions carousel
				m_carousel.DoMissions();
			} break;

			case State.PROGRESSION_1: {
				// Show progression carousel (not yet split)
				m_carousel.DoProgression();

				// Show unlock bar as well
				m_unlockBarAnimator.Show();
			} break;

			case State.PROGRESSION_2: {
				// Nothing to do for now
			} break;

			case State.FINISHED: {
				// Tell carousel to finish as well
				m_carousel.Finish();
			} break;
		}

		// Store new state
		m_state = _newState;
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
