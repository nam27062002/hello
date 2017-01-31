// OpenEggSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the 3D scene of the Open Egg screen.
/// </summary>
public class OpenEggSceneController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Transform m_eggAnchor = null;
	[SerializeField] private Transform m_rewardAnchor = null;
	[Space]
	//[SerializeField] private GodRaysFX m_rewardGodRaysFX = null;	// [AOC] Commented out until fixed
	[SerializeField] private GodRaysFXFast m_godRaysFX = null;
	[SerializeField] private Transform m_tapFXPool = null;
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_tapFX = new ParticleSystem[(int)EggReward.Rarity.COUNT];
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_openFX = new ParticleSystem[(int)EggReward.Rarity.COUNT];
	[SerializeField] private ParticleSystem m_explosionFX = null;


	// Events
	public UnityEvent OnIntroFinished = new UnityEvent();
	public UnityEvent OnEggOpenFinished = new UnityEvent();

	// Internal
	private GameObject m_rewardView = null;

	private EggController m_eggView = null;
	public EggController eggView {
		get { return m_eggView; }
	}

	public Egg eggData {
		get { return (m_eggView == null) ? null : m_eggView.eggData; }
	}


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Clear();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<EggController, int>(GameEvents.EGG_TAP, OnEggTap);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<EggController, int>(GameEvents.EGG_TAP, OnEggTap);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Clear();
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnValidate() {
		// Make sure the rarity array has exactly the same length as rarities in the game.
		m_openFX.Resize((int)EggReward.Rarity.COUNT);
		m_tapFX.Resize((int)EggReward.Rarity.COUNT);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear the whole 3D scene.
	/// </summary>
	public void Clear() {
		// Return tap FX to the pool
		for(int i = 0; i < m_tapFX.Length; i++) {
			if(m_tapFX[i] != null) {
				m_tapFX[i].transform.SetParent(m_tapFXPool);
				m_tapFX[i].Stop(true);
				m_tapFX[i].gameObject.SetActive(false);
			}
		}

		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;
		}

		if(m_rewardView != null) {
			GameObject.Destroy(m_rewardView);
			m_rewardView = null;
		}

		/*if(m_rewardGodRaysFX != null) {
			m_rewardGodRaysFX.StopFX();
		}*/
		if(m_godRaysFX != null) {
			m_godRaysFX.StopFX();
		}
		
		for(int i = 0; i < m_openFX.Length; i++) {
			if(m_openFX[i] != null) {
				m_openFX[i].Stop(true);
			}
		}

		if(m_explosionFX != null) {
			m_explosionFX.Stop(true);
		}
	}

	/// <summary>
	/// Initialize the egg view with the given egg data. Optionally reuse an existing
	/// egg view.
	/// </summary>
	/// <param name="_egg">The egg to be opened.</param>
	public void InitEggView(Egg _egg) {
		// Clear any active stuff
		Clear();

		// Create a new instance of the egg prefab
		m_eggView = _egg.CreateView();

		// Attach it to the 3d scene's anchor point
		// Make sure anchor is active!
		m_eggAnchor.gameObject.SetActive(true);
		m_eggView.transform.SetParent(m_eggAnchor, false);
		m_eggView.transform.position = m_eggAnchor.position;

		// Launch intro as soon as possible (wait for the camera to stop moving)
		m_eggView.gameObject.SetActive(false);

		// [AOC] Hacky!! Disable particle FX (Premium Egg Idle)
		ParticleSystem[] particleFX = m_eggView.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < particleFX.Length; i++) {
			particleFX[i].gameObject.SetActive(false);
		}

		// Attach tap FX to the egg's view (but don't activate it just yet)
		ParticleSystem tapFX = m_tapFX[(int)eggData.rewardData.rarity];
		if(tapFX != null) {
			tapFX.transform.SetParentAndReset(m_eggView.anchorFX);
			tapFX.gameObject.SetActive(false);
		}
	}

	//------------------------------------------------------------------//
	// ANIMATIONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start the intro animation for the egg!
	/// </summary>
	public void LaunchIntro() {
		// Ignore if we don't have a valid egg view
		if(m_eggView == null) return;

		// Assume we can do it (no checks)
		// Activate egg
		m_eggView.gameObject.SetActive(true);

		// [AOC] TODO!! Some awesome FX!!
		m_eggView.transform.DOScale(0f, 0.5f).From().SetEase(Ease.OutElastic).OnComplete(OnIntroFinishedCallback);
	}

	/// <summary>
	/// Launch the egg open animation.
	/// </summary>
	public void LaunchOpenEggAnim() {
		// Ignore if we don't have a valid egg view
		if(m_eggView == null) return;

		// Hide egg
		m_eggView.gameObject.SetActive(false);

		// Trigger the proper FX based on reward rarity
		ParticleSystem openFX = m_openFX[(int)eggData.rewardData.rarity];
		if(openFX != null) {
			openFX.Clear();
			openFX.Play(true);
		}

		// Explosion FX
		// Match material with the egg shell!
		if(m_explosionFX != null) {
			m_explosionFX.Play();
		}

		// Program reward animation
		Invoke("OnEggOpenFinishedCallback", 0.35f);
	}

	/// <summary>
	/// Replace the egg by its reward.
	/// </summary>
	public void LaunchRewardAnim() {
		// Ignore if we don't have a valid egg view
		if(m_eggView == null) return;

		// Aux vars
		EggReward rewardData = eggData.rewardData;
		DefinitionNode rewardDef = eggData.rewardData.def;

		// Create a fake reward view
		switch(rewardData.type) {
			case "pet": {
				// Show a 3D preview of the pet
				m_rewardView = new GameObject("RewardView");
				m_rewardView.transform.SetParentAndReset(m_rewardAnchor);	// Attach it to the anchor and reset transformation

				// Use a PetLoader to simplify things
				MenuPetLoader loader = m_rewardView.AddComponent<MenuPetLoader>();
				loader.Setup(MenuPetLoader.Mode.MANUAL, MenuPetPreview.Anim.IN, true);
				loader.Load(rewardData.itemDef.sku);

				// Animate it
				m_rewardView.transform.DOScale(0f, 1f).SetDelay(0.1f).From().SetRecyclable(true).SetEase(Ease.OutBack);
				m_rewardView.transform.DOLocalRotate(m_rewardView.transform.localRotation.eulerAngles + Vector3.up * 360f, 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetDelay(0.5f).SetRecyclable(true);
			} break;
		}

		// If the reward is being replaced by coins, show it (works for any type of reward)
		if(rewardData.coins > 0) {
			// Create instance
			GameObject prefab = Resources.Load<GameObject>("UI/Metagame/Rewards/PF_CoinsReward");
			GameObject coinsObj = GameObject.Instantiate<GameObject>(prefab);

			// Attach it to the reward view and reset transformation
			coinsObj.transform.SetParent(m_rewardView.transform, false);
		}

		// Show reward godrays
		// Except if duplicate! (for now)
		/*if(m_rewardGodRaysFX != null) {
			m_rewardGodRaysFX.StartFX(eggData.rewardData.rarity);
		}
		*/
		if(m_godRaysFX != null && !eggData.rewardData.duplicated) {
			// Custom color based on reward's rarity
			m_godRaysFX.StartFX(eggData.rewardData.rarity);

			// Show with some delay to sync with pet's animation
			m_godRaysFX.transform.DOScale(0f, 0.05f).From().SetDelay(0.15f).SetRecyclable(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The intro anim has finished.
	/// </summary>
	private void OnIntroFinishedCallback() {
		// Notify external scripts
		OnIntroFinished.Invoke();
	}

	/// <summary>
	/// The open egg animation has finished.
	/// </summary>
	private void OnEggOpenFinishedCallback() {
		// Notify external scripts
		OnEggOpenFinished.Invoke();
	}

	/// <summary>
	/// An opening egg has been tapped.
	/// </summary>
	/// <param name="_egg">The egg that has been tapped.</param>
	/// <param name="_tapCount">Tap count.</param>
	private void OnEggTap(EggController _egg, int _tapCount) {
		// Show the right particle effect based on rarity!
		if(_tapCount == 1 && _egg == m_eggView) {
			// Activate FX
			ParticleSystem tapFX = m_tapFX[(int)_egg.eggData.rewardData.rarity];
			tapFX.gameObject.SetActive(true);
			tapFX.Stop(true);
			tapFX.Clear();
			tapFX.Play(true);

			// Disable smoke FX
		}
	}
}