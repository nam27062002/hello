//--------------------------------------------------------------------------------
// GameCamera.cs
//--------------------------------------------------------------------------------
// Handles the logic for moving the camera around in-game.
//--------------------------------------------------------------------------------
//#define DEBUG_DRAW_BOUNDS

// using Definitions;
// using FGOL;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour, IBroadcastListener
{
	private const float			m_maxTrackAheadScaleX = 0.15f;
    private const float         m_maxTrackAheadScaleY = 0.2f; //JO
	private const float			m_trackBlendRate = 1.0f;
	private const float			m_defaultFOV = 30.0f;
	private const float			m_minZ = 10.0f;
	private const float			m_frameWidthDefault = 20.0f;
	private const float			m_frameWidthBoss = 40.0f; // TEMP boss cam just zooms out
    private const float         m_frameWidthBoost = 32.5f;
	private const float         m_frameWidthFury = 30.0f;
	private const float         m_frameWidthSpace = 40.0f;


    // camera zoom blending values for bosses
    private float               m_zBlendRateBoss = 20.0f;
    private float               m_zDampingRangeBoss = 2.0f;
    private float               m_fovBlendRateBoss = 40.0f;
    private float               m_fovDampingRangeBoss = 5.0f;

	// bosses and how they affect us
	private BossCameraAffector	m_largestBoss = null;
	private float				m_largestBossFrameIncrement = 0.0f;
	private bool				m_frameLargestBossWithPlayer = true;
	private bool				m_isFramed = false;

	// camera zoom add/subtract amount based on size
	private float				m_frameWidthIncrement = 0.0f;
	private float				m_frameWidthDecrement = 0.0f;
	private const float			m_standardSizeSmallest = 1.0f;	// roughly how big a Sardine currently is
	private const float			m_standardSizeLargest = 25.0f; // how big a blue whale is
	private const float 		m_frameWidthIncrementMax = 5.0f;	// the biggest of all creatures will use this increment

    // camera zoom blending values for general play
    private float               m_zBlendRateNormal = 5.0f;
    private float               m_zDampingRangeNormal = 2.0f;
    private float               m_fovBlendRateNormal = 20.0f;
    private float               m_fovDampingRangeNormal = 5.0f;

    // water-line offsets
    private Vector3             m_spaceLineOffset = new Vector3(0,0,0);

	// camera-shake
	private float				m_cameraShake = 0.0f;
	public bool m_useSmoothDamp = true;
	public float m_smoothDampValue = 0.25f;


	public enum TrackAheadMode
    {
        Linear,
        EaseIn
    }
    [SerializeField]
    private float m_maxLookUpOffset = 3.0f;
    [SerializeField]
    private float m_maxLookDownOffset = 3.0f;
    [SerializeField]
    private float m_spaceHeightLookUpMin = -10.0f;
    [SerializeField]
    private float m_spaceHeightLookUpMax = -5.0f;
    [SerializeField]
    private float m_spaceHeightLookDownMin = -5.0f;
    [SerializeField]
    private float m_spaceHeightLookDownMax = 5.0f;
    [SerializeField]
    private float m_rotateLerpDuration = 2.0f;
    [SerializeField]
    private float m_trackAheadScale = 1.5f;
    [SerializeField]
    private TrackAheadMode m_trackAheadMode;

	[Separator("Entity management")]
	[SerializeField] private float m_activationDistance = 10f;
	[SerializeField] private float m_activationRange = 5f;
	[SerializeField] private float m_deactivationDistance = 20f;


    // Camera rotation look at shark
	private float               m_rotateLerpTimer = 0.0f;
    private Vector3             m_trackAheadPos = Vector3.zero;
    private float               m_camDelayLerpT = 0.0f;
    private const float         m_maxLerpDistance = 50.0f;
	private const float 		m_maxRotationAngleX = 22.5f; //JO
    private const float         m_maxRotationAngleY = 20.0f;

	private Transform			m_transform;
	private Camera				m_unityCamera;

	private Vector3 			m_currentPos;
	private Vector3				m_position;

	private Vector3 			m_currentLookAt;
	private Vector3				m_lookAt;

    private Vector3             m_rotation;
	private float				m_fov = m_defaultFOV;						// Vertical FOV in degrees
	private FastBounds2D		m_screenWorldBounds = new FastBounds2D();	// 2D bounds of the camera view at the z=0 plane
	private FastBounds2D		m_backgroundWorldBounds = new FastBounds2D();	// same, at Z for background spawners

	// Camera bounds (from Dragon)
	private FastBounds2D 		m_activationMinNear = new FastBounds2D();
	public FastBounds2D 		activationMinRectNear { get { return m_activationMinNear; }}
	private FastBounds2D 		m_activationMaxNear = new FastBounds2D();
	public FastBounds2D 		activationMaxRectNear { get { return m_activationMaxNear; }}

	private FastBounds2D 		m_activationMinFar = new FastBounds2D();
	public FastBounds2D 		activationMinRectFar { get { return m_activationMinFar; }}
	private FastBounds2D 		m_activationMaxFar = new FastBounds2D();
	public FastBounds2D 		activationMaxRectFar { get { return m_activationMaxFar; }}

	private FastBounds2D 		m_activationMinBG = new FastBounds2D();
	public FastBounds2D 		activationMinRectBG { get { return m_activationMinBG; }}
	private FastBounds2D 		m_activationMaxBG = new FastBounds2D();
	public FastBounds2D 		activationMaxRectBG { get { return m_activationMaxBG; }}

	private FastBounds2D 		m_deactivationNear = new FastBounds2D();
	public FastBounds2D 		deactivationRectNear { get { return m_deactivationNear; }}

	private FastBounds2D 		m_deactivationFar = new FastBounds2D();
	public FastBounds2D 		deactivationRectFar { get { return m_deactivationFar; }}

	private FastBounds2D 		m_deactivationBG = new FastBounds2D();
	public FastBounds2D 		deactivationRectBG { get { return m_deactivationBG; }}


	private const int m_numFrustumPlanes = 6;
	private Plane[] m_frustumPlanes = new Plane[m_numFrustumPlanes];

	private int					m_pixelWidth = 640;
	private int					m_pixelHeight = 480;
	private float				m_pixelAspectX = 1.333f;
	private float				m_pixelAspectY = 0.75f;

	private GameObject			m_targetObject = null;			// this is the object we're currently tracking
	private Transform			m_targetTransform = null;		// this is the target object's cached transform component
	private Vector3 			m_extraTargetDisplacement = GameConstants.Vector3.zero;
	private DragonMotion		m_targetMachine = null;			// this is the target object's Machine component, if it has one (can be null)
	private GameObject			m_queuedTargetObject = null;	// if someone attempts to assign the target on level start before our Awake, queue the request here

	private bool				m_haveBoss = false;
	private bool				m_hasSlowmo = false;
	private float 				m_bossInLerp = 0.0f;
	private float 				m_bossInAngleLerp = 0.0f;
	private Vector3				m_posFrom = Vector3.zero;
	private Vector3				m_posTo = Vector3.zero;

	private bool				m_isLerpingBetweenTargets = false;
	private float				m_positionLerp;
    private float               m_rotationLerp;
    private float               m_expOne;

	private Vector3				m_trackAheadVector = Vector3.zero;
	private bool				m_snap;
	private bool				m_hasInitialized = false;		// this is set once we've done our Awake()
	private bool				m_firstTime = true;				// this is set for the whole of our first frame, and cleared at the end of LateUpdate()

	// properties
	public Vector3				position {get{return m_position;}}
    public Vector3              rotation { get {return m_rotation; }  }
	public FastBounds2D			screenWorldBounds {get{return m_screenWorldBounds;}}
	public FastBounds2D			backgroundWorldBounds {get{return m_backgroundWorldBounds;}}
	public int					pixelWidth {get{return m_pixelWidth;}}
	public int					pixelHeight {get{return m_pixelHeight;}}
	public float				pixelAspectX {get{return m_pixelAspectX;}}
	public float				pixelAspectY {get{return m_pixelAspectY;}}
	public bool					hasInitialized {get{return m_hasInitialized;}}

	private bool				m_averageBossPositions = false;
	private int 				m_prevNumBosses = 0;
	private float 				m_bossLerpRate = 0.5f;

	private List<BossCameraAffector> m_bossCamAffectors = new List<BossCameraAffector>();
/*
	private SlowmoBreachTrigger m_slowmoBreachTrigger = null;
	private SlowmoDeathTrigger m_slowmoDeathTrigger = null;
	// private PostProcessEffectsManager m_postProcessEffectsManager = null;

	public SlowmoBreachTrigger slowmoBreachTrigger { get { return m_slowmoBreachTrigger; } }
	public SlowmoDeathTrigger slowmoDeathTrigger { get { return m_slowmoDeathTrigger; } }
	// public PostProcessEffectsManager postProcessEffectsManager { get { return m_postProcessEffectsManager; } }
*/

	// Camera setup values used on control panel
	private float m_lastSize = 0;
	public float lastSize
	{
		get{ return m_lastSize; }
	}
	private float m_lastFrameWidthModifier = 0;
	public float lastFrameWidthModifier
	{
		get{ return m_lastFrameWidthModifier; }
	}

	private bool m_fury = false;
	public float m_megaFireStartDecrement = 10;


	enum BossCamMode
	{
		NoBoss,
		BossNoFraming,
		BossFramingIn,
		BossFramingOut
	}

	private BossCamMode 		m_bossCamMode;

	private TouchControlsDPad	m_touchControls = null;

	public float m_introDuration = 10.0f;
	private float m_introTimer = 0;
	public float m_introDisplacement = 10;
	private Vector3	m_introPosition;
	public AnimationCurve m_introMoveCurve;
	public AnimationCurve m_introFrameWidthMultiplier;

	enum State
	{
		INTRO,
		INTRO_DONE,
		PLAY
	};
	State m_state = State.INTRO;
	bool m_targetIsDead = false;
	Vector3 m_targetDeadPosition;

	private float m_megaFirePrewarmTimer = 0;
	private float m_megaFirePrewarmDuration = 0;
	public AnimationCurve m_megaFireZoomMultiplier;
	public AnimationCurve m_megaFireTimescaleMultiplier;

	//----------------------------------------------------------------------------

	void Awake() {
		PublicAwake();

		if (FeatureSettingsManager.IsDebugEnabled)
		{
			// gameObject.AddComponent<RenderProfiler>();	// TODO (MALH): Recover this
			Debug_Awake();
		}

		if (FeatureSettingsManager.instance.LevelsLOD <= FeatureSettings.ELevel4Values.low) {
			m_deactivationDistance *= 0.5f;
		}

		InstanceManager.gameCamera = this;

		// Subscribe to external events
		Messenger.AddListener<DragonBreathBehaviour.Type, float>(MessengerEvents.PREWARM_FURY_RUSH, OnFuryPrewarm);
        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
		// Messenger.AddListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);
		// Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoost);
		Messenger.AddListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
		Messenger.AddListener(MessengerEvents.CAMERA_INTRO_DONE, IntroDone);
		Messenger.AddListener<float, float>(MessengerEvents.CAMERA_SHAKE, OnCameraShake);

		Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);



		Messenger.AddListener<Vector2>(MessengerEvents.DEVICE_RESOLUTION_CHANGED, OnResolutionChanged);
	}

	public void PublicAwake()
	{
		m_transform = transform;
		m_unityCamera = GetComponent<Camera>();

		UpdateFrustumPlanes();

		DebugUtils.Assert(m_unityCamera != null, "No Camera");
/*
		m_slowmoBreachTrigger = GetComponentInChildren<SlowmoBreachTrigger>();
		DebugUtils.Assert(m_slowmoBreachTrigger != null, "No SlowmoBreachTrigger");

		m_slowmoDeathTrigger = GetComponentInChildren<SlowmoDeathTrigger>();
		DebugUtils.Assert(m_slowmoDeathTrigger != null, "No SlowmoDeathTrigger" );
*/
		// m_postProcessEffectsManager = GetComponentInChildren<PostProcessEffectsManager>();
		// Assert.Fatal(m_postProcessEffectsManager != null);

		m_snap = true;

		// get screen dimensions and aspect ratio
		UpdatePixelData();

		m_position = m_transform.position;
		m_currentPos = m_position;
		m_currentLookAt = m_lookAt = m_position;
		m_currentLookAt.z = m_lookAt.z = 0;
        m_rotation = m_transform.rotation.eulerAngles;

		m_posFrom = m_posTo = m_transform.position;

		UpdateValues(m_position);
		m_hasInitialized = true;

		// If an attempt was made to assign the target object before this Awake()
		// function was called, we queued it up and it is now safe to assign it.
		if(m_queuedTargetObject != null)
		{
			SetTargetObject(m_queuedTargetObject);
			m_queuedTargetObject = null;
		}

        m_expOne = Mathf.Exp(1.0f);
		m_bossCamMode = BossCamMode.NoBoss;
		m_state = State.INTRO;

        // We can't setup post process effects here because FeatureSettings means to be ready first. Since Gamecamera and FeatureSettings are initialized at the same time when the game is
        // launched from the level editor, we need to synchronize this stuff
        NeedsToSetupPostProcessEffects = true;
    }

	/*
	IEnumerator Start()
	{
		while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
		{
			yield return null;
		}

	}*/

	public void Init(Vector3 _introPos) {
		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null)
		{
			m_touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
		}
		LevelEditor.LevelEditorSceneController editor = InstanceManager.gameSceneControllerBase as LevelEditor.LevelEditorSceneController;
		if ( editor != null )
		{
			if (LevelEditor.LevelEditor.settings.spawnAtCameraPos) {
				SetTargetObject( InstanceManager.player.gameObject );
				m_state = State.PLAY;
			} else {
				if (LevelEditor.LevelEditor.settings.useIntro)
				{
					StartIntro(_introPos);
				}
				else
				{
					m_position = _introPos;
					m_position.z = -m_minZ;	// ensure we pull back some distance, so that we don't screw up the bounds calculations due to plane-ray intersection messing up
					m_transform.position = m_position;

					SetTargetObject( InstanceManager.player.gameObject );
					m_state = State.PLAY;
				}
			}
		}
		else
		{
			StartIntro(_introPos);
		}

		UpdateBounds();
	}

	public void StartIntro(Vector3 _introPos)
	{
		m_position = _introPos;
		m_position.z = -m_minZ;	// ensure we pull back some distance, so that we don't screw up the bounds calculations due to plane-ray intersection messing up
		m_transform.position = m_position;

		SetTargetObject( InstanceManager.player.gameObject );
		m_state = State.INTRO;
		m_introTimer = m_introDuration;
		m_introPosition = m_position;
		m_introDisplacement = InstanceManager.player.dragonMotion.introDisplacement;
		m_position += Vector3.left * m_introDisplacement * m_introMoveCurve.Evaluate(0);
		m_transform.rotation = Quaternion.identity;
	}

	void OnDestroy() {
		Messenger.RemoveListener<DragonBreathBehaviour.Type, float>(MessengerEvents.PREWARM_FURY_RUSH, OnFuryPrewarm);
		Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
		// Messenger.RemoveListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);
		// Messenger.RemoveListener<bool>(GameEvents.BOOST_TOGGLED, OnBoost);
		Messenger.RemoveListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
		Messenger.RemoveListener(MessengerEvents.CAMERA_INTRO_DONE, IntroDone);
		Messenger.RemoveListener<float, float>(MessengerEvents.CAMERA_SHAKE, OnCameraShake);
		Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);

        // Unsubscribe from external events.
		Messenger.RemoveListener<Vector2>(MessengerEvents.DEVICE_RESOLUTION_CHANGED, OnResolutionChanged);

		InstanceManager.gameCamera = null;

        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();
    }
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFury( furyRushToggled.activated, furyRushToggled.type );
            }break;
        }
    }

	public void UpdatePixelData()
	{
		float pw = m_unityCamera.pixelWidth;
		float ph = m_unityCamera.pixelHeight;
		m_pixelWidth = (int)pw;
		m_pixelHeight = (int)ph;
		m_pixelAspectX = pw/ph;
		m_pixelAspectY = ph/pw;
	}

	private void OnResolutionChanged(Vector2 resolution)
	{
		UpdatePixelData ();
	}

	private void OnFury(bool _active, DragonBreathBehaviour.Type _type)
	{
		m_fury = _active;
	}

	private void OnFuryPrewarm(DragonBreathBehaviour.Type _type, float _duration)
	{
		m_megaFirePrewarmTimer = m_megaFirePrewarmDuration = _duration * 2;
        InstanceManager.timeScaleController.StartSlowMotion(m_megaFirePrewarmTimer, m_megaFireTimescaleMultiplier);
	}

    private void CountDownEnded()
	{
		if ( m_state == State.INTRO )
		{
			// m_state = State.INTRO_DONE;
			m_state = State.PLAY;
		}
	}

	private void IntroDone()
	{
		if ( m_state == State.INTRO )
		{
			// m_state = State.INTRO_DONE;
			m_state = State.PLAY;
		}
	}

	private void OnCameraShake( float duration, float intensity)
	{
		SetCameraShake( duration );
	}


    public void SetTargetObject(GameObject obj, bool snap = true)
 	{
		if (obj == null )
	 		return;

 		if(!m_hasInitialized )
		{
			m_queuedTargetObject = obj;
			return;
		}

		// if we have a prefab instance, work out which entity we are, get our 'defaultSize',
		// compare it to the standard size (BlackTip?), apply that ratio to the frame width increment
		DragonPlayer pi = obj.GetComponent<DragonPlayer>();
		if(pi != null)
		{
            float size = pi.data.defaultSize;
			float cameraFrameWidthModifier = pi.data.cameraFrameWidthModifier;
			SetFrameWidthIncrement( size, cameraFrameWidthModifier );
		}
		else
		{
			m_frameWidthIncrement = 0.0f;
		}

		m_snap = snap;

		// if we're not snapping, we need to note the current camera position so
		// we can blend from it
		if(snap)
			m_isLerpingBetweenTargets = false;
		else
		{
			m_posFrom = m_transform.position;
			m_isLerpingBetweenTargets = true;
			m_positionLerp = 0.0f;
		}

		m_targetObject = obj;
		m_targetTransform = obj.transform;
		m_targetMachine = obj.GetComponent<DragonMotion>();

		m_posTo = m_targetTransform.position;

		// When camera target is assigned on the first frame, get the camera into a good position
		// ASAP so things like spawners don't mess up before we get to our first update.
		// TODO: consider just doing this based on m_snap, and get rid of firstTime flag?

		if(m_firstTime)
		{
			// m_position = m_targetTransform.position;
			// m_position.z = -m_minZ;	// ensure we pull back some distance, so that we don't screw up the bounds calculations due to plane-ray intersection messing up

			UpdateValues(m_targetMachine.position);
		}

		m_currentLookAt = m_lookAt = m_position;
		m_currentLookAt.z = m_lookAt.z = 0;
	}

	public void SetFrameWidthIncrement( float size, float cameraFrameWidthModifier )
	{
		m_lastSize = size;
		m_lastFrameWidthModifier = cameraFrameWidthModifier;

		float sizeIncrementRatio = (size - m_standardSizeSmallest) / (m_standardSizeLargest - m_standardSizeSmallest); // 0 -1
		sizeIncrementRatio = Mathf.Clamp01(sizeIncrementRatio);

		m_frameWidthIncrement = Mathf.Lerp(0.0f, m_frameWidthIncrementMax, sizeIncrementRatio);

		// Apply the entity camera modifer after lerping the increment.
		m_frameWidthIncrement += cameraFrameWidthModifier;
	}

	public float GetFrameWidth(float size, float cameraFrameWidthModifier)
	{
		SetFrameWidthIncrement(size, cameraFrameWidthModifier);
		return m_frameWidthDefault + m_frameWidthIncrement;
	}

	public bool IsTarget(GameObject obj)
	{
		return (m_targetObject == obj);
	}

	public bool IsTarget(DragonMotion machine)
	{
		return (m_targetMachine == machine);
	}

	public void Snap(bool _value = true)
	{
		m_snap = _value;
	}

	public void NotifyBoss(BossCameraAffector bca)
	{
		// safety check to avoid duplicates (it should not happen anyway).
		if(!m_bossCamAffectors.Contains(bca))
		{
			if(bca.frameWidthIncrement > m_largestBossFrameIncrement || m_largestBoss == null)
			{
				m_largestBoss = bca;
				m_largestBossFrameIncrement = bca.frameWidthIncrement;
				m_frameLargestBossWithPlayer = bca.frameMeAndPlayer;
			}
			m_prevNumBosses = m_bossCamAffectors.Count;
			m_haveBoss = true;
			m_bossCamAffectors.Add(bca);
		}
	}

	public void RemoveBoss(BossCameraAffector bca)
	{
		if(m_bossCamAffectors.Count > 0 && m_bossCamAffectors.Contains(bca))
		{
			m_prevNumBosses = m_bossCamAffectors.Count;
			m_bossCamAffectors.Remove(bca);
			// update the largest boss data if necessary.
			if(m_largestBoss == bca)
			{
				if(m_bossCamAffectors.Count > 0)
				{
					m_largestBoss = m_bossCamAffectors[0];
					m_largestBossFrameIncrement = m_bossCamAffectors[0].frameWidthIncrement;
					m_frameLargestBossWithPlayer = m_bossCamAffectors[0].frameMeAndPlayer;

					for(int i = 1; i < m_bossCamAffectors.Count; i++)
					{
						if(m_bossCamAffectors[i].frameWidthIncrement > m_largestBossFrameIncrement)
						{
							m_largestBoss = m_bossCamAffectors[i];
							m_largestBossFrameIncrement = m_bossCamAffectors[i].frameWidthIncrement;
							m_frameLargestBossWithPlayer = m_bossCamAffectors[i].frameMeAndPlayer;
						}
					}
				}
				else
				{
					m_largestBoss = null;
					m_largestBossFrameIncrement = 0;
					m_frameLargestBossWithPlayer = false;
				}
			}
			// update the other boss related variables.
			if(m_bossCamAffectors.Count == 0)
			{
				m_haveBoss = false;
			}
		}
	}

	public void OnDirectionChange()
	{
		m_rotateLerpTimer = 0.0f;
	}

	void LateUpdate()
	{

		PlayUpdate();

        if (NeedsToSetupPostProcessEffects && FeatureSettingsManager.IsReady())
        {
            SetupPostProcessEffects();
        }

		/*
		switch( m_state )
		{
			case State.INTRO:
			{
			}break;
			case State.PLAY:
			{

			}break;
		}
		*/

		UpdateFrustumPlanes();
	}

	private void UpdateFrustumPlanes()
	{
		//m_frustumPlanes = GeometryUtility.CalculateFrustumPlanes(m_unityCamera);
		CalculateFrustumPlanes( m_frustumPlanes, m_unityCamera.projectionMatrix * m_unityCamera.worldToCameraMatrix);
	}

	private static System.Action<Plane[], Matrix4x4> _calculateFrustumPlanes_Imp;
    public static void CalculateFrustumPlanes(Plane[] planes, Matrix4x4 worldToProjectMatrix)
    {
        if (planes == null) throw new System.ArgumentNullException("planes");
        if (planes.Length < 6) throw new System.ArgumentException("Output array must be at least 6 in length.", "planes");

        if (_calculateFrustumPlanes_Imp == null)
        {
            var meth = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new System.Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);
            if (meth == null) throw new System.Exception("Failed to reflect internal method. Your Unity version may not contain the presumed named method in GeometryUtility.");

            _calculateFrustumPlanes_Imp = System.Delegate.CreateDelegate(typeof(System.Action<Plane[], Matrix4x4>), meth) as System.Action<Plane[], Matrix4x4>;
            if(_calculateFrustumPlanes_Imp == null) throw new System.Exception("Failed to reflect internal method. Your Unity version may not contain the presumed named method in GeometryUtility.");
        }

        _calculateFrustumPlanes_Imp(planes, worldToProjectMatrix);
    }



	bool PlayingIntro()
	{
		return m_introTimer > 0;
	}

	void PlayUpdate()
	{
        if (InstanceManager.gameSceneControllerBase.paused)
            return;
		float dt = Time.deltaTime;
		Vector3 targetPosition;

		if ( m_introTimer <= 0 ){
			if ( m_targetObject == null )
			{
				targetPosition = m_position;
			}
			else if ( m_targetIsDead )
			{
				targetPosition = m_targetDeadPosition;
			}
			else
			{
				targetPosition = m_targetTransform.position;
				Vector3 vel = GameConstants.Vector3.zero;
				if ( m_fury )
				{
					if ( m_useSmoothDamp )
						m_extraTargetDisplacement = Vector3.SmoothDamp( m_extraTargetDisplacement, m_targetMachine.direction * InstanceManager.player.breathBehaviour.actualLength * 0.3f, ref vel, m_smoothDampValue);
					else
						m_extraTargetDisplacement = Vector3.Lerp( m_extraTargetDisplacement, m_targetMachine.direction * InstanceManager.player.breathBehaviour.actualLength * 0.3f, Time.deltaTime * 2);
				}
				else
				{
					if ( m_useSmoothDamp )
						m_extraTargetDisplacement = Vector3.SmoothDamp( m_extraTargetDisplacement, GameConstants.Vector3.zero, ref vel, m_smoothDampValue);
					else
						m_extraTargetDisplacement = Vector3.Lerp( m_extraTargetDisplacement, GameConstants.Vector3.zero, Time.deltaTime * 2);
				}
				targetPosition += m_extraTargetDisplacement;
				UpdateTrackAheadVector(m_targetMachine);
			}
		}else{
			m_introTimer -= Time.deltaTime;
			float delta = m_introTimer / m_introDuration;
			m_trackAheadVector = Vector3.zero;
			float displacement = m_introMoveCurve.Evaluate(1.0f-delta) * m_introDisplacement;
			targetPosition = m_introPosition + (Vector3.left * displacement);

			if ( m_introTimer <= 0 )
			{
				m_posFrom = m_transform.position;
				m_isLerpingBetweenTargets = true;
				m_positionLerp = 0.0f;

				m_trackAheadPos = m_position;
				m_trackAheadPos.z = 0;
			}
		}

		Vector3 desiredPos = targetPosition - m_trackAheadVector;

        // space-line camera position offsetiness
        if(m_targetObject != null)
        {
            UpdateSpaceLevelOffset ();
            desiredPos += m_spaceLineOffset;
        }

		// If we just changed target and are not snapping, we fairly quickly lerp to our new position
		// DB: Bypass this if we are being affected by boss cameras
        if(m_isLerpingBetweenTargets)
        {
			float lerpRate = 2.0f;

            m_position 		= Vector3.Lerp(m_posFrom, desiredPos, m_positionLerp);
            m_positionLerp += lerpRate * dt;

            if(m_positionLerp >= 1.0f)
            {
                m_isLerpingBetweenTargets = false;
				// DO STUFF HERE!
				m_bossCamMode = BossCamMode.NoBoss;
//				m_bossInLerp = 0.0f;
//				m_bossInAngleLerp = 0.0f;
            }
        }
        else
        {
			UpdatePosition(desiredPos);

			// SAFETY CATCH
			//special case - if the camera is more than a certain distance from the shark, don't lerp. snap it instead
			if((desiredPos - m_position).sqrMagnitude > m_maxLerpDistance * m_maxLerpDistance)
			{
				m_position = desiredPos;
			}

			if( m_touchControls != null && m_touchControls.directionChanged)	// have we changed direction in this Update()
			{
				m_rotateLerpTimer = 0.0f;
			}

        }

		float frameWidth = m_frameWidthDefault;

		if ( PlayingIntro() )
		{
			frameWidth += m_frameWidthIncrement;
			frameWidth *= m_introFrameWidthMultiplier.Evaluate( 1.0f - (m_introTimer/m_introDuration) );
			m_snap = true;
			UpdateZooming(frameWidth, false);
		}
		else
		{
            bool hasBoss = HasBoss();


            if ( m_megaFirePrewarmTimer > 0 )
            {
            	float delta = 1.0f - m_megaFirePrewarmTimer / m_megaFirePrewarmDuration;
				frameWidth = m_megaFireZoomMultiplier.Evaluate( delta ) * m_frameWidthFury;
				// Time.timeScale = m_megaFireTimescaleMultiplier.Evaluate( delta );
            	m_megaFirePrewarmTimer -= Time.unscaledDeltaTime;
                /*
				if (m_megaFirePrewarmTimer <= 0 )
				{
					Time.timeScale = 1;
				}
                */
            }
            else
            {
				if (m_targetMachine != null)
		        {
							if(!hasBoss)
						 {
				 if (targetPosition.y > DragonMotion.SpaceStart)
				 {
					 frameWidth = m_frameWidthSpace;
				 }
				 else if ( m_fury )
							 {
								 frameWidth = m_frameWidthFury;
							 }
							 else
							 {
					 //frameWidth = Mathf.Lerp(m_frameWidthDefault, m_frameWidthBoost, m_targetMachine.howFast);
							//TONI: Testing, instead of linear, cubic interpolation.
							frameWidth = m_frameWidthDefault + ((m_frameWidthBoost - m_frameWidthDefault) * m_targetMachine.howFast * m_targetMachine.howFast * m_targetMachine.howFast);
				 }
						 }
		        }
				frameWidth += m_frameWidthIncrement;
				if(m_hasSlowmo)
				{
					frameWidth -= m_frameWidthDecrement;
				}
				else if(hasBoss)
				{
					frameWidth += m_largestBossFrameIncrement;
				}
            }
				
			UpdateZooming(frameWidth, hasBoss);
		}


		UpdateCameraShake();
		UpdateValues(targetPosition);

		m_snap = false;
		m_firstTime = false;

		m_prevNumBosses = m_bossCamAffectors.Count;

#if DEBUG_DRAW_BOUNDS
		DebugDraw.DrawBounds2D(m_screenWorldBounds);
#endif
	}

	// Also called DampIIR (wiki search ...)
	float Damping(float src, float dst, float dt, float factor)
	{
	    return (((src * factor) + (dst * dt)) / (factor + dt));
	}



	void UpdateSpaceLevelOffset()
	{
        float y = m_targetObject.transform.position.y - DragonMotion.SpaceStart;
		float halfHeight = (m_spaceHeightLookUpMax + m_spaceHeightLookDownMin) / 2.0f;

		if (y < m_spaceHeightLookUpMin)
		{
			// do nothing
			m_spaceLineOffset.y = 0.0f;
		}
		else if ((y >= m_spaceHeightLookUpMin) && (y < m_spaceHeightLookUpMax))
		{
			float ratio = (y - m_spaceHeightLookUpMin) / (m_spaceHeightLookUpMax - m_spaceHeightLookUpMin);
			m_spaceLineOffset.y = -(ratio * m_maxLookUpOffset);
		}
		else if ((y >= m_spaceHeightLookUpMax) && (y < halfHeight))
		{
			// first half in between looking up and down
			float ratio = 1.0f - (y - m_spaceHeightLookUpMax) / (halfHeight - m_spaceHeightLookUpMax);
			m_spaceLineOffset.y = -(ratio * m_maxLookUpOffset);
		}
		else if ((y >= halfHeight) && (y < m_spaceHeightLookDownMin))
		{
			// second half in between looking up and down
			float ratio = (y - halfHeight) / (m_spaceHeightLookDownMin - halfHeight);
			m_spaceLineOffset.y = ratio * m_maxLookDownOffset;
		}
		else if ((y >= m_spaceHeightLookDownMin) && (y < m_spaceHeightLookDownMax))
		{
			float ratio = 1.0f - (y - m_spaceHeightLookDownMin) / (m_spaceHeightLookDownMax - m_spaceHeightLookDownMin);
			m_spaceLineOffset.y = (ratio * m_maxLookDownOffset);
		}
		else if (y > m_spaceHeightLookDownMax)
		{
			// do nothing
			m_spaceLineOffset.y = 0.0f;
		}
	}

	void UpdateCameraDelayLerp()
	{
		// update the camera delay lerp
		m_rotateLerpTimer += Time.deltaTime;
		m_camDelayLerpT = m_rotateLerpTimer / m_rotateLerpDuration; // 0 - 1
		m_camDelayLerpT = Mathf.Clamp01 (m_camDelayLerpT);

		if (m_trackAheadMode == TrackAheadMode.EaseIn)
		{
			// modulate t on an exponential curve
			m_camDelayLerpT = Mathf.Exp (m_camDelayLerpT); // 1 - 2.something

			// re-normalize
			m_camDelayLerpT = (m_camDelayLerpT - 1.0f) / (m_expOne - 1.0f);
			m_camDelayLerpT = Mathf.Clamp01 (m_camDelayLerpT);
		}
	}

	void CheckForBossCamMode()
	{
		if(HasBoss())
		{
			// we've got a boss, see if we just acquired a boss (or gained another)
			if (m_bossCamAffectors.Count > m_prevNumBosses)
			{
				// does the largest boss need framing?
				if ((!m_frameLargestBossWithPlayer) || (m_bossCamAffectors.Count > 1))	// if you've got multiple bosses, don't frame them with player, just center on player
				{
					m_bossCamMode = BossCamMode.BossNoFraming;
					m_camDelayLerpT = 0.0f;
					m_rotateLerpTimer = 0.0f;
				}
				else
				{
					m_bossCamMode = BossCamMode.BossFramingIn;
					m_isFramed = true;
					m_bossInLerp = 0;
				}
			}
		}
		else
		{
			// we don't have a boss, see if we just lost a boss
			if (m_bossCamAffectors.Count < m_prevNumBosses)
			{
				// where we in framed boss mode?
				if (m_bossCamMode == BossCamMode.BossFramingIn || m_isFramed)
				{
					m_bossCamMode = BossCamMode.BossFramingOut;
					m_bossInLerp = 1.0f;
					m_isFramed = false;
				}
				else
				{
					m_bossCamMode = BossCamMode.NoBoss;
				}
			}
			// Make sure not to interupt frame out as it resets the lerp value using easing rather than reset them at once which will cause in camera rotation jump
			else if (m_bossCamMode != BossCamMode.BossFramingOut)
			{
				m_bossCamMode = BossCamMode.NoBoss;
			}
		}
	}

	void UpdatePosition(Vector3 desiredPos)
	{
		CheckForBossCamMode ();

		switch(m_bossCamMode)
		{
			case BossCamMode.NoBoss:
			case BossCamMode.BossNoFraming:
			{
				UpdateCameraDelayLerp ();

				// we're in regular mode (or the boss doesn't need framing)
				m_position = Vector3.Lerp(m_position, desiredPos, m_camDelayLerpT);
			}
			break;
			case BossCamMode.BossFramingIn:
			{
				// work out center between target and boss
				Vector3 targetPos = m_targetObject.transform.position + (m_trackAheadVector * m_trackAheadScale);

				// get average position of all current bosses
				Vector3 sumPos = Vector3.zero;
				int numBossesActive = 0;
				for(int i = 0; i < m_bossCamAffectors.Count; i++)
				{
					BossCameraAffector boss = m_bossCamAffectors[i];

					if(boss != null)
					{
						sumPos += boss.transform.position;
						numBossesActive++;
					}
				}

				// sometimes the boss gets destroyed halfway through this, so check and deal with it..
				if(numBossesActive == 0)
				{
					m_haveBoss = false;
					return;
				}

				Vector3 bossPos = m_averageBossPositions ? sumPos / numBossesActive : m_largestBoss.transform.position;
				Vector3 centeredPos = (targetPos + bossPos) * 0.5f;

				// lerp
				m_posFrom = m_transform.position;
				m_posTo = centeredPos;

				m_bossInLerp += m_bossLerpRate * Time.deltaTime;
				m_bossInLerp = Mathf.Clamp01(m_bossInLerp);

				m_bossInAngleLerp += m_bossLerpRate * Time.deltaTime;
				m_bossInAngleLerp = Mathf.Clamp01(m_bossInAngleLerp);

				m_position = Vector3.Lerp(m_posFrom, m_posTo , m_bossInLerp);
			}
			break;
			case BossCamMode.BossFramingOut:
			{
				// lerp
				m_posTo = m_targetObject.transform.position;

				m_posFrom = m_transform.position;

				m_bossInLerp -= m_bossLerpRate * Time.deltaTime;
				m_bossInLerp = Mathf.Clamp01(m_bossInLerp);

				m_bossInAngleLerp -= m_bossLerpRate * Time.deltaTime;
				m_bossInAngleLerp = Mathf.Clamp01(m_bossInAngleLerp);

				if(m_bossInLerp > 0.0f)
				{
					// we're coming out of boss mode
					m_position = Vector3.Lerp(m_posFrom, m_posTo, 1.0f - m_bossInLerp);
				}
				else
				{
					// done lerping out
					m_camDelayLerpT = 0.0f;
					m_rotateLerpTimer = 0.0f;
					m_bossCamMode = BossCamMode.NoBoss;
				}
			}
			break;
		}
	}

	private void UpdateTrackAheadVector(DragonMotion machine)
	{
		if(machine == null)
		{
			m_trackAheadVector = Vector3.zero;
			return;
		}

		float dt = Time.deltaTime;
		float trackAheadRangeX = m_screenWorldBounds.w * m_maxTrackAheadScaleX;	// todo: have maxTrackAheadScale account for size of target?
		float trackAheadRangeY = m_screenWorldBounds.h * m_maxTrackAheadScaleY;
		float trackBlendRate = trackAheadRangeX * m_trackBlendRate;

		Vector3 desiredTrackAhead = machine.velocity / machine.absoluteMaxSpeed;

		desiredTrackAhead.x *= trackAheadRangeX;
		desiredTrackAhead.y *= trackAheadRangeY;
		if(m_snap)
			m_trackAheadVector = desiredTrackAhead;
		else
			Util.MoveTowardsVector3XYWithDamping(ref m_trackAheadVector, ref desiredTrackAhead, trackBlendRate*dt, 1.0f);
		//UnityEngine.Debug.Log("Ahead Vector" + m_trackAheadVector);
	}

	// Zooming in and out is done by specifying the desired width of the frame, i.e. how wide is the visible frame in metres at the z=0 plane?
	// We zoom in and out by animating Z position, but at close range we zoom in by animating FOV instead.
	public void UpdateZooming(float desiredFrameWidth, bool bossZoom)
	{
        // deal with frame height and vertical FOV, as unity camera uses vertical FOV.
        float desiredFrameHeight = desiredFrameWidth * m_pixelAspectY;

		// figure out what Z distance the camera would need to be at to get the desired
		// frame width at the default FOV
		float fov = m_defaultFOV;
		float hh = desiredFrameHeight * 0.5f;	// half height
		float ratio = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
		float z = hh / ratio;

		// if this Z distance is too close, instead figure out what FOV we would need to
		// get the desired frame width at the fixed minimum Z distance
		if(z < m_minZ)
		{
			ratio = hh / m_minZ;
			fov = Mathf.Atan(ratio) * Mathf.Rad2Deg * 2.0f;
			z = m_minZ;
		}

		// blend final Z and FOV values.
		// Testing doing it this way instead of blending input value (desiredFrameWidth) and snapping these values, so we
		// avoid Z pos appearing to accelerate/decelerate in an unnatural way
		float dt = Time.deltaTime;
		if(m_snap)
		{
			m_position.z = -z;
			m_fov = fov;
		}
		else
		{
            float zBlendRate = bossZoom ? m_zBlendRateBoss : m_zBlendRateNormal;
            float zDampingRange = bossZoom ? m_zDampingRangeBoss : m_zDampingRangeNormal;
            float fovBlendRate = bossZoom ? m_fovBlendRateBoss : m_fovBlendRateNormal;
            float fovDampingRange = bossZoom ? m_fovDampingRangeBoss : m_fovDampingRangeNormal;

			m_position.z = Util.MoveTowardsWithDamping(m_transform.position.z, -z, zBlendRate*dt, zDampingRange);
			m_fov = Util.MoveTowardsWithDamping(m_fov, fov, fovBlendRate*dt, fovDampingRange);
		}
	}

	public void SetCameraShake(float value)
	{
		if (value > m_cameraShake)
			m_cameraShake = value;
	}

	private void UpdateCameraShake()
	{
		if (m_cameraShake > 0.0f)
		{
			m_cameraShake -= Time.deltaTime;

			if (m_cameraShake < 0.0f)
				m_cameraShake = 0.0f;
		}
	}

	// This should be called after all camera movement/zooming logic has finished, to set the final position, culling bounds etc.
	// from m_position and m_fov.
	private void UpdateValues( Vector3 targetPosition )
	{
		if ( m_position.y > DragonMotion.FlightCeiling - m_screenWorldBounds.h )
		{

			// 1.-
			// m_position.y = DragonMotion.FlightCeiling - m_screenWorldBounds.h / 2.0f;

			// 2.-
			// float delta = 1.0f - Mathf.Clamp01((DragonMotion.FlightCeiling - m_position.y) / m_screenWorldBounds.h);
			// m_position.y = Mathf.Lerp( m_position.y, DragonMotion.FlightCeiling - (m_screenWorldBounds.h / 2.0f), delta);

			// 3.-
			float delta = Mathf.Clamp01( (m_position.y - (DragonMotion.FlightCeiling - m_screenWorldBounds.h)) / (m_screenWorldBounds.h) );
			m_position.y = Mathf.Lerp( DragonMotion.FlightCeiling - m_screenWorldBounds.h , DragonMotion.FlightCeiling - (m_screenWorldBounds.h / 2.0f) , delta);
		}

		m_transform.position = m_position;
		if ( Time.timeScale > 0 )
			m_transform.position += Random.insideUnitSphere * m_cameraShake;

		if((m_targetTransform != null) && !PlayingIntro())
		{
			Vector3 targetTrackAhead = m_trackAheadVector * m_trackAheadScale;
			Vector3 targetTrackPos =  targetPosition + targetTrackAhead;
			targetTrackPos.z = 0;
			if ( targetTrackPos.y > DragonMotion.FlightCeiling - m_screenWorldBounds.h )
			{
				// 2.-
				// float delta = 1.0f - Mathf.Clamp01((DragonMotion.FlightCeiling - targetTrackPos.y) / m_screenWorldBounds.h);
				// targetTrackPos.y = Mathf.Lerp( targetTrackPos.y, DragonMotion.FlightCeiling - (m_screenWorldBounds.h / 2.0f), delta);

				// 3.-
				float delta = Mathf.Clamp01( (targetTrackPos.y - (DragonMotion.FlightCeiling - m_screenWorldBounds.h)) / (m_screenWorldBounds.h) );
				targetTrackPos.y = Mathf.Lerp( DragonMotion.FlightCeiling - m_screenWorldBounds.h , DragonMotion.FlightCeiling - (m_screenWorldBounds.h / 2.0f) , delta);
			}

			if(m_isLerpingBetweenTargets)
			{
				m_trackAheadPos = Vector3.Lerp(m_trackAheadPos, targetTrackPos, m_positionLerp);
			}
			else
			{
				// SAFETY CATCH
				if((m_trackAheadPos - targetTrackPos).sqrMagnitude > m_maxLerpDistance * m_maxLerpDistance)
				{
					m_trackAheadPos = targetTrackPos;
				}
				else
				{
					m_trackAheadPos = Vector3.Lerp(m_trackAheadPos, targetTrackPos, m_camDelayLerpT);
				}
			}

			Vector3 currentPos = m_position;
			currentPos.z = m_trackAheadPos.z;

			Vector3 lookAtPos = Vector3.Lerp(m_trackAheadPos, currentPos, m_bossInAngleLerp);
			m_transform.LookAt(lookAtPos);

            // clamp the rotation at a maximum of 30 degrees either way
            Vector3 rot = m_transform.rotation.eulerAngles;
			if(rot.y > 180.0f)
			{
				rot.y = rot.y - 360.0f;
			}
			if(rot.x > 180.0f)
			{
				rot.x = rot.x - 360.0f;
			}
			rot.y = Mathf.Clamp(rot.y, -m_maxRotationAngleY, m_maxRotationAngleY);
			rot.x = Mathf.Clamp(rot.x, -m_maxRotationAngleX, m_maxRotationAngleX);

            m_transform.rotation = Quaternion.Euler(rot);
		}
		UpdateBounds();

	}

    private Ray[] m_cameraRays = new Ray[4];
    private Vector3[] m_cameraPts = new Vector3[4];
    private FastBounds2D[] m_bounds = new FastBounds2D[2];
    private float[] m_depth = new float[2];

	public void UpdateFOV() {
		m_unityCamera.fieldOfView = m_fov;
	}

	void UpdateBounds() {
		float z = -m_position.z;

		UpdateFOV();

		// Now that we tilt the camera a bit, need to modify how it gets the world bounds
        m_cameraRays[0] = m_unityCamera.ScreenPointToRay(new Vector3(0.0f, 0.0f, z));
        m_cameraRays[1] = m_unityCamera.ScreenPointToRay(new Vector3(m_unityCamera.pixelWidth, 0.0f, z));
        m_cameraRays[2] = m_unityCamera.ScreenPointToRay(new Vector3(m_unityCamera.pixelWidth, m_unityCamera.pixelHeight, z));
        m_cameraRays[3] = m_unityCamera.ScreenPointToRay(new Vector3(0.0f, m_unityCamera.pixelHeight, z));

        // generate two world bounds, one for z=0, one for background spawners
        m_depth[0] = 0f;
        m_depth[1] = SpawnerManager.BACKGROUND_LAYER_Z;
        m_bounds[0] = m_screenWorldBounds;
        m_bounds[1] = m_backgroundWorldBounds;

		for (int j = 0; j < m_depth.Length; j++) {
			Plane plane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, m_depth[j]));

			for (int i=0; i<4; i++) {
				Vector3? intersect = Util.RayPlaneIntersect(m_cameraRays[i], plane);
				if(intersect != null) {
                    m_cameraPts[i] = (Vector3)intersect;
					if(i == 0)  // initialize bounds with first point and zero size
                        m_bounds[j].Set(m_cameraPts[i].x, m_cameraPts[i].y, 0.0f, 0.0f);
					else
                        m_bounds[j].Encapsulate(ref m_cameraPts[i]);
				}
			}

			#if DEBUG_DRAW_BOUNDS
			DebugDraw.DrawLine(pts[0], pts[1]);
			DebugDraw.DrawLine(pts[1], pts[2]);
			DebugDraw.DrawLine(pts[2], pts[3]);
			DebugDraw.DrawLine(pts[3], pts[0]);
			#endif
		}

		float expand = 0;

		expand = m_activationDistance;
		m_activationMinNear.Set( m_screenWorldBounds );
		m_activationMinNear.ExpandBy(expand, expand);
		m_activationMinNear.ExpandBy(-expand, -expand);

		m_activationMinFar.Set(m_activationMinNear);
		m_activationMinFar.ApplyScale(1.5f);

		m_activationMinBG.Set(m_backgroundWorldBounds);
		m_activationMinBG.ExpandBy(expand, expand);
		m_activationMinBG.ExpandBy(-expand, -expand);


		expand = m_activationDistance + m_activationRange;
		m_activationMaxNear.Set( m_screenWorldBounds );
		m_activationMaxNear.ExpandBy( expand, expand );
		m_activationMaxNear.ExpandBy( -expand, -expand );

		m_activationMaxFar.Set(m_activationMaxNear);
		m_activationMaxFar.ApplyScale(1.5f);

		m_activationMaxBG.Set(m_backgroundWorldBounds);
		m_activationMaxBG.ExpandBy(expand, expand);
		m_activationMaxBG.ExpandBy(-expand, -expand);


		expand = m_deactivationDistance;
		m_deactivationNear.Set( m_screenWorldBounds );
		m_deactivationNear.ExpandBy( expand, expand );
		m_deactivationNear.ExpandBy( -expand, -expand );

		m_deactivationFar.Set(m_deactivationNear);
		m_deactivationFar.ApplyScale(1.5f);

		m_deactivationBG.Set(m_activationMaxBG);
		m_deactivationBG.ExpandBy( 2f, 2f );
		m_deactivationBG.ExpandBy( -2f, -2f );
	}

	// On-screen tests.  In Hungry Dragons, these were static for performance.
	// Going to try these non-static for now in case somehow we end up needing multiple cameras.  And when we inevitably don't, and are
	// trying desperately to improve framerate at the end of the project, they can be put back to static.
	// The same goes for having screenWorldBounds as a public static variable.
	public bool IsPointOnScreen2D(Vector3 pos) {
		return m_screenWorldBounds.Contains(ref pos);
	}

	public bool IsPointOnScreen2D(ref Vector3 pos) {
		return m_screenWorldBounds.Contains(pos);
	}

	public bool IsPointOnScreen2D(float x, float y) {
		return m_screenWorldBounds.Contains(x, y);
	}


	// Same tests from Dragon Camera
	public bool IsInsideActivationMinArea(Vector3 _point) {
		return m_activationMinNear.Contains(_point);
	}

	public bool IsInsideActivationMinArea(Bounds _bounds) {
		return m_activationMinNear.Intersects(_bounds);
	}

	public bool IsInsideActivationMaxArea(Vector3 _point) {
		return m_activationMaxNear.Contains(_point);
	}

	public bool IsInsideActivationMaxArea(Bounds _bounds) {
		return m_activationMaxNear.Intersects(_bounds);
	}

	public bool IsInsideActivationMaxArea(Rect _bounds) {
		return m_activationMaxNear.Intersects(_bounds);
	}

	public bool IsInsideActivationArea(Vector3 _point) {
		return !m_activationMinNear.Contains(_point) && m_activationMaxNear.Contains(_point);
	}

	public bool IsInsideActivationArea(Bounds _bounds) {
		return !m_activationMinNear.Intersects(_bounds) && m_activationMaxNear.Intersects(_bounds);
	}

	public bool IsInsideBackgroundActivationArea(Vector3 _point) {
		return !m_activationMinBG.Contains(_point) && m_activationMaxBG.Contains(_point);
	}

	public bool IsInsideBackgroundActivationArea(Bounds _bounds) {
		return !m_activationMinBG.Intersects(_bounds) && m_activationMaxBG.Intersects(_bounds);
	}


	//
	public bool IsInsideDeactivationArea(Vector3 _point) {
		if (_point.z < SpawnerManager.FAR_LAYER_Z) {
			return !m_deactivationNear.Contains(_point);
		} else {
			return !m_deactivationFar.Contains(_point);
		}
	}

	public bool IsInsideDeactivationArea(Bounds _bounds) {
		if (_bounds.center.z < SpawnerManager.FAR_LAYER_Z) {
			return !m_deactivationNear.Intersects(_bounds);
		} else {
			return !m_deactivationFar.Intersects(_bounds);
		}
	}

	public bool IsInsideDeactivationArea(Rect _bounds) {
		return !m_deactivationNear.Intersects(_bounds);
	}
	//

	//
	public bool IsInsideDeactivationAreaFar(Rect _bounds) {
		return !m_deactivationFar.Intersects(_bounds);
	}
	//

	//
	public bool IsInsideBackgroundDeactivationArea(Vector3 _point) {
		return !m_deactivationBG.Contains(_point);
	}

	public bool IsInsideBackgroundDeactivationArea(Bounds _bounds) {
		return !m_deactivationBG.Intersects(_bounds);
	}

	public bool IsInsideBackgroundDeactivationArea(Rect _bounds) {
		return !m_deactivationBG.Intersects(_bounds);
	}
	//


	public bool IsInside2dFrustrum(Vector3 _point) {
		return m_screenWorldBounds.Contains(_point);
	}

	public bool IsInside2dFrustrum(Bounds _bounds) {
		return m_screenWorldBounds.Intersects(_bounds);
	}

	public bool IsInsideCameraFrustrum(Vector3 _p) {
		Bounds b = new Bounds(_p, Vector3.one);
		return GeometryUtility.TestPlanesAABB(m_frustumPlanes, b);
		/*
		for (int i = 0; i < m_numFrustumPlanes; ++i) {
			if (m_frustumPlanes[i].GetSide(_p)) return false;
		}
		return true;*/
	}

	public bool IsInsideCameraFrustrum(Bounds _bounds) {
		return GeometryUtility.TestPlanesAABB(m_frustumPlanes, _bounds);
	}

    private bool HasBoss() {
        bool returnValue = m_haveBoss;
        if (returnValue) {
            // Check if the feature is enabled
            returnValue = FeatureSettingsManager.instance.IsBossZoomOutEnabled;
        }

        return returnValue;
    }

	public void NotifySlowmoActivation(bool active, float frameWidthDecrement = 0f)
	{
		if(active) {
			m_frameWidthDecrement = frameWidthDecrement;
		} else {
			m_frameWidthDecrement = 0f;
		}
		m_hasSlowmo = active;
	}

    //------------------------------------------------------------------//
    // Debug															//
    //------------------------------------------------------------------//
    void OnDrawGizmos() {
		if (!Application.isPlaying) {
			if (m_unityCamera == null )
				m_unityCamera = GetComponent<Camera>();
			UpdateBounds();
		}

		if (enabled) {
			Vector3 center;
			Vector3 size;

			Gizmos.color = Color.yellow;
			m_screenWorldBounds.GetCentre( out center );
			m_screenWorldBounds.GetSize(out size);
			Gizmos.DrawWireCube(center, size);

			Gizmos.color = Color.cyan;
			m_activationMinNear.GetCentre( out center );
			m_activationMinNear.GetSize(out size);
			Gizmos.DrawWireCube(center, size);
			m_activationMaxNear.GetCentre( out center );
			m_activationMaxNear.GetSize(out size);
			Gizmos.DrawWireCube(center, size);

			Gizmos.color = Color.magenta;
			m_deactivationNear.GetCentre( out center );
			m_deactivationNear.GetSize(out size);
			Gizmos.DrawWireCube(center, size);


			Gizmos.color = Colors.WithAlpha(Color.cyan, 0.5f);
			m_activationMinBG.GetCentre( out center );
			m_activationMinBG.GetSize(out size);
			Gizmos.DrawWireCube(center, size);
			m_activationMaxBG.GetCentre( out center );
			m_activationMaxBG.GetSize(out size);
			Gizmos.DrawWireCube(center, size);

			Gizmos.color = Colors.WithAlpha(Color.magenta, 0.5f);
			m_deactivationBG.GetCentre( out center );
			m_deactivationBG.GetSize(out size);
			Gizmos.DrawWireCube(center, size);
		}
	}

    private bool NeedsToSetupPostProcessEffects { get; set; }

    private void SetupPostProcessEffects()
    {
        NeedsToSetupPostProcessEffects = false;

        SetupFrameColorEffect();
    }


    private void SetupFrameColorEffect()
    {
        // The effect is enabled if the feature is enabled for this device
        FrameColoring effect = GetComponent<FrameColoring>();
        if (effect != null)
        {
            effect.enabled = FeatureSettingsManager.instance.IsFrameColorEffectEnabled;
        }
    }

	private void OnPlayerRevive(DragonPlayer.ReviveReason _reason)
	{
		m_targetIsDead = false;

	}

	private void OnPlayerKo( DamageType _type, Transform _tr)
	{
		m_targetIsDead = true;
		m_targetDeadPosition = m_targetTransform.position;
	}

    #region debug
    // This region is responsible for enabling/disabling the glow effect for profiling purposes. This code is placed here because GlowEffect is a third-party code so
    // we don't want to change it if it's not really necessary in order to make future updates easier
    private void Debug_Awake()
    {
        Messenger.AddListener(MessengerEvents.CP_QUALITY_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnDestroy()
    {
        Messenger.RemoveListener(MessengerEvents.CP_QUALITY_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged()
    {
        NeedsToSetupPostProcessEffects = true;
    }
    #endregion
}
