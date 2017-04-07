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
		PROGRESSION,
		COLLECTIBLES,
		CAROUSEL_MISSIONS,
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
	[SerializeField] private ResultsScreenXPBar m_unlockBar = null;
	[SerializeField] private ResultsScreenCarousel m_carousel = null;

	// Animators
	[Separator]
	[SerializeField] private ShowHideAnimator m_popupAnimator = null;
	[SerializeField] private ShowHideAnimator m_bottomBarAnimator = null;
	[SerializeField] private ShowHideAnimator m_unlockBarAnimator = null;

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
				return InstanceManager.gameSceneControllerBase.elapsedSeconds;
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
					ChangeState(State.PROGRESSION);
				}
			} break;
			
			case State.PROGRESSION: {
				// If timer has finished, go to next state!
				if(m_timer <= 0f) {
					ChangeState(State.COLLECTIBLES);
				}
			} break;

			case State.COLLECTIBLES: {
				// If timer has finished, go to next state!
				if(m_timer <= 0f) {
					ChangeState(State.CAROUSEL_MISSIONS);
				}
			} break;

			case State.CAROUSEL_MISSIONS: {
				// Wait for carousel to finish
				if(m_carousel.isIdleOrFinished) {
					ChangeState(State.FINISHED);
				}
			} break;

			case State.FINISHED: {
				// Do nothing
				if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST )
				{
					// Bo back to the menu
					TryGoToMenu();
				}
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
				m_unlockBar.Init();
				m_unlockBarAnimator.ForceHide(false);

				// Initialize carousel
				m_carousel.gameObject.SetActive(true);
				m_carousel.Init();
				m_bottomBarAnimator.ForceHide(false);
			} break;

			case State.WAIT_INTRO: {
				m_timer = 0.5f;
			} break;

			case State.INTRO: {
				// Launch dragon animation
				m_scene.LaunchDragonAnim();

				// Start timer to next state
				m_timer = UIConstants.resultsIntroDuration;
			} break;

			case State.RESULTS: {
				// Show popup
				m_popupAnimator.Show();

				// Show bottom bar
				m_bottomBarAnimator.Show();

				// Compute durations
				float totalDuration = UIConstants.resultsPanelDuration - m_popupAnimator.tweenDuration;
				float finalPause = 0.25f;
				float textAnimationDuration = (totalDuration - finalPause)/3f;	// 3 texts to be filled

				// Launch number animators
				// Reset
				m_scoreAnimator.duration = textAnimationDuration;
				m_coinsAnimator.duration = textAnimationDuration;
				m_bonusCoinsAnimator.duration = textAnimationDuration;
				m_scoreAnimator.SetValue(0, false);
				m_coinsAnimator.SetValue(0, false);
				m_bonusCoinsAnimator.SetValue(0, false);

				// Animated sequence!
				int coinsBonus = survivalBonus;
				Sequence seq = DOTween.Sequence()
					// Score
					.AppendInterval(m_popupAnimator.tweenDuration)	// Initial delay, give time for the show animation
					.AppendCallback(() => { m_scoreAnimator.SetValue(0, score); })

					// High score
					.AppendInterval(textAnimationDuration * 0.9f)	// Overlap a little bit
					.AppendCallback(() => {
						// New High Score animation
						if(isHighScore) {
							m_newHighScoreDeco.SetActive(true);
							m_newHighScoreDeco.transform.localScale = Vector3.zero;
							m_newHighScoreDeco.transform.DOScale(1f, 0.25f)
								.SetEase(Ease.OutBack)
								.SetAutoKill(true)
								.OnComplete(() => {
									// TODO!! Play some SFX
								});
						}
					})

					// Bonus coins
					.AppendInterval(isHighScore ? 0.25f: 0f)
					.AppendCallback(() => { m_bonusCoinsAnimator.SetValue(0, coinsBonus); })

					// Coins total
					.AppendInterval(textAnimationDuration * 0.9f)	// Overlap a little bit
					.AppendCallback(() => { m_coinsAnimator.SetValue(0, coins + coinsBonus); })

					// Final pause
					.AppendInterval(textAnimationDuration + finalPause);

				// Start timer to next state
				m_timer = seq.Duration();
			} break;
			
			case State.PROGRESSION: {
				// Show and animate unlock bar as well
				// Start timer to next state
				m_unlockBarAnimator.Show();
				m_timer = m_unlockBar.LaunchAnimation();
			} break;

			case State.COLLECTIBLES: {
				// Launch 3D rewards animations
				// Start timer to next state
				m_timer = m_scene.LaunchRewardsAnim(m_carousel.chestsPill);

				// Show chests carousel as well
				m_carousel.DoChests();
			} break;

			case State.CAROUSEL_MISSIONS: {
				// Show missions carousel
				m_carousel.DoMissions();
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
			RewardManager.ApplyEndOfGameRewards();

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
