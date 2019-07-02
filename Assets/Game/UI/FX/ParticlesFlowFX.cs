// CurrencyTransferFX.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using DG.Tweening;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Visual effect to make a flock of sprites fly from one point of the screen to another.
/// Typically used to represent currency transfers.
/// </summary>
public class ParticlesFlowFX : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string COINS = "UI/FX/PF_CoinsTransferFX";
	public const string PC = "UI/FX/PF_PCTransferFX";
    public const string GOLDEN_FRAGMENTS = "UI/FX/PF_GoldenFragmentsTransferFX";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_prefab;
	public GameObject prefab {
		get { return m_prefab; }
		set { m_prefab = value; ValidatePool(); }
	}

    [SerializeField] private Vector3 m_from = Vector3.zero;
	public Vector3 fromPos {
		get { return m_from; }
		set { m_from = value; InitVars(); }
	}

    [SerializeField] private Vector3 m_to = Vector3.one;
	public Vector3 toPos {
		get { return m_to; }
		set { m_to = value; InitVars(); }
	}

	[SerializeField] private bool m_autoKill = false;
	public bool autoKill {
		get { return m_autoKill; }
		set { m_autoKill = value; }
	}

	// System parameters
	[SerializeField] private float m_totalDuration = 5f;
	public float totalDuration {
		get { return m_totalDuration; }
		set { m_totalDuration = value; }
	}

	[SerializeField] [Range(0f, 100f)] private float m_rate = 30f;
	public float rate {
		get { return m_rate; }
		set { m_rate = value; InitVars(); }
	}

	[SerializeField] private float m_inRelativeDuration = 0.15f;
	public float inRelativeDuration {
		get { return m_inRelativeDuration; }
		set { m_inRelativeDuration = value; }
	}

	[SerializeField] private float m_outRelativeDuration = 0.25f;
	public float outRelativeDuration {
		get { return m_outRelativeDuration; }
		set { m_outRelativeDuration = value; }
	}

	[SerializeField] private Ease m_ease = Ease.InOutQuad;
	public Ease ease {
		get { return m_ease; }
		set { m_ease = value; }
	}

	// Particle parameters
	[SerializeField] private Range m_speed = new Range(5f, 10f);	// World units per second
	public Range speed {
		get { return m_speed; }
		set { m_speed = value; }
	}

	[SerializeField] private Range m_rotationSpeed = new Range(360f, 540f);	// Degrees per second, will be randomized for each axis
	public Range rotationSpeed {
		get { return m_rotationSpeed; }
		set { m_rotationSpeed = value; }
	}

	[SerializeField] private Range m_amplitude = new Range(-0.15f, 0.15f);	// Percentage of the total distance to go, so the curve aperture is proportional regardless of the distance.
	public Range amplitude {
		get { return m_amplitude; }
		set { m_amplitude = value; }
	}

	[SerializeField] private Ease m_amplitudeEase = Ease.InOutQuad;
	public Ease amplitudeEase {
		get { return m_amplitudeEase; }
		set { m_amplitudeEase = value; }
	}

	// Events
	public UnityEvent OnStart = new UnityEvent();
	public UnityEvent OnStop = new UnityEvent();
	public UnityEvent OnPause = new UnityEvent();
	public UnityEvent OnResume = new UnityEvent();
	public UnityEvent OnFinish = new UnityEvent();
	public UnityEvent OnKill = new UnityEvent();

	// Particle Pools
	private PoolHandler m_pool = null;
	public PoolHandler pool { get { return m_pool; }}
	private List<Tween> m_activeTweens = new List<Tween>();
	private bool m_activeTweensLock = false;

	// Internal logic
	private bool m_active = false;
	public bool active {
		get { return m_active; }
	}

	private float m_activeTime = 0f;
	public float activeTime {
		get { return m_activeTime; }
	}

	private float m_lastEmissionTime = 0f;
	private bool m_autoKillPending = false;

	// Cache some values
	private float m_frequency = 0f;
	private Vector3 m_offset = Vector3.right;
	private Vector3 m_dir = Vector3.right;
	private Vector3 m_perpDir = Vector3.up;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Make sure we have a valid pool!
		ValidatePool();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// If waiting for autokill, wait until there are no active tweens and kill it!
		if(m_autoKillPending && m_activeTweens.Count == 0) {
			KillFX();
			return;
		}

		// Nothing to do if not active
		if(!m_active) return;

		// Check total timer
		m_activeTime += Time.deltaTime;
		if(m_activeTime >= m_totalDuration) {
			// Notify listeners
			OnFinish.Invoke();

			// Stop emitting! Will be auto-killed if configured.
			StopFX();
			return;
		}

		// Check emission timer
		float emissionDelta = Time.time - m_lastEmissionTime;
		while(emissionDelta >= m_frequency) {
			// Spawn as many times as needed! (in case of low framerate, more than one particle could be spawned at the same frame)
			Spawn();
			emissionDelta -= m_frequency;
		}
	}

	//------------------------------------------------------------------------//
	// SYSTEM CONTROL METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start the show!
	/// </summary>
	public void StartFX() {	// Weird name to prevent conflicts with Monobehaviour Start() message.
		// Reset timer
		m_activeTime = 0f;

		// Cache some values
		InitVars();

		// Make sure we have a valid pool!
		ValidatePool();

		// Start it!
		m_active = true;
		m_autoKillPending = false;

		// Notify listeners
		OnStart.Invoke();

		// Spawn the first particle!
		Spawn();
	}

	/// <summary>
	/// Stops the system. If set to auto-kill, it will be destroyed as well.
	/// </summary>
	public void StopFX() {
		// Stop emitting
		m_active = false;

		// Auto-kill?
		if(m_autoKill) {
			m_autoKillPending = true;
		}

		// Notify listeners
		OnStop.Invoke();
	}

	/// <summary>
	/// Pause the system.
	/// </summary>
	public void PauseFX() {
		// That will do it
		m_active = false;

		// Notify listeners
		OnPause.Invoke();
	}

	/// <summary>
	/// Resume after a pause. Activates the system but doesn't reset the timer.
	/// </summary>
	public void ResumeFX() {
		// Activate, but don't reset timer.
		m_active = true;

		// Notify listeners
		OnResume.Invoke();
	}

	/// <summary>
	/// Stop and instantly kill this FX.
	/// </summary>
	public void KillFX() {
		// Stop the effect
		m_active = false;
		m_autoKillPending = false;

		// Kill all active tweens
		// Prevent them to auto-remove themselves from the list while iterating it
		m_activeTweensLock = true;
		for(int i = 0; i < m_activeTweens.Count; ++i) {
			m_activeTweens[i].Kill(true);
		}
		m_activeTweens.Clear();
		m_activeTweensLock = false;

		// Notify listeners
		OnKill.Invoke();

		// Destroy ourselves
		// [AOC] TODO!! Allow the option to not-self destroy (for effects that might be constantly spawning)
		GameObject.Destroy(this.gameObject);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize internal vars cached for better performance.
	/// To be called every time Fromm, To or Rate change.
	/// </summary>
	private void InitVars() {
		m_frequency = 1f/m_rate;
		m_offset = m_to - m_from;
		m_dir = m_offset.normalized;
		m_perpDir = new Vector3(m_dir.y, -m_dir.x, m_dir.z);	// Compute direction as perpendicular of main movement. See https://gamedev.stackexchange.com/questions/70075/how-can-i-find-the-perpendicular-to-a-2d-vector
	}

	/// <summary>
	/// Make sure we have a valid pool!
	/// </summary>
	private void ValidatePool() {
		// If prefab is not valid, invalidate pool as well
		if(m_prefab == null) {
			m_pool = null;
			return;
		}

		// Make sure we have the right pool
		m_pool = UIPoolManager.GetHandler(m_prefab.name);

		// If pool doesn't exist, create it now
		if(m_pool == null) {
			m_pool = UIPoolManager.CreatePool(m_prefab);
		}
	}

	/// <summary>
	/// Spawn a single particle.
	/// </summary>
	private void Spawn() {
		// Pool must be valid
		if(m_pool == null) return;

		// Grab a particle from the pool
		GameObject targetObj = m_pool.GetInstance();
		if(targetObj == null) return;

		// Add as children of the effect so particles are rendered at the expected depth
		Transform target = targetObj.transform;
		target.SetParent(this.transform, false);

		// Get the canvas group to allow fade animations. Add one if not found.
		CanvasGroup canvas = target.ForceGetComponent<CanvasGroup>();

		// Aux vars
		float speed = m_speed.GetRandom();
		float duration = m_offset.magnitude / speed;

		// Set initial values
		target.gameObject.SetActive(true);
		target.position = m_from;
		target.localScale = Vector3.zero;
		target.localRotation = Quaternion.identity;
		target.gameObject.SetLayerRecursively(this.gameObject.layer);
		canvas.alpha = 0f;

		// Program a new animation for this particle!
		Sequence seq = DOTween.Sequence()
			.SetRecyclable(true);
			
		// Main movement tween -------------------------------------------------
		seq.Append(
			target.DOBlendableMoveBy(m_offset, duration)
			.SetEase( m_ease)
		);

		// Amplitude variation -------------------------------------------------
		float amplitude = m_amplitude.GetRandom() * m_offset.magnitude;	// Amplitude is represented as percentage of the total offset to keep it proportional independently of the distance to move
		seq.Join(
			// Join in a sequence to be able to ease both loops as one single tween
			DOTween.Sequence().Append(
				target.DOBlendableMoveBy(m_perpDir * amplitude, duration/2f)	// Half duration, 2 yoyo loops. Curve could be parametrized.
				.SetLoops(2, LoopType.Yoyo)
				.SetEase(Ease.InOutQuad)
			).SetEase(m_amplitudeEase)
		);

		// Rotation ------------------------------------------------------------
		Vector3 rotSpeed = new Vector3(
			m_rotationSpeed.GetRandom(),
			m_rotationSpeed.GetRandom(),
			m_rotationSpeed.GetRandom()
		);

		target.DOBlendableLocalRotateBy(Vector3.right * 360f, rotSpeed.x, RotateMode.FastBeyond360)
			.SetSpeedBased()
			.SetLoops(-1, LoopType.Restart);

		target.DOBlendableLocalRotateBy(Vector3.up * 360f, rotSpeed.y, RotateMode.FastBeyond360)
			.SetSpeedBased()
			.SetLoops(-1, LoopType.Restart);

		target.DOBlendableLocalRotateBy(Vector3.forward * 360f, rotSpeed.z, RotateMode.FastBeyond360)
			.SetSpeedBased()
			.SetLoops(-1, LoopType.Restart);

		// In animation --------------------------------------------------------
		// Scale In
		float inDuration = duration * m_inRelativeDuration;
		seq.Join(
			target.DOScale(Vector3.one, inDuration)
		);

		// Fade In
		seq.Join(
			canvas.DOFade(1f, inDuration)
		);

		// Out animation -------------------------------------------------------
		// Scale Out
		float outDuration = duration * m_outRelativeDuration;
		float outStart = duration - outDuration;
		seq.Insert(
			outStart,
			target.DOScale(Vector3.zero, outDuration)
		);

		// Fade Out
		seq.Insert(
			outStart,
			canvas.DOFade(0f, outDuration)
		);

		// Once sequence has finished, go back to the pool!
		seq.OnComplete(
			() => {
				// Kill all tweens on the target
				target.DOKill();

				// Put target back to the pool
				m_pool.ReturnInstance(target.gameObject);

				// Remove sequence from active tweens list
				if(!m_activeTweensLock) m_activeTweens.Remove(seq);	// Unless locked!

				// Disable the target
				target.gameObject.SetActive(false);
			}
		);

		// Store sequence to the active tweens list
		m_activeTweens.Add(seq);

		// Done! Reset emission timer
		m_lastEmissionTime = Time.time;
	}

	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create and play a transfer FX with the given parameters.
	/// Most parameters can still be changed right after instantiation.
	/// Will be auto-killed when finished, unless auto-kill flag is turned off.
	/// </summary>
	/// <param name="_prefab">Particle prefab.</param>
	/// <param name="_parent">Where to instantiate the new FX.</param>
	/// <param name="_from">Initial position, world coordinates.</param>
	/// <param name="_to">Final position, world coordinates.</param>
	/// <param name="_duration">Total duration.</param>
	public static ParticlesFlowFX CreateAndLaunch(GameObject _prefab, Transform _parent, Vector3 _from, Vector3 _to, float _duration) {
		// Ignore if prefab is null
		if(_prefab == null) return null;

		// Crete a new game object and attach the Transfer FX component
		GameObject obj = new GameObject();
		ParticlesFlowFX newInstance = obj.AddComponent<ParticlesFlowFX>();

		// Initialize new instance
		InitNewInstance(ref newInstance, _parent, _from, _to);

		// Setup given parameters
		newInstance.prefab = _prefab;
		newInstance.totalDuration = _duration;

		// Auto-play!
		newInstance.StartFX();

		// Done!
		return newInstance;
	}

	/// <summary>
	/// Instantiates the given FX prefab and launches it with its defined setup.
	/// Most parameters can still be changed right after instantiation.
	/// Will be auto-killed when finished unless the flag is turned off.
	/// </summary>
	/// <returns>The newly created FX.</returns>
	/// <param name="_prefab">FX prefab to be instantiated.</param>
	/// <param name="_parent">Where to instantiate the new FX.</param>
	/// <param name="_from">Initial position, world coordinates.</param>
	/// <param name="_to">Final position, world coordinates.</param>
	public static ParticlesFlowFX InstantiateAndLaunch(GameObject _prefab, Transform _parent, Vector3 _from, Vector3 _to) {
		// Given prefab must be valid
		if(_prefab == null) return null;

		// And have the TransferFX component
		if(_prefab.GetComponent<ParticlesFlowFX>() == null) return null;

		// Create a new instance
		ParticlesFlowFX fx = GameObject.Instantiate<GameObject>(_prefab).GetComponent<ParticlesFlowFX>();

		// Initialize fx instance
		InitNewInstance(ref fx, _parent, _from, _to);

		// Auto-play!
		fx.StartFX();

		// Done!
		return fx;
	}

	/// <summary>
	/// Load a FX prefab from resources, create a new instance a and launch it with its defined setup.
	/// Most parameters can still be changed right after instantiation.
	/// Will be auto-killed when finished unless the flag is turned off.
	/// </summary>
	/// <returns>The newly created FX.</returns>
	/// <param name="_prefabPath">The Resources path of the FX prefab to be instantiated. Use the constants in this class for most common FX.</param>
	/// <param name="_parent">Where to instantiate the new FX.</param>
	/// <param name="_from">Initial position, world coordinates.</param>
	/// <param name="_to">Final position, world coordinates.</param>
	public static ParticlesFlowFX LoadAndLaunch(string _prefabPath, Transform _parent, Vector3 _from, Vector3 _to) {
		// Load the prefab from resources and use corresponding method
		GameObject prefab = Resources.Load<GameObject>(_prefabPath);
		return InstantiateAndLaunch(prefab, _parent, _from, _to);
	}

	/// <summary>
	/// Given a game currency, return the default prefab for it.
	/// </summary>
	/// <returns>The path of the default prefab for the given currency.</returns>
	/// <param name="_currency">Currency.</param>
	public static string GetDefaultPrefabPathForCurrency(UserProfile.Currency _currency) {
		switch(_currency) {
			case UserProfile.Currency.SOFT: return COINS;
			case UserProfile.Currency.HARD: return PC;
			case UserProfile.Currency.GOLDEN_FRAGMENTS: return GOLDEN_FRAGMENTS;
		}
		return string.Empty;
	}

	/// <summary>
	/// Internal initializer for a given instance.
	/// </summary>
	/// <param name="_newInstance">The FX instance to be initialized.</param>
	/// <param name="_parent">Where to put the new FX.</param>
	/// <param name="_from">Initial position, world coordinates.</param>
	/// <param name="_to">Final position, world coordinates.</param>
	private static void InitNewInstance(ref ParticlesFlowFX _newInstance, Transform _parent, Vector3 _from, Vector3 _to) {
		// Setup given parameters
		_newInstance.fromPos = _from;
		_newInstance.toPos = _to;

		// Initialize some extra parameters
		_newInstance.autoKill = true;

		// Move into parent's hierarchy
		if(_parent != null) {
			// Apply parent's layer as well
			_newInstance.transform.SetParent(_parent, false);
			_newInstance.gameObject.SetLayerRecursively(_parent.gameObject.layer);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}