/* PLEASE NOTE
   If it is needed to add a new value to the enum, please, don't try to rearrange the list and add the new one on the bottom.
   This is because this enumeration is used also in some UI elements, so, changing the position of the values will also change
   the selected value of this enum in the UI where required. (E.G. the quit button in the ingame pause menu).
 	*/
public enum Events
{
    LevelSelect,
	LevelUnlock,
	LevelUnlockDisplay,
	LevelUnlockFinished,
    LocationTap,
    ScreenLoaded,
    SharkClick,
    SharkSelect,
	SharkCentered,
	SharkTierSelect,
	SharkPurchase,
    SharkDeath,
    StateChange,
    KilledByPlayer,
    SessionData,
	ScoreUpdate,
    InventoryReward,
    GameBonus,
    MissionComplete,
    ShowMissionCompleteText,
    MissionRefresh,
    ShoalComplete,
    GoldRushStart,
    GoldRushEnd,
    PlayerDamage,
    MultiplierUpdate,
    LatchOn,
    StartBoostBarOnboarding,
    StopBoostBarOnboarding,
    EquippableAttached,
    ResetEquippables,
    ShowResultScreen,
    SharkLevelUp,
    ShowSaveMeNotification,
    StartSharkRebirth,
    SharkRebirth,
    ShowInvincibilityParticles,
    ShowPoisonParticles,
    StopStatusParticles,
    StateLoaded,
    EnableIngameHUD,
	UIAttributeUpdating,
	UIAttributeUpdated,
    UILevelUpAnim,
	UIUpgradeAttributeAnim,
    UITriggerResultsLevelUp,
    UITriggerResultsCoinCount,
	UITriggerResultCarousel,
    PreviewPet,
	SharkHitWater,
	BankUpdated,
	SurvivalBonusAchieved,
	SharkTeleporting,
	ClosedIAPPopup,
	UIAccessoryPanelDisabled,
	EquippableSelected,
	UIItemBoughtAnim,
	EquippableRemoved,
    InGameLoaded,
    GameFullyLoaded,
	UISharkBoughtAnim,
    UISharkUnlockAnim,
	AppPaused,
	UIUnlockTierAnimationStart,
	UIUnlockTierAnimationEnd,
    UIUnlockTierAnimationFireworks,
	GoldRushDeactivateParticles,
	MegaGoldRushDeactivateParticles,
	UIShopItemClicked,
	UIWorldSpinning,
	GoInGameButtonPressed,
    SharkSelectOnboardingComplete,
    UIWorldStartDrag,
	UIButtonControlsTutorialClosed,
	CloseResultScreen,
	LevelSelectOnboardingComplete,
    OnGameDBLoaded,
    ParticleManagerReady,
    ShopNewStateChange,
    LoadingCurtainToMenuDismissed,
    SharkSelectInteractablesToggle,
    CollectedHungryLetter,
	OnTeleportStarted,
	OnTeleportEnded,
    SpecialSharkLockStateChange,
    OnSharkBoughtAnimFinished,
    PushNotificationReceived,
	AllHungryLettersCollected,
	ToggleInfiniteHealthFX,
	ToggleUnlimitedBoostFX,
	PetSelected,
	SuperSizeModeStart,
	SuperSizeModeEnd,
	PlayerBoosting,
    OnPetEquipped,
    SharkScalingOnFrontEndFinished,
	OnGameServicesInitialized,
    DisplayMissionAreaName,
	EarlyAllHungryLettersCollected,
	InGameAudioPaused,
    ChangedLanguage,
	SessionCurrencySubstracted,
	TreasureChestCollected,
	SharkViewerButtonClicked,
	SharkSelectDragStart,
	OnUserLoggedIn, // receives User class as only param
	OnUserLoggedOut, // receives NULL as params
	SharkTierInfoPopupClosed,
	OnLeaderboardEntryClicked,
    OnFriendListItemsClicked,
	None,
	SharkSelectOnboardingAnimatorFinished,
	DailyEventItemRewardAwarded,
	PauseButtonStateChanged, // pause button in game state change (disabled / enabled)
    PromoPackAwardComplete,
	/**
	 * ENTER NEW EVENTS HERE!!!
	 */

#if !PRODUCTION || UNITY_EDITOR
	GoldRushDebug,
	MegaGoldRushDebug,
	DebugCompleteMission,
	MultiSceneStart,
	SpawnCollectibleEverywhere,
	RespawnCollectiblesRandomly,
#endif
}