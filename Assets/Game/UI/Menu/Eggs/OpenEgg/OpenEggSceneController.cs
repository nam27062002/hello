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
	[SerializeField] private ParticleSystem m_openEggFX = null;
	[SerializeField] private GodRaysFX m_rewardGodRaysFX = null;

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

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Clear();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear the whole 3D scene.
	/// </summary>
	public void Clear() {
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;
		}

		if(m_rewardView != null) {
			GameObject.Destroy(m_rewardView);
			m_rewardView = null;
		}

		if(m_rewardGodRaysFX != null) {
			m_rewardGodRaysFX.StopFX();
		}

		if(m_openEggFX != null) {
			m_openEggFX.Stop();
		}
	}

	/// <summary>
	/// Initialize the egg view with the given egg data. Optionally reuse an existing
	/// egg view.
	/// </summary>
	/// <param name="_egg">The egg to be opened.</param>
	public void InitEggView(Egg _egg) {
		// If we already have an egg view, destroy it
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;
		}

		// Create a new instance of the egg prefab
		m_eggView = _egg.CreateView();

		// Attach it to the 3d scene's anchor point
		// Make sure anchor is active!
		m_eggAnchor.gameObject.SetActive(true);
		m_eggView.transform.SetParent(m_eggAnchor, false);
		m_eggView.transform.position = m_eggAnchor.position;

		// Launch intro as soon as possible (wait for the camera to stop moving)
		m_eggView.gameObject.SetActive(false);

		// [AOC] Hacky!! Disable particle FX
		ParticleSystem[] particleFX = m_eggView.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < particleFX.Length; i++) {
			particleFX[i].gameObject.SetActive(false);
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

		// Trigger FX
		if(m_openEggFX != null) {
			m_openEggFX.Clear();
			m_openEggFX.Play(true);
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
				loader.Setup(MenuPetLoader.Mode.MANUAL, MenuPetLoader.Anim.BREAK_EGG, true);
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
		// Custom color based on reward's rarity
		if(m_rewardGodRaysFX != null) {
			m_rewardGodRaysFX.StartFX(eggData.rewardData.def.Get("rarity"));

			/*// Show with some delay to sync with pet's animation
			m_rewardGodRaysFX.transform.DOScale(0f, 0.05f).From().SetDelay(0.15f).SetRecyclable(true).OnStart(
				() => {
					m_rewardGodRaysFX.StartFX(eggData.rewardDef.Get("rarity"));
				}
			);*/
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The intro anim has finished.
	/// </summary>
	private void OnIntroFinishedCallback() {
		// Change egg state
		m_eggView.eggData.ChangeState(Egg.State.OPENING);

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
}