#define AWARD_AS_SOON_AS_COLLECTED

using FGOL;
using System.Collections.Generic;
using UnityEngine;

public class HungryLettersManager : MonoBehaviour
{
	//------------------------------------------------------------
	// Enumerations:
	//------------------------------------------------------------

	public enum Difficulties
	{
		Easy,
		Hard,
		Normal,
	}

	public enum CollectibleLetters
	{
		H = 0, U = 1, N = 2, G = 3, R = 4, Y = 5, EnumEnd = 6,
	}
    
    public static char ToChar( CollectibleLetters letter )
    {
        char[] c = new char[]{'H', 'U', 'N', 'G', 'R', 'Y'};
        return c[(int)letter];
    }

	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	protected bool m_showDebugMessages;

	// [SerializeField]
	// private GameObject[] m_blueLetterPrefabs;   // please note that the letters should be put in the order to compose the word HUNGRY.
	[SerializeField]
	private GameObject[] m_letterPrefabs;   // please note that the letters should be put in the order to compose the word HUNGRY.
	// [SerializeField]
	// private GameObject[] m_greyLetterPrefabs;   // please note that the letters should be put in the order to compose the word HUNGRY.
	[SerializeField]
	private List<HungryLettersPlaceholder> m_easySpawnerPoints;
	[SerializeField]
	private List<HungryLettersPlaceholder> m_normalSpawnerPoints;
	[SerializeField]
	private List<HungryLettersPlaceholder> m_hardSpawnerPoints;

	[SeparatorAttribute("Audio")]
	[SerializeField]
	private string m_onCollectSound;
	[SerializeField]
	private string m_onStartLettersCollectedSound;
	[SerializeField]
	private string m_onLettersCollectedSound;
	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private static bool[] m_specificLettersCollected;

	private GameObject[] m_instantiatedLetters;
	private HungryLetter[] m_instantiatedHungryLetter;
	private int m_lettersCollected;
	private DefinitionNode m_data;

	private List<int> m_coinAwards;
	private List<int> m_scoreAwards;
	private Reward m_reward;

	//------------------------------------------------------------
	// Public Properties:
	//------------------------------------------------------------

	public static bool[] lettersCollected { get { return m_specificLettersCollected; }	}

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
        if (Application.isPlaying)
            InstanceManager.hungryLettersManager = this;

        Init();
        m_reward.origin = "letter";
	}

	protected void Init(){
		#if !UNITY_EDITOR
		m_showDebugMessages = false;
		#endif
		m_data = DefinitionsManager.SharedInstance.GetDefinition( DefinitionsCategory.HUNGRY_LETTERS, "hungry_letters");// GameDataManager.Instance.gameDB.GetItem<Definitions.HungryCollectibleData>(App.Instance.currentLevel);
		if ( m_data != null )
		{
			int sum = m_data.GetAsInt("easySpawnPercentage", 50) + m_data.GetAsInt("normalSpawnPercentage", 30) + m_data.GetAsInt("hardSpawnPercentage", 20);
			Assert.Fatal(sum == 100, "The sum of the spawn percentages set in the DB is NOT 100 (it's " + sum + "). Please fix them !!!");

			m_coinAwards = m_data.GetAsList<int>("coinsAwarded");
			m_scoreAwards = m_data.GetAsList<int>("scoreAwarded");
		}

		// instantiate the letters.
		InstantiateLetters(m_letterPrefabs);

		m_lettersCollected = 0;
	}

	protected void Start()
	{
        if (SceneController.mode != SceneController.Mode.TOURNAMENT || !HDLiveDataManager.tournament.HasModOfType(typeof(ModDragonSize))) {
            Spawn();

            // [AOC] If cheating, instantiate a dummy letter for every spawner!
            if (DebugSettings.showAllCollectibles) {
                SpawnDebug();
            }
        }
	}

	protected void OnEnable()
	{
		Messenger.AddListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);
		Messenger.AddListener(MessengerEvents.START_ALL_HUNGRY_LETTERS_COLLECTED, OnAllCollectedStart);
#if !PRODUCTION || UNITY_EDITOR
		// TODO: Recover This!
		// EventManager.Instance.RegisterEvent(Events.RespawnCollectiblesRandomly, OnDebugRespawn);
		// EventManager.Instance.RegisterEvent(Events.SpawnCollectibleEverywhere, OnSpawnEverywhere);
#endif
	}

	protected void OnDisable()
	{
		Messenger.RemoveListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);
		Messenger.RemoveListener(MessengerEvents.START_ALL_HUNGRY_LETTERS_COLLECTED, OnAllCollectedStart);
#if !PRODUCTION || UNITY_EDITOR
		// TODO: Recover This!
		// EventManager.Instance.DeregisterEvent(Events.RespawnCollectiblesRandomly, OnDebugRespawn);
		// EventManager.Instance.DeregisterEvent(Events.SpawnCollectibleEverywhere, OnSpawnEverywhere);
#endif
	}

    void OnDestroy() {
        if (Application.isPlaying && ApplicationManager.IsAlive) {
            InstanceManager.hungryLettersManager = null;
        }
    }

    //------------------------------------------------------------
    // Public Methods:
    //------------------------------------------------------------

    public void LetterCollected(HungryLetter letter)
	{
        // report analytics before to move the letter in the UI.
        HDTrackingManager.Instance.Notify_HungryLetterCollected();

        // play the sfx.
        if ( !string.IsNullOrEmpty(m_onCollectSound) )
			AudioController.Play( m_onCollectSound );
		// AudioManager.PlaySfx(AudioManager.Ui.HungryLetter);
		// place letter in the UI.
		m_specificLettersCollected[(int)letter.letter] = true;
		if(HungryLettersPanel.Instance != null)
		{
			// Mark this letter as "collected"
			// send the proper event out.
			// EventManager.Instance.TriggerEvent(Events.CollectedHungryLetter, m_data.coinsAwarded[m_lettersCollected], m_data.gemsAwarded[m_lettersCollected], m_data.spinsAwarded[m_lettersCollected]);
#if !PRODUCTION || UNITY_EDITOR
			// avoid null references when debugging...
			if(m_lettersCollected == m_letterPrefabs.Length)
			{
				m_lettersCollected = 0;
			}
#endif
			// transfer the letter in the UI.
			HungryLettersPanel.Instance.TransferLetterToUi(letter
#if !AWARD_AS_SOON_AS_COLLECTED
			, AwardStuff
#endif
			);
		}

#if AWARD_AS_SOON_AS_COLLECTED
		// award stuff.
		AwardStuff();
#endif
	}

	public bool IsLetterCollected(CollectibleLetters letter)
	{
		return m_specificLettersCollected[(int)letter];
	}

	//------------------------------------------------------------
	// Private Methods:
	//------------------------------------------------------------

	private void AwardStuff()
	{
		//TextSystem.Instance.ShowSituationalText(SituationalTextSystem.Type.HungryLetterCollected);
		// award stuff.
		if ( m_lettersCollected < m_coinAwards.Count )
			m_reward.coins = m_coinAwards[ m_lettersCollected ];
		if ( m_lettersCollected < m_scoreAwards.Count )
			m_reward.score = m_scoreAwards[ m_lettersCollected ];
		Messenger.Broadcast<Reward>(MessengerEvents.LETTER_COLLECTED, m_reward);
		//report analytics.
		// TODO: Recover analytics
		/*
		if(m_data.coinsAwarded[m_lettersCollected] > 0)
		{
			HSXAnalyticsManager.Instance.CurrencyEarned("HUNGRYLetterCoins", App.Instance.currentLevel, Bank.CurrencyType.Coins.ToString(), m_data.coinsAwarded[m_lettersCollected]);
		}
		if(m_data.gemsAwarded[m_lettersCollected] > 0)
		{
			HSXAnalyticsManager.Instance.CurrencyEarned("HUNGRYLetterGems", App.Instance.currentLevel, Bank.CurrencyType.Gems.ToString(), m_data.gemsAwarded[m_lettersCollected]);
		}
		if(m_data.spinsAwarded[m_lettersCollected] > 0)
		{
			HSXAnalyticsManager.Instance.CurrencyEarned("HUNGRYLetterSpins", App.Instance.currentLevel, Bank.CurrencyType.Spins.ToString(), m_data.spinsAwarded[m_lettersCollected]);
		}
		*/
		// update the number of letters collected after awarding because we will have the correct index for the arrays.
		m_lettersCollected++;
		// check if all letters have been collected.
		if(m_lettersCollected == m_instantiatedLetters.Length)
		{
			Messenger.Broadcast(MessengerEvents.EARLY_ALL_HUNGRY_LETTERS_COLLECTED);
			HungryLettersPanel.Instance.AllCollected();
		}
	}

	private void Spawn()
	{
		// [AOC] Skip if doing the first run tutorial!
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {
			return;
		}

		DragonTier dragonTier = InstanceManager.player.data.tier;
		// create a list of available indexes for the positions that can be used to spawn the letters.
		List<int> easyAvailablePositionIndexes = new List<int>();
		for(int i = 0; i < m_easySpawnerPoints.Count; i++)
		{
			if ( m_easySpawnerPoints[i].m_minTier <= dragonTier )
				easyAvailablePositionIndexes.Add(i);
		}
		List<int> normalAvailablePositionIndexes = new List<int>();
		for(int i = 0; i < m_normalSpawnerPoints.Count; i++)
		{
			if ( m_normalSpawnerPoints[i].m_minTier <= dragonTier )
				normalAvailablePositionIndexes.Add(i);
		}
		List<int> hardAvailablePositionIndexes = new List<int>();
		for(int i = 0; i < m_hardSpawnerPoints.Count; i++)
		{
			if ( m_hardSpawnerPoints[i].m_minTier <= dragonTier )
				hardAvailablePositionIndexes.Add(i);
		}
		HungryLettersPlaceholder placeholder = null;
		int pickedPositionIndex = -1;
		Difficulties difficulty = Difficulties.Easy;
		// spawn random letters in random locations.
		for(int i = 0; i < m_instantiatedLetters.Length; i++)
		{
			// safety check for not being stuck in a infinite loop...
			if(easyAvailablePositionIndexes.Count == 0 && hardAvailablePositionIndexes.Count == 0 && normalAvailablePositionIndexes.Count == 0)
			{
				Debug.TaggedLogError("HungryLetterManager", "No available position");
				return;
			}
			// we have some available positions, then, let's go !!
			bool ready = false;
			while(!ready)
			{
				// pick a random difficulty based on the percentages set in the DB.
				int random = Random.Range(1, 101); // 101 NOT included...
				int easySpawnPercentage = m_data.GetAsInt("easySpawnPercentage", 50);
				int normalSpawnPercentage = m_data.GetAsInt("normalSpawnPercentage", 30);
				int hardSpawnPercentage = m_data.GetAsInt("hardSpawnPercentage", 20);
				if(random <= easySpawnPercentage)
				{
					difficulty = Difficulties.Easy;
				}
				else if(random <= easySpawnPercentage + normalSpawnPercentage)
				{
					difficulty = Difficulties.Normal;
				}
				else
				{
					difficulty = Difficulties.Hard;
				}
				ShowDebugMessage("<color=lightblue>spawn percentages</color> -> <color=green>easy=" + easySpawnPercentage + "%</color> <color=white>normal=" + normalSpawnPercentage + "%</color> <color=red>hard=" + hardSpawnPercentage + "%</color> <color=lightblue>random number got</color> -> <color=yellow>" + random + "</color> <color=lightblue>so</color> -> <color=" + (difficulty == Difficulties.Easy ? "green" : difficulty == Difficulties.Normal ? "white" : "red") + ">" + difficulty.ToString() + "</color>");
				// check if there are available positions for this difficulty.
				switch(difficulty)
				{
					case Difficulties.Easy:
						if(easyAvailablePositionIndexes.Count > 0)
						{
							ready = true;
						}
						break;
					case Difficulties.Hard:
						if(hardAvailablePositionIndexes.Count > 0)
						{
							ready = true;
						}
						break;
					case Difficulties.Normal:
						if(normalAvailablePositionIndexes.Count > 0)
						{
							ready = true;
						}
						break;
				}
				// if not get another random difficulty.
				if(!ready)
				{
					ShowDebugMessage("<color=yellow>no available slots for</color> <color=" + (difficulty == Difficulties.Easy ? "green" : difficulty == Difficulties.Normal ? "white" : "red") + ">" + difficulty.ToString() + "</color><color=yellow>. rolling again...</color>");
				}
			}
			// pick a random available position for the selected difficulty and spawn the letter.
			switch(difficulty)
			{
				case Difficulties.Easy:
					// pick a random position index. PLEASE NOTE: the last is excluded for this reason is Coun and not Count-1.
					pickedPositionIndex = easyAvailablePositionIndexes[Random.Range(0, easyAvailablePositionIndexes.Count)];
					// remove the picked index from the list.
					easyAvailablePositionIndexes.Remove(pickedPositionIndex);
					// apply the randomness.
					placeholder = m_easySpawnerPoints[pickedPositionIndex];
					break;
				case Difficulties.Hard:
					// pick a random position index. PLEASE NOTE: the last is excluded for this reason is Coun and not Count-1.
					pickedPositionIndex = hardAvailablePositionIndexes[Random.Range(0, hardAvailablePositionIndexes.Count)];
					// remove the picked index from the list.
					hardAvailablePositionIndexes.Remove(pickedPositionIndex);
					// apply the randomness.
					placeholder = m_hardSpawnerPoints[pickedPositionIndex];
					break;
				case Difficulties.Normal:
					// pick a random position index. PLEASE NOTE: the last is excluded for this reason is Coun and not Count-1.
					pickedPositionIndex = normalAvailablePositionIndexes[Random.Range(0, normalAvailablePositionIndexes.Count)];
					// remove the picked index from the list.
					normalAvailablePositionIndexes.Remove(pickedPositionIndex);
					// apply the randomness.
					placeholder = m_normalSpawnerPoints[pickedPositionIndex];
					break;
			}
			m_instantiatedLetters[i].SetActive(true);
			// m_instantiatedHungryLetter[i] = m_instantiatedLetters[i].GetComponent<HungryLetter>();
			m_instantiatedHungryLetter[i].Init(this, placeholder.transform);
		}
		// Assert.Fatal(m_instantiatedLetters.Length == m_data.coinsAwarded.Length && m_instantiatedLetters.Length == m_data.scoreAwarded.Length && m_instantiatedLetters.Length == m_data.gemsAwarded.Length && m_instantiatedLetters.Length == m_data.spinsAwarded.Length, "The number of instantiated collectibles don't match with the number of rewards defined in the DB.");
	}

	/// <summary>
	/// Spawn a debug dummy letter for each spawn point. Debug only!
	/// </summary>
	private void SpawnDebug() {
		// Go through all spawn points
		List<HungryLettersPlaceholder> allSpawnPoints = new List<HungryLettersPlaceholder>(m_easySpawnerPoints.Count + m_normalSpawnerPoints.Count + m_hardSpawnerPoints.Count);
		allSpawnPoints.AddRange(m_easySpawnerPoints);
		allSpawnPoints.AddRange(m_normalSpawnerPoints);
		allSpawnPoints.AddRange(m_hardSpawnerPoints);

		// Create a new letter instance for each spawn point
		GameObject letterPrefab = m_letterPrefabs[0];
		GameObject letterObj = null;
		HungryLetter letter = null;
		Color debugColor = Colors.WithAlpha(Color.red, 0.5f);
		for(int i = 0; i < allSpawnPoints.Count; ++i) {
			// Create a new letter instance
			letterObj = GameObject.Instantiate<GameObject>(letterPrefab);
			letter = letterObj.GetComponent<HungryLetter>();
			letter.Init(this, allSpawnPoints[i].transform);

			// Do some customization to let know everyone these are debug letters
			// Tint map marker
			letter.mapMarker.GetComponentInChildren<SpriteRenderer>().color = debugColor;

			// Move a bit further away so we can see actual letters in the map
			letter.transform.SetPosZ(letter.transform.position.z + 1);

			// Disable collider
			letter.collider.enabled = false;
		}
	}

	private void InstantiateLetters(GameObject[] prefabList)
	{
		m_instantiatedLetters = new GameObject[prefabList.Length];
		m_instantiatedHungryLetter = new HungryLetter[prefabList.Length];
		for(int i = 0; i < prefabList.Length; i++)
		{
			m_instantiatedLetters[i] = Instantiate(prefabList[i]);
			m_instantiatedLetters[i].SetActive(false);
			m_instantiatedHungryLetter[i] = m_instantiatedLetters[i].GetComponent<HungryLetter>();
		}

		//Which letters have we picked so far? None at the moment we spawn them.
		m_specificLettersCollected = new bool[prefabList.Length];
		for(int i = 0; i < prefabList.Length; i++)
		{
			m_specificLettersCollected[i] = false;
		}
	}

	private void ShowDebugMessage(string message)
	{
		if(m_showDebugMessages)
		{
			Debug.Log(message);
		}
	}

	private void Reset()
	{
		//Mark all the letters as "not collected"
		for(int i = 0; i < m_specificLettersCollected.Length; i++)
		{
			m_specificLettersCollected[i] = false;
		}

		/*
		// destroy the previous letters.
		for(int i = 0; i < m_instantiatedLetters.Length; i++)
		{
			Destroy(m_instantiatedLetters[i]);
		}
		*/

		// reset the counter.
		m_lettersCollected = 0;
	}

	public void Respawn()
	{
		Reset();
		// instantiate the new letters.
		// InstantiateLetters(m_letterPrefabs);

		// spawn the new letters.
		Spawn();
		// reinitialize the UI.
		HungryLettersPanel.Instance.ReInit(m_instantiatedLetters.Length);
	}

	public GameObject GetClosestActiveLetter( Vector3 center )
	{
		GameObject go = null;
		float minDistance = float.MaxValue;
		int size = m_instantiatedHungryLetter.Length;
		for( int i = 0; i<size; ++i )
		{
			if ( !IsLetterCollected( m_instantiatedHungryLetter[i].letter )	)
			{
				float dist = (center - m_instantiatedHungryLetter[i].transform.position).sqrMagnitude;
				if ( dist < minDistance )
				{
					minDistance = dist;
					go = m_instantiatedLetters[i];
				}
			}
		}
		return go;
	}

	//------------------------------------------------------------
	// Event Handlers:
	//------------------------------------------------------------

	private void OnAllCollectedStart()
	{
		if ( !string.IsNullOrEmpty( m_onStartLettersCollectedSound ) )
			AudioController.Play(m_onStartLettersCollectedSound);
	}

	private void OnSuperSizeToggle(bool _activated, DragonSuperSize.Source _source)
	{
		if (_source == DragonSuperSize.Source.LETTERS) {
			if (!_activated)
			{
				// respawn the letters after a SSM event only when we have already collected all of them.
				if(m_lettersCollected == m_letterPrefabs.Length)
				{
					Respawn();
				}	
			}
			else
			{
				if ( !string.IsNullOrEmpty(m_onLettersCollectedSound) )
					AudioController.Play(m_onLettersCollectedSound);
			}
		}
	}

	//------------------------------------------------------------
	// QC Debug Stuff:
	//------------------------------------------------------------

#if !PRODUCTION || UNITY_EDITOR
	private void OnDebugRespawn(System.Enum eventReceived, object[] args)
	{
		Respawn();
	}

	private void OnSpawnEverywhere(System.Enum eventReceived, object[] args)
	{
		CollectibleLetters letter = (CollectibleLetters)args[0];
		Reset();
		// instantiate the new letters.
		m_instantiatedLetters = new GameObject[m_easySpawnerPoints.Count + m_normalSpawnerPoints.Count + m_hardSpawnerPoints.Count];
		GameObject prefab = null;
		prefab = m_letterPrefabs[(int)letter];
		Assert.Fatal(prefab != null, "The prefab of the letter is null !!");
		for(int i = 0; i < m_instantiatedLetters.Length; i++)
		{
			m_instantiatedLetters[i] = Instantiate(prefab);
			m_instantiatedLetters[i].SetActive(false);
		}
		// spawn everywhere.
		int counter = m_instantiatedLetters.Length - 1;
		for(int i = 0; i < m_easySpawnerPoints.Count; i++)
		{
			m_instantiatedLetters[counter].SetActive(true);
			m_instantiatedLetters[counter].GetComponent<HungryLetter>().Init(this, m_easySpawnerPoints[i].transform);
			counter--;
		}
		for(int i = 0; i < m_normalSpawnerPoints.Count; i++)
		{
			m_instantiatedLetters[counter].SetActive(true);
			m_instantiatedLetters[counter].GetComponent<HungryLetter>().Init(this, m_normalSpawnerPoints[i].transform);
			counter--;
		}
		for(int i = 0; i < m_hardSpawnerPoints.Count; i++)
		{
			m_instantiatedLetters[counter].SetActive(true);
			m_instantiatedLetters[counter].GetComponent<HungryLetter>().Init(this, m_hardSpawnerPoints[i].transform);
			counter--;
		}
		// reinitialize the UI.
		HungryLettersPanel.Instance.ReInit(m_instantiatedLetters.Length);
	}

	public void ClearSpawnPoints()
	{
		m_easySpawnerPoints.Clear();
		m_normalSpawnerPoints.Clear();
		m_hardSpawnerPoints.Clear();
	}

	public void AddSpawnerPoint(HungryLettersPlaceholder _point, HungryLettersManager.Difficulties _difficulty )
	{
		switch( _difficulty )
		{
			case Difficulties.Easy:
			{
				m_easySpawnerPoints.Add( _point );
			}break;
			case Difficulties.Normal:
			{
				m_normalSpawnerPoints.Add( _point );
			}break;
			case Difficulties.Hard:
			{
				m_hardSpawnerPoints.Add( _point );
			}break;
			default:
			{
				
			}break;
		}
	}

#endif
}